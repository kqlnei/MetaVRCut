using UnityEngine;

public class FlickOff : MonoBehaviour
{
    private Animator anim;
    private Transform parentTransform;
    private Knockback knockbackScript;
    private Rigidbody rb;

    [Header("Knockback Settings")]
    [Tooltip("ノックバックの力")]
    public float forceAmount = 10f;

    [Tooltip("ノックバック時の方向")]
    public Vector3 knockbackDirection = new Vector3(0, 1, 1);

    [Header("Rotation Settings")]
    [Tooltip("回転速度 (度/秒)")]
    public float rotationSpeed = 60f;

    void Start()
    {
        anim = GetComponent<Animator>();
        parentTransform = transform.parent;
        knockbackScript = parentTransform?.GetComponent<Knockback>();
        rb = GetComponent<Rigidbody>();

        if (anim == null) Debug.LogWarning("Animator component not found!", this);
        if (knockbackScript == null) Debug.LogWarning("Knockback script not found on parent!", this);
        if (rb == null) Debug.LogWarning("Rigidbody component not found!", this);
    }

    void Update()
    {
        // アニメーションが無効化されている場合に回転処理を行う
        if (anim != null && !anim.enabled)
        {
            RotateObject();
        }
    }

    private void RotateObject()
    {
        // X軸方向に回転させる
        float rotationAmount = -rotationSpeed * Time.deltaTime;
        transform.Rotate(rotationAmount, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (anim != null)
            {
                anim.SetBool("AnimOn", true);
            }
        }
        else if (other.CompareTag("katana"))
        {
            //ノックバック処理を実行
            HandleKatanaHit();
        }
    }

    private void HandleKatanaHit()
    {
        knockbackScript?.ApplyKnockback();

        if (anim != null) anim.enabled = false;

        if (rb != null)
        {
            rb.mass = 1;
            rb.useGravity = true;

            // ノックバック方向を正規化して力を加える
            Vector3 normalizedDirection = knockbackDirection.normalized;
            rb.AddForce(normalizedDirection * forceAmount, ForceMode.Impulse);

            if (parentTransform != null)
            {
                parentTransform.position += normalizedDirection * forceAmount * Time.deltaTime;
            }
        }
    }
}

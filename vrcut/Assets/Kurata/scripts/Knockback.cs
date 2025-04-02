using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 1.0f;
    public float gravityEnableThresholdY = 4.0f; // Y座標がこの値を超えると重力を有効化
    public float knockbackResetDelay = 5.0f; // ノックバック後の処理遅延時間

    private Rigidbody rb;
    private float initialYPosition;
    private bool isKnockbackActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }
        initialYPosition = transform.position.y;
    }

    public void ApplyKnockback()
    {
        if (rb == null) return;

        // ノックバックの方向と力を設定
        Vector3 knockbackDirection = new Vector3(0, 1, 1).normalized;

        rb.velocity = Vector3.zero; // 既存の速度リセット
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        rb.useGravity = true; // ノックバック時に重力を有効化
        Debug.Log("Knockback applied, gravity enabled.");

        isKnockbackActive = true;

        // ノックバック後の監視を開始
        StartCoroutine(HandleKnockbackReset());
    }

    private IEnumerator HandleKnockbackReset()
    {
        yield return new WaitForSeconds(knockbackResetDelay);

        while (isKnockbackActive)
        {
            float currentYPosition = transform.position.y;

            // 重力を強制的に有効化する条件
            if (currentYPosition >= gravityEnableThresholdY && !rb.useGravity)
            {
                rb.useGravity = true;
                Debug.Log("Gravity enabled due to high Y position.");
            }

            // Y座標が初期値に戻ったら処理を終了
            if (currentYPosition <= initialYPosition)
            {
                ResetKnockbackState();
                yield break;
            }

            yield return null;
        }
    }

    private void ResetKnockbackState()
    {
        if (rb == null) return;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // Y座標を初期値に戻す
        transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);
        Debug.Log("Knockback state reset, gravity disabled.");

        isKnockbackActive = false;

        Destroy(rb, 0.5f);
    }
}
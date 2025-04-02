using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 1.0f;
    public float gravityEnableThresholdY = 4.0f; // Y���W�����̒l�𒴂���Əd�͂�L����
    public float knockbackResetDelay = 5.0f; // �m�b�N�o�b�N��̏����x������

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

        // �m�b�N�o�b�N�̕����Ɨ͂�ݒ�
        Vector3 knockbackDirection = new Vector3(0, 1, 1).normalized;

        rb.velocity = Vector3.zero; // �����̑��x���Z�b�g
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        rb.useGravity = true; // �m�b�N�o�b�N���ɏd�͂�L����
        Debug.Log("Knockback applied, gravity enabled.");

        isKnockbackActive = true;

        // �m�b�N�o�b�N��̊Ď����J�n
        StartCoroutine(HandleKnockbackReset());
    }

    private IEnumerator HandleKnockbackReset()
    {
        yield return new WaitForSeconds(knockbackResetDelay);

        while (isKnockbackActive)
        {
            float currentYPosition = transform.position.y;

            // �d�͂������I�ɗL�����������
            if (currentYPosition >= gravityEnableThresholdY && !rb.useGravity)
            {
                rb.useGravity = true;
                Debug.Log("Gravity enabled due to high Y position.");
            }

            // Y���W�������l�ɖ߂����珈�����I��
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

        // Y���W�������l�ɖ߂�
        transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);
        Debug.Log("Knockback state reset, gravity disabled.");

        isKnockbackActive = false;

        Destroy(rb, 0.5f);
    }
}
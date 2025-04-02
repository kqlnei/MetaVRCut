using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFlickOff : MonoBehaviour
{
    private Animator anim;
    //private Rigidbody rbPare;
    private Transform parentTransform;
    public float forceAmount = 10f;
    public float rotationSpeed = 60f;

    private Knockback knockbackScript;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        parentTransform = gameObject.GetComponentInParent<Transform>();
        knockbackScript = transform.parent.GetComponent<Knockback>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.enabled == false)
        {
            // X�������ɉ�]������
            float rotationAmount = -rotationSpeed * Time.deltaTime;  // 1�t���[���ŉ�]����p�x
            transform.Rotate(rotationAmount, 0f, 0f);  // X���ɉ�]��������

            // ��]�p�x��0�`360�x�ɐ�������
            float currentRotationX = transform.eulerAngles.x;
            if (currentRotationX > 360f)
            {
                currentRotationX -= 360f;
            }

            // ��]�p�x��0����360�x�ɕۂ�
            transform.eulerAngles = new Vector3(Mathf.Repeat(currentRotationX, 360f), transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(SetAnimOnWithDelay(0.75f)); // 0.5�b�̒x�����w��
        }

        if (other.CompareTag("katana"))
        {
            knockbackScript.ApplyKnockback();
            anim.enabled = false;

            Rigidbody rb = this.GetComponent<Rigidbody>();
            rb.mass = 1;
            rb.useGravity = true;

            Vector3 knockbackDirection = new Vector3(0, 1, 1); // �����𐳋K��
            rb.AddForce(knockbackDirection * forceAmount, ForceMode.Impulse);
            parentTransform.position += new Vector3(0, 0, 1) * forceAmount * Time.deltaTime;
        }
    }

    // Coroutine���g���Ēx����ǉ�
    private IEnumerator SetAnimOnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetBool("AnimOn", true);
    }


}

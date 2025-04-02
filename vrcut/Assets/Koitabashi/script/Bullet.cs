using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // �e�̑��x
    public GameObject shooter; // ���ˌ���EnemyShooter
    private bool isReflected = false; // ���˕Ԃ��t���O

    [Header("���Ŏa�鎞�̑��x臒l")]
    [SerializeField] private float katanaSpeed = 3.0f;

    [Header("�T�E���h�֘A")]
    [SerializeField] private AudioClip reflectSound; // ���ɓ����������̉�
    [SerializeField] private AudioClip hitEnemySound; // �G�ɓ����������̉�

    [Header("�e�̎���")]
    [SerializeField] private float bulletLifeTime = 5f; // �e�̎����i�b�j

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        // ��莞�Ԍ�ɒe���폜
        Destroy(gameObject, bulletLifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "katana" && !isReflected)
        {
            SwordTracker swordTracker = other.GetComponent<SwordTracker>();
            if (swordTracker != null && swordTracker.currentSpeed > katanaSpeed) // ����: ���̑��x���w��ȏ�
            {
                // ���˕Ԃ�����
                if (shooter != null)
                {
                    Vector3 reflectedDirection = (shooter.transform.position - transform.position).normalized;
                    Rigidbody rb = GetComponent<Rigidbody>();

                    // ���̑��x����ɒe�̔��ˑ��x�𒲐�
                    float newSpeed = Mathf.Clamp(swordTracker.currentSpeed * 2f, 10f, 50f);
                    rb.velocity = reflectedDirection * newSpeed;

                    transform.rotation = Quaternion.LookRotation(reflectedDirection);
                    isReflected = true;

                    // ���ɓ����������̉����Đ�
                    SoundManager.PlaySound(reflectSound, transform.position, 10f, 150f);
                }
            }
            else
            {
                Debug.Log("Swing too slow to reflect!");
            }
        }
        else if (other.gameObject.tag == "Damage" && isReflected)
        {
            GameObject parent = other.transform.parent?.gameObject; // �e�I�u�W�F�N�g���擾

            if (parent != null)
            {
                Destroy(parent); // �e�I�u�W�F�N�g���폜
            }

            // �G�ɓ����������̉����Đ�
            SoundManager.PlaySound(hitEnemySound, transform.position, 10f, 150f);

            Destroy(gameObject); // �e���폜
        }
    }
}

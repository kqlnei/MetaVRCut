//�{�X�p��EnemyShooter�����P�����畁�ʂ̃G�l�~�[�����������Ȃ����̂ł������p�ɍēx�쐬
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter2 : MonoBehaviour
{
    public GameObject bulletPrefab; // �e��Prefab
    public Transform bulletSpawnPoint; // �e�����˂����ʒu
    public Transform playerCamera; // �v���C���[�̃J�������A�^�b�`
    public float shootInterval = 2f; // �e�����Ԋu
    public float bulletSpeed = 10f; // �e�̑��x
    public float predictionTime = 0.5f; // �v���C���[�̈ړ���\�����鎞��
    public AudioClip shootSound; // �e�������̉���ǉ�
    private AudioSource audioSource; // �����Đ����邽�߂� AudioSource
    private bool isPlayerInRange = false;

    void Start()
    {
        // AudioSource ��������
        audioSource = GetComponent<AudioSource>();
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // �G�̌������v���C���[�J���������ɒ���
            Vector3 direction = playerCamera.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction); // �S�����̉�]

            transform.parent.rotation = Quaternion.Slerp(
                transform.parent.rotation,
                targetRotation,
                Time.deltaTime * 5f // ��]���x
            );

            // �e��������
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                StartCoroutine(ShootAtPlayer(other));
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerInRange = false; // �͈͊O�ɂȂ�����e�����̂��~
        }
    }

    IEnumerator ShootAtPlayer(Collider player)
    {
        while (isPlayerInRange)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (shootSound != null && audioSource != null)
            {
                SoundManager.PlaySound(shootSound, transform.position, 10f, 2000f);
            }
            if (rb != null)
            {
                // �v���C���[�̑��x���擾
                PlayerMovementTracker tracker = player.GetComponent<PlayerMovementTracker>();
                Vector3 playerVelocity = tracker != null ? tracker.CurrentVelocity : new Vector3(0, 0, 5f);

                // �e�̓��B���Ԃ��v�Z
                float travelTime = Vector3.Distance(bulletSpawnPoint.position, playerCamera.position) / bulletSpeed * 0.1f;//�����̐���������΃v���C���[�̎��_�ɑ΂��Ăǂ��ɔ�Ԃ��ς��

                // �v���C���[�̖����ʒu���v�Z
                Vector3 predictedPosition = playerCamera.position + playerVelocity * travelTime;

                // �f�o�b�O���C���Ŋm�F
                Debug.DrawLine(bulletSpawnPoint.position, predictedPosition, Color.red, 2f); // �e�̐i�s���C��
                Debug.DrawLine(playerCamera.position, playerCamera.position + playerVelocity, Color.green, 2f); // �v���C���[�̐i�s���C��

                // �������i�s�������v�Z
                Vector3 bulletDirection = (predictedPosition - bulletSpawnPoint.position).normalized;

                // �e�̑��x��ݒ�
                rb.velocity = bulletDirection * bulletSpeed;

                // �e�̉�]��i�s�����ɍ��킹��i�����ڂ𒲐��j
                bullet.transform.forward = bulletDirection;

                // �e�ɔ��ˌ���o�^
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.shooter = gameObject; // ����EnemyShooter��o�^
                }
            }

            // ���̒e�����܂őҋ@
            yield return new WaitForSeconds(shootInterval);
        }
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab; // 弾のPrefab
    public Transform bulletSpawnPoint; // 弾が発射される位置
    public Transform playerCamera; // プレイヤーのカメラをアタッチ
    public float shootInterval = 2f; // 弾を撃つ間隔
    public float bulletSpeed = 10f; // 弾の速度
    public float predictionTime = 0.5f; // プレイヤーの移動を予測する時間
    public AudioClip shootSound; // 弾を撃つ時の音を追加
    private bool isPlayerInRange = false;
    private Quaternion initialRotation; // スタート時の回転を保持
    private AudioSource audioSource; // 音を再生するための AudioSource

    void Start()
    {
        // スタート時の回転を記録
        initialRotation = transform.parent.rotation;
        // AudioSource を初期化
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // 敵の向きをプレイヤーカメラ方向に調整
            Vector3 direction = playerCamera.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction); // 全方向の回転

            // スタート時の回転を基準にした相対的な回転を計算
            targetRotation = initialRotation * Quaternion.Inverse(Quaternion.identity) * targetRotation;

            transform.parent.rotation = Quaternion.Slerp(
                transform.parent.rotation,
                targetRotation,
                Time.deltaTime * 5f // 回転速度
            );

            // 弾を撃つ準備
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
            isPlayerInRange = false; // 範囲外になったら弾を撃つのを停止
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
                SoundManager.PlaySound(shootSound, transform.position, 100f, 2000f);
            }
            if (rb != null)
            {
                // プレイヤーの速度を取得
                PlayerMovementTracker tracker = player.GetComponent<PlayerMovementTracker>();
                Vector3 playerVelocity = tracker != null ? tracker.CurrentVelocity : new Vector3(0, 0, 5f);

                // 弾の到達時間を計算
                float travelTime = Vector3.Distance(bulletSpawnPoint.position, playerCamera.position) / bulletSpeed * 0.25f;

                // プレイヤーの未来位置を計算
                Vector3 predictedPosition = playerCamera.position + playerVelocity * travelTime;

                // デバッグラインで確認
                Debug.DrawLine(bulletSpawnPoint.position, predictedPosition, Color.red, 2f);
                Debug.DrawLine(playerCamera.position, playerCamera.position + playerVelocity, Color.green, 2f);

                // 正しい進行方向を計算
                Vector3 bulletDirection = (predictedPosition - bulletSpawnPoint.position).normalized;

                // 弾の速度を設定
                rb.velocity = bulletDirection * bulletSpeed;

                // 弾の回転を進行方向に合わせる
                bullet.transform.forward = bulletDirection;

                // 弾に発射元を登録
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.shooter = gameObject; // このEnemyShooterを登録
                }
            }

            // 次の弾を撃つまで待機
            yield return new WaitForSeconds(shootInterval);
        }
    }
}
//ボス用にEnemyShooterを改善したら普通のエネミーがおかしくなったのでそっち用に再度作成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter2 : MonoBehaviour
{
    public GameObject bulletPrefab; // 弾のPrefab
    public Transform bulletSpawnPoint; // 弾が発射される位置
    public Transform playerCamera; // プレイヤーのカメラをアタッチ
    public float shootInterval = 2f; // 弾を撃つ間隔
    public float bulletSpeed = 10f; // 弾の速度
    public float predictionTime = 0.5f; // プレイヤーの移動を予測する時間
    public AudioClip shootSound; // 弾を撃つ時の音を追加
    private AudioSource audioSource; // 音を再生するための AudioSource
    private bool isPlayerInRange = false;

    void Start()
    {
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
                SoundManager.PlaySound(shootSound, transform.position, 10f, 2000f);
            }
            if (rb != null)
            {
                // プレイヤーの速度を取得
                PlayerMovementTracker tracker = player.GetComponent<PlayerMovementTracker>();
                Vector3 playerVelocity = tracker != null ? tracker.CurrentVelocity : new Vector3(0, 0, 5f);

                // 弾の到達時間を計算
                float travelTime = Vector3.Distance(bulletSpawnPoint.position, playerCamera.position) / bulletSpeed * 0.1f;//ここの数字いじればプレイヤーの視点に対してどこに飛ぶか変わる

                // プレイヤーの未来位置を計算
                Vector3 predictedPosition = playerCamera.position + playerVelocity * travelTime;

                // デバッグラインで確認
                Debug.DrawLine(bulletSpawnPoint.position, predictedPosition, Color.red, 2f); // 弾の進行ライン
                Debug.DrawLine(playerCamera.position, playerCamera.position + playerVelocity, Color.green, 2f); // プレイヤーの進行ライン

                // 正しい進行方向を計算
                Vector3 bulletDirection = (predictedPosition - bulletSpawnPoint.position).normalized;

                // 弾の速度を設定
                rb.velocity = bulletDirection * bulletSpeed;

                // 弾の回転を進行方向に合わせる（見た目を調整）
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
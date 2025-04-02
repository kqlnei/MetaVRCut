using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;              // 弾の初速
    [SerializeField] private float bulletLifeTime = 5f;        // 弾の寿命（秒）

    [Header("Reflection Settings")]
    [SerializeField] private float katanaSpeedThreshold = 3.0f; // 刀で斬る時の速度閾値
    [SerializeField] private float minReflectedSpeed = 15f;     // 反射後の最小速度
    [SerializeField] private float maxReflectedSpeed = 30f;     // 反射後の最大速度
    [SerializeField] private Vector2 deviationXRange = new Vector2(-0.5f, 0.5f); // 横方向のランダム範囲
    [SerializeField] private Vector2 deviationYRange = new Vector2(-0.3f, 0.3f); // 縦方向のランダム範囲

    [Header("Sound Settings")]
    [SerializeField] private AudioClip reflectSound;           // 刀に当たった時の音
    [SerializeField] private AudioClip hitEnemySound;          // 敵に当たった時の音

    public GameObject shooter; // 発射元のEnemyShooter

    private bool isReflected = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, bulletLifeTime); // 弾の寿命を設定
    }

    private void OnTriggerEnter(Collider other)
    {
        // 「katana」タグとの衝突で反射処理を実行
        if (other.CompareTag("katana") && !isReflected)
        {
            HandleReflection(other);
        }
        // 反射済みの弾が「Damage」タグのオブジェクトに当たった場合
        else if (other.CompareTag("Damage") && isReflected)
        {
            HandleEnemyHit(other);
        }
    }

    /// <summary>
    /// 刀に当たった際の反射処理
    /// </summary>
    /// <param name="other">衝突したオブジェクトのコライダー</param>
    private void HandleReflection(Collider other)
    {
        SwordTracker swordTracker = other.GetComponent<SwordTracker>();
        if (swordTracker != null && swordTracker.currentSpeed > katanaSpeedThreshold)
        {
            // 反射方向は基本的に進行方向の逆
            Vector3 baseDirection = -transform.forward;

            //反射方向に変化を付与
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            randomOffset.x = Mathf.Clamp(randomOffset.x, deviationXRange.x, deviationXRange.y);
            randomOffset.y = Mathf.Clamp(randomOffset.y, deviationYRange.x, deviationYRange.y);
            Vector3 reflectDirection = (baseDirection + randomOffset).normalized;

            // 刀の速度に基づく反射速度計算
            float speedAboveThreshold = swordTracker.currentSpeed - katanaSpeedThreshold;
            float speedRatio = Mathf.Clamp01(speedAboveThreshold / katanaSpeedThreshold);
            float newSpeed = Mathf.Lerp(minReflectedSpeed, maxReflectedSpeed, speedRatio);

            rb.velocity = reflectDirection * newSpeed;
            transform.rotation = Quaternion.LookRotation(reflectDirection);

            isReflected = true;

            // 反射時のサウンド
            SoundManager.PlaySound(reflectSound, transform.position, 10f, 150f);
        }
        else
        {
            Debug.Log("Swing too slow to reflect!");
        }
    }

    /// <summary>
    /// 反射後に敵に当たった際の処理
    /// </summary>
    /// <param name="other">衝突したオブジェクトのコライダー</param>
    private void HandleEnemyHit(Collider other)
    {
        // Damage タグのオブジェクトの親（通常は敵本体）を削除
        if (other.transform.parent != null)
        {
            Destroy(other.transform.parent.gameObject);
        }
        // 敵にヒットした際のサウンド
        SoundManager.PlaySound(hitEnemySound, transform.position, 10f, 150f);
        Destroy(gameObject); // 弾削除
    }
}
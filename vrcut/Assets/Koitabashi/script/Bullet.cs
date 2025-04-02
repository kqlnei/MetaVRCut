using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // 弾の速度
    public GameObject shooter; // 発射元のEnemyShooter
    private bool isReflected = false; // 跳ね返しフラグ

    [Header("刀で斬る時の速度閾値")]
    [SerializeField] private float katanaSpeed = 3.0f;

    [Header("サウンド関連")]
    [SerializeField] private AudioClip reflectSound; // 刀に当たった時の音
    [SerializeField] private AudioClip hitEnemySound; // 敵に当たった時の音

    [Header("弾の寿命")]
    [SerializeField] private float bulletLifeTime = 5f; // 弾の寿命（秒）

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        // 一定時間後に弾を削除
        Destroy(gameObject, bulletLifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "katana" && !isReflected)
        {
            SwordTracker swordTracker = other.GetComponent<SwordTracker>();
            if (swordTracker != null && swordTracker.currentSpeed > katanaSpeed) // 条件: 刀の速度が指定以上
            {
                // 跳ね返し処理
                if (shooter != null)
                {
                    Vector3 reflectedDirection = (shooter.transform.position - transform.position).normalized;
                    Rigidbody rb = GetComponent<Rigidbody>();

                    // 刀の速度を基に弾の反射速度を調整
                    float newSpeed = Mathf.Clamp(swordTracker.currentSpeed * 2f, 10f, 50f);
                    rb.velocity = reflectedDirection * newSpeed;

                    transform.rotation = Quaternion.LookRotation(reflectedDirection);
                    isReflected = true;

                    // 刀に当たった時の音を再生
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
            GameObject parent = other.transform.parent?.gameObject; // 親オブジェクトを取得

            if (parent != null)
            {
                Destroy(parent); // 親オブジェクトを削除
            }

            // 敵に当たった時の音を再生
            SoundManager.PlaySound(hitEnemySound, transform.position, 10f, 150f);

            Destroy(gameObject); // 弾を削除
        }
    }
}

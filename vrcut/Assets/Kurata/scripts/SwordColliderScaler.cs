using UnityEngine;

public class SwordColliderScaler : MonoBehaviour
{
    public Transform handle; // 刀の持ち手のTransform（基準点）
    public float baseLength = 1.0f; // 元の長さ
    public float maxStretchFactor = 2.0f; // 伸ばす最大倍率
    public float velocityThreshold = 2.0f; // 伸び始める速度のしきい値

    private BoxCollider boxCollider;
    private Vector3 previousPosition;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        previousPosition = transform.position;
    }

    void Update()
    {
        // 刀の速度を計算
        float velocity = (transform.position - previousPosition).magnitude / Time.deltaTime;

        // 速度に応じてBoxColliderのサイズを変更
        float stretchFactor = Mathf.Clamp(1 + (velocity / velocityThreshold), 1, maxStretchFactor);
        boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, baseLength * stretchFactor);

        // コライダーの位置調整（長さが伸びる分、中心をずらす）
        boxCollider.center = new Vector3(0, 0, (boxCollider.size.z - baseLength) / 2);

        previousPosition = transform.position;
    }
}

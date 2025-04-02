using UnityEngine;

public class ContinuousCollider : MonoBehaviour
{
    [Header("Collider Settings")]
    [SerializeField] private Transform colliderObject; // 伸ばす対象のコライダーオブジェクト

    private Vector3 previousPosition;
    private float minColliderExtent; // コライダーの最小Z方向の大きさ（初期値の半分）

    private void Start()
    {
        if (colliderObject == null)
        {
            Debug.LogError("Collider Object is not assigned in ContinuousCollider.");
            enabled = false;
            return;
        }

        // 初期スケールから最小の大きさを算出
        minColliderExtent = colliderObject.localScale.z / 2f;
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        UpdateColliderBasedOnMovement();
    }

    /// <summary>
    /// 前フレからの移動量に応じ、コライダーの位置とスケールを調整
    /// </summary>
    private void UpdateColliderBasedOnMovement()
    {
        // 現在位置と前回位置との差分
        Vector3 displacement = transform.position - previousPosition;

        // 正面方向(transform.forward)への移動量
        float displacementZ = Vector3.Dot(displacement, transform.forward);

        // 移動量に応じてコライダーの位置と大きさを調整
        if (displacementZ > minColliderExtent)
        {
            // 前方へ閾値以上移動した場合
            colliderObject.position = transform.position - (displacementZ - minColliderExtent) / 2f * transform.forward;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, displacementZ + minColliderExtent);
        }
        else if (displacementZ < -minColliderExtent)
        {
            // 後方へ閾値以上移動した場合
            colliderObject.position = transform.position + (-displacementZ - minColliderExtent) / 2f * transform.forward;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, -displacementZ + minColliderExtent);
        }
        else
        {
            // 移動が閾値内の場合は初期状態に戻す
            colliderObject.position = transform.position;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, minColliderExtent * 2f);
        }

        // 次フレームでの比較用
        previousPosition = transform.position;
    }
}
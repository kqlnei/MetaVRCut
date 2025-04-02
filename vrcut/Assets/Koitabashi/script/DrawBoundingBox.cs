using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DrawBoundingBox : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    void OnDrawGizmos()
    {
        // MeshRendererを取得
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            // オブジェクトのバウンドボックスを取得
            Bounds bounds = meshRenderer.bounds;

            // バウンドボックスをワイヤーフレームで描画
            Gizmos.color = Color.green;  // 色を設定
            Gizmos.DrawWireCube(bounds.center, bounds.size);  // 中心とサイズでバウンドボックスを描画
        }
    }
}

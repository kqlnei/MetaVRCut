/*************************************************************
 * 
 * このスクリプトは最終的に使用されていません。
 * 
 *************************************************************/

using UnityEngine;
using BLINDED_AM_ME;

public class Cutter2 : MonoBehaviour
{
    public GameObject cuttingPlane;     // 切断面を示すプレーン
    public Material capMaterial;        // 切断面に使用するマテリアル
    public Vector3 cuttingBoxSize = new Vector3(2, 0.01f, 2);  // 切断面の範囲
    public string targetTag = "Cuttable"; // 切断可能なオブジェクトのタグ

    void OnTriggerEnter(Collider other)
    {

        // 対象が特定のタグを持っているか確認
        if (other.CompareTag(targetTag))
        {
            
            GameObject target = other.gameObject;
            
            // ギズモが対象オブジェクトに触れているか確認
            if (IsTouchingCuttingBox(target.GetComponent<Renderer>().bounds, cuttingPlane.transform, cuttingBoxSize))
            {
                Debug.Log("触れてます");
                PerformCut(target);
            }
        }
    }

    void PerformCut(GameObject target)
    {
        Vector3 anchorPoint = cuttingPlane.transform.position;
        Vector3 normalDirection = cuttingPlane.transform.up;

        // 切断面のプレーンを作成
        Plane cuttingPlaneObject = new Plane(normalDirection, anchorPoint);

        // MeshCutを呼び出し、対象オブジェクトを切断
        GameObject[] pieces = MeshCut.Cut(target, anchorPoint, normalDirection, capMaterial);

        if (pieces != null)
        {
            foreach (GameObject piece in pieces)
            {
                // 各切断されたピースにRigidbodyを追加し、物理演算を適用
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = 1;
                rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
            }
        }
    }

    // ギズモの範囲にオブジェクトのバウンドボックスが触れているか確認
    bool IsTouchingCuttingBox(Bounds bounds, Transform cuttingPlaneTransform, Vector3 boxSize)
    {
        // オブジェクトのバウンドボックスの中心をギズモのローカル空間に変換
        Vector3 localCenter = cuttingPlaneTransform.InverseTransformPoint(bounds.center);

        // ローカル空間のバウンドボックスのサイズを半分にして計算
        Vector3 halfBoxSize = boxSize * 0.5f;

        // バウンドボックスのエクステントを考慮した計算で、一部でも範囲内に触れているかチェック
        if (Mathf.Abs(localCenter.x) - bounds.extents.x < halfBoxSize.x &&
            Mathf.Abs(localCenter.y) - bounds.extents.y < halfBoxSize.y &&
            Mathf.Abs(localCenter.z) - bounds.extents.z < halfBoxSize.z)
        {
            return true;
        }

        return false;
    }


    void OnDrawGizmos()
    {
        if (cuttingPlane != null)
        {
            Vector3 anchorPoint = cuttingPlane.transform.position;

            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.TRS(anchorPoint, cuttingPlane.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cuttingBoxSize);
        }
    }
}

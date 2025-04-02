/*************************************************************
 * 
 * このスクリプトは最終的に使用されていません。
 * 
 *************************************************************/


using UnityEngine;
using BLINDED_AM_ME;
public class Cutter : MonoBehaviour
{
    public GameObject victim;           // 切断対象のオブジェクト
    public GameObject cuttingPlane;     // 切断面を示すプレーン
    public Material capMaterial;        // 切断面に使用するマテリアル
    public Vector3 cuttingBoxSize = new Vector3(2, 0.01f, 2);  // 切断面の範囲

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PerformCut(victim);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Cuttable"))
        {
            PerformCut(other.gameObject);
        }
    }

    void PerformCut(GameObject target)
    {
        Vector3 anchorPoint = cuttingPlane.transform.position;
        Vector3 normalDirection = cuttingPlane.transform.up;

        // ターゲットのメッシュバウンドを取得
        MeshRenderer targetRenderer = target.GetComponent<MeshRenderer>();
        if (targetRenderer != null)
        {
            // 切断面のプレーンを作成
            Plane cuttingPlaneObject = new Plane(normalDirection, anchorPoint);

            // オブジェクトのバウンドボックスを取得
            Bounds targetBounds = targetRenderer.bounds;

            // オブジェクトのバウンドボックスが切断面に触れているか確認
            if (IsTouchingCuttingBox(targetBounds, cuttingPlane.transform, cuttingBoxSize))
            {
                GameObject[] pieces = MeshCut.Cut(target, anchorPoint, normalDirection, capMaterial);

                if (pieces != null)
                {
                    foreach (GameObject piece in pieces)
                    {
                        Rigidbody rb = piece.AddComponent<Rigidbody>();
                        rb.mass = 1;
                        rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
                        victim = piece;
                    }
                }
            }
            else
            {
                Debug.Log("オブジェクトが切断面に触れていません。");
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

        // ローカル空間において、バウンドボックスがギズモの範囲内にあるかを確認
        if (Mathf.Abs(localCenter.x) < halfBoxSize.x + bounds.extents.x &&
            Mathf.Abs(localCenter.y) < halfBoxSize.y + bounds.extents.y &&
            Mathf.Abs(localCenter.z) < halfBoxSize.z + bounds.extents.z)
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

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(anchorPoint, cuttingPlane.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cuttingBoxSize);
        }
    }
}

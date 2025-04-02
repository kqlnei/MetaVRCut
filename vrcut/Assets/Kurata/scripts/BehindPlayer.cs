using UnityEngine;

public class BehindPlayer : MonoBehaviour
{
    private const string CuttableTag = "Cuttable";

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag(CuttableTag))
        {
            // 破壊前に追加処理を実行
            HandleCuttableDestruction(other);

            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Cuttable オブジェクトが破壊される際の追加処理
    /// </summary>
    /// <param name="cuttableCollider">破壊対象のコライダー</param>
    private void HandleCuttableDestruction(Collider cuttableCollider)
    {
        Debug.Log($"{cuttableCollider.name} がいい感じに破壊できたお");
    }
}

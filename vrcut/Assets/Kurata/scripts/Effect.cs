using UnityEngine;

public class Effect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("エフェクトとして生成されるプレハブ")]
    public GameObject effectPrefab;

    [Tooltip("生成されたエフェクトが破壊されるまでの時間（秒）")]
    public float destroyDelay = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cuttable"))
        {
            if (effectPrefab == null)
            {
                Debug.LogError("Effect prefab is not assigned!", this);
                return;
            }

            // 接触点を取得
            Vector3 spawnPosition = other.ClosestPoint(transform.position);

            // エフェクトプレハブを生成
            GameObject spawnedEffect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            if (destroyDelay > 0)
            {
                Destroy(spawnedEffect, destroyDelay);
            }
            else
            {
                Debug.LogWarning("Destroy delay must be greater than 0", this);
            }
        }
    }
}

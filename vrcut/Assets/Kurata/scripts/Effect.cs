using UnityEngine;

public class Effect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("�G�t�F�N�g�Ƃ��Đ��������v���n�u")]
    public GameObject effectPrefab;

    [Tooltip("�������ꂽ�G�t�F�N�g���j�󂳂��܂ł̎��ԁi�b�j")]
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

            // �ڐG�_���擾
            Vector3 spawnPosition = other.ClosestPoint(transform.position);

            // �G�t�F�N�g�v���n�u�𐶐�
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

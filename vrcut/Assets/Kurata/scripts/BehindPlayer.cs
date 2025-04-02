using UnityEngine;

public class BehindPlayer : MonoBehaviour
{
    private const string CuttableTag = "Cuttable";

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag(CuttableTag))
        {
            // �j��O�ɒǉ����������s
            HandleCuttableDestruction(other);

            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Cuttable �I�u�W�F�N�g���j�󂳂��ۂ̒ǉ�����
    /// </summary>
    /// <param name="cuttableCollider">�j��Ώۂ̃R���C�_�[</param>
    private void HandleCuttableDestruction(Collider cuttableCollider)
    {
        Debug.Log($"{cuttableCollider.name} �����������ɔj��ł�����");
    }
}

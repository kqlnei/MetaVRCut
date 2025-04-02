using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DrawBoundingBox : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    void OnDrawGizmos()
    {
        // MeshRenderer���擾
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            // �I�u�W�F�N�g�̃o�E���h�{�b�N�X���擾
            Bounds bounds = meshRenderer.bounds;

            // �o�E���h�{�b�N�X�����C���[�t���[���ŕ`��
            Gizmos.color = Color.green;  // �F��ݒ�
            Gizmos.DrawWireCube(bounds.center, bounds.size);  // ���S�ƃT�C�Y�Ńo�E���h�{�b�N�X��`��
        }
    }
}

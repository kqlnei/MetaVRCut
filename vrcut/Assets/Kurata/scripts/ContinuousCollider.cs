using UnityEngine;

public class ContinuousCollider : MonoBehaviour
{
    [Header("Collider Settings")]
    [SerializeField] private Transform colliderObject; // �L�΂��Ώۂ̃R���C�_�[�I�u�W�F�N�g

    private Vector3 previousPosition;
    private float minColliderExtent; // �R���C�_�[�̍ŏ�Z�����̑傫���i�����l�̔����j

    private void Start()
    {
        if (colliderObject == null)
        {
            Debug.LogError("Collider Object is not assigned in ContinuousCollider.");
            enabled = false;
            return;
        }

        // �����X�P�[������ŏ��̑傫�����Z�o
        minColliderExtent = colliderObject.localScale.z / 2f;
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        UpdateColliderBasedOnMovement();
    }

    /// <summary>
    /// �O�t������̈ړ��ʂɉ����A�R���C�_�[�̈ʒu�ƃX�P�[���𒲐�
    /// </summary>
    private void UpdateColliderBasedOnMovement()
    {
        // ���݈ʒu�ƑO��ʒu�Ƃ̍���
        Vector3 displacement = transform.position - previousPosition;

        // ���ʕ���(transform.forward)�ւ̈ړ���
        float displacementZ = Vector3.Dot(displacement, transform.forward);

        // �ړ��ʂɉ����ăR���C�_�[�̈ʒu�Ƒ傫���𒲐�
        if (displacementZ > minColliderExtent)
        {
            // �O����臒l�ȏ�ړ������ꍇ
            colliderObject.position = transform.position - (displacementZ - minColliderExtent) / 2f * transform.forward;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, displacementZ + minColliderExtent);
        }
        else if (displacementZ < -minColliderExtent)
        {
            // �����臒l�ȏ�ړ������ꍇ
            colliderObject.position = transform.position + (-displacementZ - minColliderExtent) / 2f * transform.forward;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, -displacementZ + minColliderExtent);
        }
        else
        {
            // �ړ���臒l���̏ꍇ�͏�����Ԃɖ߂�
            colliderObject.position = transform.position;
            colliderObject.localScale = new Vector3(colliderObject.localScale.x, colliderObject.localScale.y, minColliderExtent * 2f);
        }

        // ���t���[���ł̔�r�p
        previousPosition = transform.position;
    }
}
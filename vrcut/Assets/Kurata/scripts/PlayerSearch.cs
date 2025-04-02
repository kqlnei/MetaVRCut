using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSearch : MonoBehaviour
{
    public enum FollowMode
    {
        Straight,    // �����Ǐ]
        ZigZag,      // �W�O�U�O�Ǐ]
        RandomZigZag // �����_���W�O�U�O�Ǐ]
    }

    [Header("��{�ݒ�")]
    [SerializeField] private FollowMode followMode = FollowMode.Straight;
    [SerializeField] private float speed = 3f;

    [Header("�W�O�U�O�Ǐ]�p�p�����[�^")]
    [SerializeField] private float waveFrequency = 2f;                // ���g��
    [SerializeField] private List<Vector3> bounceDirections;          // �܂�Ԃ��������X�g
    [SerializeField] private List<int> waveSteps;                     // �e�����ł̃W�O�U�O�i�K��
    [SerializeField] private List<float> moveDistances;               // �e�i�K�̈ړ�����

    [Header("�����_���W�O�U�O�p�p�����[�^")]
    [SerializeField] private float randomDirectionChangeInterval = 1f;  // �����ύX�Ԋu

    private float waveTimer = 0f;             // �W�O�U�O�p�^�C�}�[
    private int currentStep = 0;              // ���݂̒i�K��
    private int currentDirectionIndex = 0;    // ���݂̐܂�Ԃ������̃C���f�b�N�X
    private int remainingSteps;               // ���݂̕����Ŏc���Ă���i�K��
    private float currentAmplitude;           // ���݂̈ړ������i�U�ꕝ�j

    private float randomTimer = 0f;           // �����_���W�O�U�O�p�^�C�}�[

    void Start()
    {
        // �W�O�U�O�Ǐ]�p�p�����[�^���������ݒ肳��Ă��邩�`�F�b�N
        if (bounceDirections == null || bounceDirections.Count == 0 ||
            waveSteps == null || waveSteps.Count == 0 ||
            moveDistances == null || moveDistances.Count == 0)
        {
            Debug.LogError("�W�O�U�O�Ǐ]�p�p�����[�^���������ݒ肳��Ă��܂���B " + gameObject.name);
            enabled = false;
            return;
        }
        remainingSteps = waveSteps[0];
        currentAmplitude = moveDistances[0];
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �v���C���[�̐eTransform�Ǝ��g�̐eTransform���擾
            Transform targetParent = other.transform.parent;
            Transform thisParent = transform.parent;

            if (targetParent == null || thisParent == null)
            {
                return;
            }

            switch (followMode)
            {
                case FollowMode.Straight:
                    StraightFollow(targetParent, thisParent);
                    break;

                case FollowMode.ZigZag:
                    ZigZagFollow(targetParent, thisParent);
                    break;

                case FollowMode.RandomZigZag:
                    RandomZigZagFollow(targetParent, thisParent);
                    break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �����_���W�O�U�O���[�h�̃^�C�}�[���Z�b�g����
            if (followMode == FollowMode.RandomZigZag)
            {
                randomTimer = 0f;
                waveTimer = 0f;
            }
            Debug.Log("�v���C���[������͈͂𗣂ꂽ");
        }
    }

    /// <summary>
    /// �v���C���[�̈ʒu�֒����I�ɒǏ]
    /// </summary>
    void StraightFollow(Transform targetParent, Transform thisParent)
    {
        Vector3 targetPosition = targetParent.position;
        thisParent.position = Vector3.MoveTowards(thisParent.position, targetPosition, speed * Time.deltaTime);
        thisParent.LookAt(targetParent);
    }

    /// <summary>
    /// ���O�ɐݒ肳�ꂽ�p�^�[���ɉ����ĒǏ]
    /// </summary>
    void ZigZagFollow(Transform targetParent, Transform thisParent)
    {
        // ���݂̐܂�Ԃ������𐳋K�����Ď擾
        Vector3 currentBounceDirection = bounceDirections[currentDirectionIndex % bounceDirections.Count].normalized;

        // �W�O�U�O�p�̃^�C�}�[��i�߂�
        waveTimer += Time.deltaTime * waveFrequency;

        // �v���C���[�ւ̒����������Z�o
        Vector3 directionToPlayer = (targetParent.position - thisParent.position).normalized;
        // �܂�Ԃ������ƒ����������琂�������̃I�t�Z�b�g���v�Z
        Vector3 perpendicular = Vector3.Cross(directionToPlayer, currentBounceDirection);
        Vector3 waveOffset = perpendicular * Mathf.Sin(waveTimer) * currentAmplitude;
        Vector3 targetPosition = targetParent.position + waveOffset;

        thisParent.position = Vector3.MoveTowards(thisParent.position, targetPosition, speed * Time.deltaTime);
        thisParent.LookAt(targetParent);

        // �������i2�΁j�o�߂�����i�K����i�߂�
        if (waveTimer >= Mathf.PI * 2)
        {
            waveTimer = 0f;
            remainingSteps--;

            if (remainingSteps <= 0)
            {
                // ���̕����֐؂�ւ�
                currentDirectionIndex++;
                currentStep++;
                if (currentStep < waveSteps.Count && currentStep < moveDistances.Count)
                {
                    remainingSteps = waveSteps[currentStep];
                    currentAmplitude = moveDistances[currentStep];
                }
                else
                {
                    // �W�O�U�O�Ǐ]�����������璼���Ǐ]�ɐ؂�ւ�
                    followMode = FollowMode.Straight;
                    Debug.Log("ZigZag completed, switching to Straight follow mode.");
                }
            }
        }
    }

    /// <summary>
    /// �����_���ȃW�O�U�O�Ǐ]���s���B��莞�Ԃ��ƂɒǏ]�p�����[�^�������_���ɕύX����B
    /// </summary>
    void RandomZigZagFollow(Transform targetParent, Transform thisParent)
    {
        // bounceDirections�����moveDistances���������ݒ肳��Ă��邩�`�F�b�N
        if (bounceDirections == null || bounceDirections.Count == 0 || moveDistances == null || moveDistances.Count == 0)
            return;

        randomTimer += Time.deltaTime;
        if (randomTimer >= randomDirectionChangeInterval)
        {
            randomTimer = 0f;
            currentDirectionIndex = Random.Range(0, bounceDirections.Count);
            currentAmplitude = moveDistances[Random.Range(0, moveDistances.Count)];
            waveFrequency = Random.Range(waveFrequency * 0.5f, waveFrequency * 1.5f);
        }

        Vector3 currentBounceDirection = bounceDirections[currentDirectionIndex].normalized;
        waveTimer += Time.deltaTime * waveFrequency;

        Vector3 directionToPlayer = (targetParent.position - thisParent.position).normalized;
        Vector3 perpendicular = Vector3.Cross(directionToPlayer, currentBounceDirection);
        Vector3 waveOffset = perpendicular * Mathf.Sin(waveTimer) * currentAmplitude;
        Vector3 targetPosition = targetParent.position + waveOffset;

        thisParent.position = Vector3.MoveTowards(thisParent.position, targetPosition, speed * Time.deltaTime);
        thisParent.LookAt(targetParent);
    }
}
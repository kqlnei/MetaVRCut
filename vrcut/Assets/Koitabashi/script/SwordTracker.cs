using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTracker : MonoBehaviour
{
    public Transform centerEyeAnchor;
    private Vector3 previousLocalPosition;
    private Vector3 previousPosition; // ���[���h���W�p�̑O�t���[���ʒu
    public float currentSpeed;
    [HideInInspector] public Vector3 velocityDirection; // �ǉ�

    void Start()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor �����ݒ�ł��BOVRCameraRig �� CenterEye ��ݒ肵�Ă��������B");
            return;
        }

        previousLocalPosition = centerEyeAnchor.InverseTransformPoint(transform.position);
        previousPosition = transform.position; // ���[���h���W�p������
    }

    void Update()
    {
        if (centerEyeAnchor == null) return;

        // ���[�J�����W�v�Z�i���������j
        Vector3 currentLocalPosition = centerEyeAnchor.InverseTransformPoint(transform.position);
        currentSpeed = (currentLocalPosition - previousLocalPosition).magnitude / Time.deltaTime;
        previousLocalPosition = currentLocalPosition;

        // ���[���h���W�ł̑��x�����v�Z�i�ǉ������j
        Vector3 currentPosition = transform.position;
        velocityDirection = (currentPosition - previousPosition).normalized; // ���K�����������x�N�g��
        previousPosition = currentPosition;
    }
}
using UnityEngine;

[AddComponentMenu("XR Interaction/HUD Controller")]
public class HUDController : MonoBehaviour
{
    #region Movement Settings
    [Header("�ړ��ݒ�")]
    [Tooltip("�Ǐ]�Ώۂ�Transform")]
    [SerializeField] private Transform _followTarget;
    [Tooltip("�ʒu�Ǐ]�̊��炩��")]
    [SerializeField][Range(0.01f, 1f)] private float _positionSmoothing = 0.15f;
    [Tooltip("�����ʒu�������[�h")]
    [SerializeField] private bool _useInstantPosition = false;
    #endregion

    #region Rotation Settings
    [Header("��]�ݒ�")]
    [Tooltip("��]�����b�N (World Space)")]
    [SerializeField] private Vector3 _lockedAxes = Vector3.zero;
    [Tooltip("��{��]���x")]
    [SerializeField][Range(0.01f, 1f)] private float _baseRotationSpeed = 0.1f;
    [Tooltip("������]臒l")]
    [SerializeField][Range(0.1f, 1f)] private float _highSpeedThreshold = 0.85f;
    [Tooltip("��]���x�{��")]
    [SerializeField][Range(1f, 5f)] private float _speedMultiplier = 2.5f;
    #endregion

    #region Private Variables
    private Quaternion _targetRotation;
    private const float ANGLE_EPSILON = 0.1f;
    #endregion

    private void Reset()
    {
        InitializeDefaultTarget();
    }

    private void Start()
    {
        ValidateComponents();
    }

    private void LateUpdate()
    {
        if (_followTarget == null) return;

        UpdatePosition();
        UpdateRotation();
    }

    /// <summary>
    /// �f�t�H���g�^�[�Q�b�g�̎������o
    /// </summary>
    private void InitializeDefaultTarget()
    {
        if (_followTarget == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null) _followTarget = mainCam.transform;
        }
    }

    /// <summary>
    /// �R���|�[�l���g���؏���
    /// </summary>
    private void ValidateComponents()
    {
        if (_followTarget == null)
        {
            Debug.LogError("�Ǐ]�Ώۂ��ݒ肳��Ă��܂���", this);
            enabled = false;
        }
    }

    /// <summary>
    /// �ʒu�X�V����
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = _useInstantPosition
            ? _followTarget.position
            : Vector3.Lerp(transform.position, _followTarget.position, _positionSmoothing);
    }

    /// <summary>
    /// ��]�X�V����
    /// </summary>
    private void UpdateRotation()
    {
        var currentRotation = transform.rotation;
        _targetRotation = CalculateTargetRotation();

        float speedFactor = CalculateSpeedFactor(currentRotation);
        ApplyRotationSmoothing(speedFactor);
    }

    /// <summary>
    /// �^�[�Q�b�g��]�̌v�Z�i�������t���j
    /// </summary>
    private Quaternion CalculateTargetRotation()
    {
        Vector3 euler = _followTarget.eulerAngles;
        return Quaternion.Euler(
            _lockedAxes.x > 0 ? 0 : euler.x,
            _lockedAxes.y > 0 ? 0 : euler.y,
            _lockedAxes.z > 0 ? 0 : euler.z
        );
    }

    /// <summary>
    /// ��]���x�W���̌v�Z
    /// </summary>
    private float CalculateSpeedFactor(Quaternion currentRotation)
    {
        float angleDifference = Quaternion.Angle(currentRotation, _targetRotation);
        return angleDifference > ANGLE_EPSILON
            ? (1.0f - (_targetRotation.w / _highSpeedThreshold)) * _speedMultiplier
            : _baseRotationSpeed;
    }

    /// <summary>
    /// ��]��ԏ����̓K�p
    /// </summary>
    private void ApplyRotationSmoothing(float speedFactor)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            Mathf.Clamp01(_baseRotationSpeed * speedFactor * Time.deltaTime * 100)
        );
    }

    /// <summary>
    /// �Ǐ]�Ώۂ𓮓I�ɕύX
    /// </summary>
    /// <param name="newTarget">�V�����Ǐ]�Ώ�</param>
    /// <param name="instantSync">���������t���O</param>
    public void SetTrackingTarget(Transform newTarget, bool instantSync = false)
    {
        _followTarget = newTarget;
        if (instantSync) ImmediateSync();
    }

    /// <summary>
    /// �����ʒu�E��]����
    /// </summary>
    public void ImmediateSync()
    {
        if (_followTarget == null) return;
        transform.SetPositionAndRotation(_followTarget.position, CalculateTargetRotation());
    }
}
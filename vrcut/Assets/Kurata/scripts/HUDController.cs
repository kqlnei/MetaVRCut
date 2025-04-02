using UnityEngine;

[AddComponentMenu("XR Interaction/HUD Controller")]
public class HUDController : MonoBehaviour
{
    #region Movement Settings
    [Header("移動設定")]
    [Tooltip("追従対象のTransform")]
    [SerializeField] private Transform _followTarget;
    [Tooltip("位置追従の滑らかさ")]
    [SerializeField][Range(0.01f, 1f)] private float _positionSmoothing = 0.15f;
    [Tooltip("即時位置同期モード")]
    [SerializeField] private bool _useInstantPosition = false;
    #endregion

    #region Rotation Settings
    [Header("回転設定")]
    [Tooltip("回転軸ロック (World Space)")]
    [SerializeField] private Vector3 _lockedAxes = Vector3.zero;
    [Tooltip("基本回転速度")]
    [SerializeField][Range(0.01f, 1f)] private float _baseRotationSpeed = 0.1f;
    [Tooltip("高速回転閾値")]
    [SerializeField][Range(0.1f, 1f)] private float _highSpeedThreshold = 0.85f;
    [Tooltip("回転速度倍率")]
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
    /// デフォルトターゲットの自動検出
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
    /// コンポーネント検証処理
    /// </summary>
    private void ValidateComponents()
    {
        if (_followTarget == null)
        {
            Debug.LogError("追従対象が設定されていません", this);
            enabled = false;
        }
    }

    /// <summary>
    /// 位置更新処理
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = _useInstantPosition
            ? _followTarget.position
            : Vector3.Lerp(transform.position, _followTarget.position, _positionSmoothing);
    }

    /// <summary>
    /// 回転更新処理
    /// </summary>
    private void UpdateRotation()
    {
        var currentRotation = transform.rotation;
        _targetRotation = CalculateTargetRotation();

        float speedFactor = CalculateSpeedFactor(currentRotation);
        ApplyRotationSmoothing(speedFactor);
    }

    /// <summary>
    /// ターゲット回転の計算（軸制限付き）
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
    /// 回転速度係数の計算
    /// </summary>
    private float CalculateSpeedFactor(Quaternion currentRotation)
    {
        float angleDifference = Quaternion.Angle(currentRotation, _targetRotation);
        return angleDifference > ANGLE_EPSILON
            ? (1.0f - (_targetRotation.w / _highSpeedThreshold)) * _speedMultiplier
            : _baseRotationSpeed;
    }

    /// <summary>
    /// 回転補間処理の適用
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
    /// 追従対象を動的に変更
    /// </summary>
    /// <param name="newTarget">新しい追従対象</param>
    /// <param name="instantSync">即時同期フラグ</param>
    public void SetTrackingTarget(Transform newTarget, bool instantSync = false)
    {
        _followTarget = newTarget;
        if (instantSync) ImmediateSync();
    }

    /// <summary>
    /// 即時位置・回転同期
    /// </summary>
    public void ImmediateSync()
    {
        if (_followTarget == null) return;
        transform.SetPositionAndRotation(_followTarget.position, CalculateTargetRotation());
    }
}
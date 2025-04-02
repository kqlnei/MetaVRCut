using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTracker : MonoBehaviour
{
    public Transform centerEyeAnchor;
    private Vector3 previousLocalPosition;
    private Vector3 previousPosition; // ワールド座標用の前フレーム位置
    public float currentSpeed;
    [HideInInspector] public Vector3 velocityDirection; // 追加

    void Start()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor が未設定です。OVRCameraRig の CenterEye を設定してください。");
            return;
        }

        previousLocalPosition = centerEyeAnchor.InverseTransformPoint(transform.position);
        previousPosition = transform.position; // ワールド座標用初期化
    }

    void Update()
    {
        if (centerEyeAnchor == null) return;

        // ローカル座標計算（既存処理）
        Vector3 currentLocalPosition = centerEyeAnchor.InverseTransformPoint(transform.position);
        currentSpeed = (currentLocalPosition - previousLocalPosition).magnitude / Time.deltaTime;
        previousLocalPosition = currentLocalPosition;

        // ワールド座標での速度方向計算（追加処理）
        Vector3 currentPosition = transform.position;
        velocityDirection = (currentPosition - previousPosition).normalized; // 正規化した方向ベクトル
        previousPosition = currentPosition;
    }
}
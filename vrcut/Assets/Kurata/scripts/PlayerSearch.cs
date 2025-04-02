using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSearch : MonoBehaviour
{
    public enum FollowMode
    {
        Straight,    // 直線追従
        ZigZag,      // ジグザグ追従
        RandomZigZag // ランダムジグザグ追従
    }

    [Header("基本設定")]
    [SerializeField] private FollowMode followMode = FollowMode.Straight;
    [SerializeField] private float speed = 3f;

    [Header("ジグザグ追従用パラメータ")]
    [SerializeField] private float waveFrequency = 2f;                // 周波数
    [SerializeField] private List<Vector3> bounceDirections;          // 折り返し方向リスト
    [SerializeField] private List<int> waveSteps;                     // 各方向でのジグザグ段階数
    [SerializeField] private List<float> moveDistances;               // 各段階の移動距離

    [Header("ランダムジグザグ用パラメータ")]
    [SerializeField] private float randomDirectionChangeInterval = 1f;  // 方向変更間隔

    private float waveTimer = 0f;             // ジグザグ用タイマー
    private int currentStep = 0;              // 現在の段階数
    private int currentDirectionIndex = 0;    // 現在の折り返し方向のインデックス
    private int remainingSteps;               // 現在の方向で残っている段階数
    private float currentAmplitude;           // 現在の移動距離（振れ幅）

    private float randomTimer = 0f;           // ランダムジグザグ用タイマー

    void Start()
    {
        // ジグザグ追従用パラメータが正しく設定されているかチェック
        if (bounceDirections == null || bounceDirections.Count == 0 ||
            waveSteps == null || waveSteps.Count == 0 ||
            moveDistances == null || moveDistances.Count == 0)
        {
            Debug.LogError("ジグザグ追従用パラメータが正しく設定されていません。 " + gameObject.name);
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
            // プレイヤーの親Transformと自身の親Transformを取得
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
            // ランダムジグザグモードのタイマーリセット処理
            if (followMode == FollowMode.RandomZigZag)
            {
                randomTimer = 0f;
                waveTimer = 0f;
            }
            Debug.Log("プレイヤーが判定範囲を離れた");
        }
    }

    /// <summary>
    /// プレイヤーの位置へ直線的に追従
    /// </summary>
    void StraightFollow(Transform targetParent, Transform thisParent)
    {
        Vector3 targetPosition = targetParent.position;
        thisParent.position = Vector3.MoveTowards(thisParent.position, targetPosition, speed * Time.deltaTime);
        thisParent.LookAt(targetParent);
    }

    /// <summary>
    /// 事前に設定されたパターンに沿って追従
    /// </summary>
    void ZigZagFollow(Transform targetParent, Transform thisParent)
    {
        // 現在の折り返し方向を正規化して取得
        Vector3 currentBounceDirection = bounceDirections[currentDirectionIndex % bounceDirections.Count].normalized;

        // ジグザグ用のタイマーを進める
        waveTimer += Time.deltaTime * waveFrequency;

        // プレイヤーへの直線方向を算出
        Vector3 directionToPlayer = (targetParent.position - thisParent.position).normalized;
        // 折り返し方向と直線方向から垂直方向のオフセットを計算
        Vector3 perpendicular = Vector3.Cross(directionToPlayer, currentBounceDirection);
        Vector3 waveOffset = perpendicular * Mathf.Sin(waveTimer) * currentAmplitude;
        Vector3 targetPosition = targetParent.position + waveOffset;

        thisParent.position = Vector3.MoveTowards(thisParent.position, targetPosition, speed * Time.deltaTime);
        thisParent.LookAt(targetParent);

        // 一定周期（2π）経過したら段階数を進める
        if (waveTimer >= Mathf.PI * 2)
        {
            waveTimer = 0f;
            remainingSteps--;

            if (remainingSteps <= 0)
            {
                // 次の方向へ切り替え
                currentDirectionIndex++;
                currentStep++;
                if (currentStep < waveSteps.Count && currentStep < moveDistances.Count)
                {
                    remainingSteps = waveSteps[currentStep];
                    currentAmplitude = moveDistances[currentStep];
                }
                else
                {
                    // ジグザグ追従が完了したら直線追従に切り替え
                    followMode = FollowMode.Straight;
                    Debug.Log("ZigZag completed, switching to Straight follow mode.");
                }
            }
        }
    }

    /// <summary>
    /// ランダムなジグザグ追従を行う。一定時間ごとに追従パラメータをランダムに変更する。
    /// </summary>
    void RandomZigZagFollow(Transform targetParent, Transform thisParent)
    {
        // bounceDirectionsおよびmoveDistancesが正しく設定されているかチェック
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
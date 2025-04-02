using UnityEngine;

public class LastController : MonoBehaviour
{
    [Header("Player & Target Settings")]
    [SerializeField] private Transform player;         // 追跡対象のプレイヤー
    [SerializeField] private GameObject[] targetObjects; // 撃破すべきターゲット

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;      // 通常時の前進速度
    [SerializeField] private float chaseSpeed = 8f;        // 追跡時の速度
    [SerializeField] private float detectionRange = 10f;   // プレイヤーを検知する距離

    private enum State
    {
        Idle,  
        MovingForward,
        ChasingPlayer
    }

    private State currentState = State.Idle;
    private bool hasStartedMoving = false; // 一度でも移動開始したかのフラグ

    void Update()
    {
        UpdateState();
        HandleMovement();
    }

    /// <summary>
    /// 現在の状態を更新する。全ターゲットが撃破された場合はプレイヤー追跡状態に移行。
    /// 移動が開始された場合、状態変更を防止して移動状態を維持する。
    /// </summary>
    void UpdateState()
    {
        if (player == null)
        {
            Debug.LogWarning("Playerが設定されていません。");
            return;
        }

        // 全ターゲットが撃破された場合は追跡状態に移行
        if (AreAllTargetsDestroyed())
        {
            currentState = State.ChasingPlayer;
            return;
        }

        // 一度移動を開始した場合、状態を維持
        if (hasStartedMoving)
            return;

        // プレイヤーが検知範囲内に入ったかどうかで状態を切り替え
        bool isPlayerInRange = Vector3.Distance(transform.position, player.position) <= detectionRange;
        currentState = isPlayerInRange ? State.MovingForward : State.Idle;
    }

    /// <summary>
    /// 状態に応じた移動処理を実行
    /// </summary>
    void HandleMovement()
    {
        switch (currentState)
        {
            case State.Idle:
                // 何もしない
                break;

            case State.MovingForward:
                if (!hasStartedMoving)
                {
                    hasStartedMoving = true;
                }
                MoveForward();
                break;

            case State.ChasingPlayer:
                ChasePlayer();
                break;
        }
    }

    /// <summary>
    /// 全ターゲットが既に撃破されているかを判定
    /// </summary>
    /// <returns>すべてのターゲットが非アクティブならtrue、存在している場合はfalse</returns>
    bool AreAllTargetsDestroyed()
    {
        foreach (GameObject target in targetObjects)
        {
            if (target != null && target.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 現在の向きに合わせて前進移動
    /// </summary>
    void MoveForward()
    {
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;
    }

    /// <summary>
    /// プレイヤーに向かって追跡移動
    /// </summary>
    void ChasePlayer()
    {
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                chaseSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// エディタ上で検知範囲を可視化用
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
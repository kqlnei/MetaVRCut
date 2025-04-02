using UnityEngine;

public class LastController : MonoBehaviour
{
    [Header("Player & Target Settings")]
    [SerializeField] private Transform player;         // �ǐՑΏۂ̃v���C���[
    [SerializeField] private GameObject[] targetObjects; // ���j���ׂ��^�[�Q�b�g

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;      // �ʏ펞�̑O�i���x
    [SerializeField] private float chaseSpeed = 8f;        // �ǐՎ��̑��x
    [SerializeField] private float detectionRange = 10f;   // �v���C���[�����m���鋗��

    private enum State
    {
        Idle,  
        MovingForward,
        ChasingPlayer
    }

    private State currentState = State.Idle;
    private bool hasStartedMoving = false; // ��x�ł��ړ��J�n�������̃t���O

    void Update()
    {
        UpdateState();
        HandleMovement();
    }

    /// <summary>
    /// ���݂̏�Ԃ��X�V����B�S�^�[�Q�b�g�����j���ꂽ�ꍇ�̓v���C���[�ǐՏ�ԂɈڍs�B
    /// �ړ����J�n���ꂽ�ꍇ�A��ԕύX��h�~���Ĉړ���Ԃ��ێ�����B
    /// </summary>
    void UpdateState()
    {
        if (player == null)
        {
            Debug.LogWarning("Player���ݒ肳��Ă��܂���B");
            return;
        }

        // �S�^�[�Q�b�g�����j���ꂽ�ꍇ�͒ǐՏ�ԂɈڍs
        if (AreAllTargetsDestroyed())
        {
            currentState = State.ChasingPlayer;
            return;
        }

        // ��x�ړ����J�n�����ꍇ�A��Ԃ��ێ�
        if (hasStartedMoving)
            return;

        // �v���C���[�����m�͈͓��ɓ��������ǂ����ŏ�Ԃ�؂�ւ�
        bool isPlayerInRange = Vector3.Distance(transform.position, player.position) <= detectionRange;
        currentState = isPlayerInRange ? State.MovingForward : State.Idle;
    }

    /// <summary>
    /// ��Ԃɉ������ړ����������s
    /// </summary>
    void HandleMovement()
    {
        switch (currentState)
        {
            case State.Idle:
                // �������Ȃ�
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
    /// �S�^�[�Q�b�g�����Ɍ��j����Ă��邩�𔻒�
    /// </summary>
    /// <returns>���ׂẴ^�[�Q�b�g����A�N�e�B�u�Ȃ�true�A���݂��Ă���ꍇ��false</returns>
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
    /// ���݂̌����ɍ��킹�đO�i�ړ�
    /// </summary>
    void MoveForward()
    {
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;
    }

    /// <summary>
    /// �v���C���[�Ɍ������ĒǐՈړ�
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
    /// �G�f�B�^��Ō��m�͈͂������p
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
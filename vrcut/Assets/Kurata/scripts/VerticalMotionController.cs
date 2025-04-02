using UnityEngine;

public class VerticalMotionController : MonoBehaviour
{
    [Header("��{�ݒ�")]
    [SerializeField] private float amplitude = 1f; // �㉺�̕�
    [SerializeField] private float speed = 1f;     // �ړ����x
    [SerializeField] private bool useLocal = true; // ���[�J�����W���g�p

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        // �����ʒu���L��
        startPosition = useLocal ? transform.localPosition : transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime * speed;

        // Y���W���v�Z
        float newY = startPosition.y + Mathf.Sin(timer) * amplitude;

        // �ʒu���X�V
        Vector3 newPosition = new Vector3(
            startPosition.x,
            newY,
            startPosition.z
        );

        if (useLocal)
        {
            transform.localPosition = newPosition;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    // �p�����[�^��ύX���郁�\�b�h�i�K�v�ɉ����Ďg�p�j
    public void SetParameters(float newSpeed, float newAmplitude)
    {
        speed = newSpeed;
        amplitude = newAmplitude;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // �����͈̔͂�����
        Vector3 basePos = Application.isPlaying ? startPosition : 
            (useLocal ? transform.localPosition : transform.position);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            basePos - Vector3.up * amplitude,
            basePos + Vector3.up * amplitude
        );
        Gizmos.DrawWireSphere(basePos, 0.1f);
    }
#endif

}
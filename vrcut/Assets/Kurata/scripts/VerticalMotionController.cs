using UnityEngine;

public class VerticalMotionController : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private float amplitude = 1f; // 上下の幅
    [SerializeField] private float speed = 1f;     // 移動速度
    [SerializeField] private bool useLocal = true; // ローカル座標を使用

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        // 初期位置を記憶
        startPosition = useLocal ? transform.localPosition : transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime * speed;

        // Y座標を計算
        float newY = startPosition.y + Mathf.Sin(timer) * amplitude;

        // 位置を更新
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

    // パラメータを変更するメソッド（必要に応じて使用）
    public void SetParameters(float newSpeed, float newAmplitude)
    {
        speed = newSpeed;
        amplitude = newAmplitude;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 動きの範囲を可視化
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
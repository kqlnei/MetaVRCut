using UnityEngine;

public class PlayerMovementTracker : MonoBehaviour
{
    public Vector3 CurrentVelocity { get; private set; }

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 毎フレーム位置から速度を計算
        CurrentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}

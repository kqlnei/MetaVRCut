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
        // ���t���[���ʒu���瑬�x���v�Z
        CurrentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}

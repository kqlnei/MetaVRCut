using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj : MonoBehaviour
{
    // Start is called before the first frame update
    public float amplitude = 3f;   // Y軸の振幅（範囲：-3から3）
    public float speed = 1f;       // 変化の速度

    private Vector3 initialPosition;

    void Start()
    {
        // オブジェクトの初期位置を記憶
        initialPosition = transform.position;
    }

    void Update()
    {
        // Mathf.Sinを使用して、Y軸の位置を変化させる
        float newY = initialPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }
}

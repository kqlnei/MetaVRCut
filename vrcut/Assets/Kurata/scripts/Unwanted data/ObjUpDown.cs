using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj : MonoBehaviour
{
    // Start is called before the first frame update
    public float amplitude = 3f;   // Y���̐U���i�͈́F-3����3�j
    public float speed = 1f;       // �ω��̑��x

    private Vector3 initialPosition;

    void Start()
    {
        // �I�u�W�F�N�g�̏����ʒu���L��
        initialPosition = transform.position;
    }

    void Update()
    {
        // Mathf.Sin���g�p���āAY���̈ʒu��ω�������
        float newY = initialPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }
}

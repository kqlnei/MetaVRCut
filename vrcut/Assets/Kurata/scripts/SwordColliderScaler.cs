using UnityEngine;

public class SwordColliderScaler : MonoBehaviour
{
    public Transform handle; // ���̎������Transform�i��_�j
    public float baseLength = 1.0f; // ���̒���
    public float maxStretchFactor = 2.0f; // �L�΂��ő�{��
    public float velocityThreshold = 2.0f; // �L�юn�߂鑬�x�̂������l

    private BoxCollider boxCollider;
    private Vector3 previousPosition;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        previousPosition = transform.position;
    }

    void Update()
    {
        // ���̑��x���v�Z
        float velocity = (transform.position - previousPosition).magnitude / Time.deltaTime;

        // ���x�ɉ�����BoxCollider�̃T�C�Y��ύX
        float stretchFactor = Mathf.Clamp(1 + (velocity / velocityThreshold), 1, maxStretchFactor);
        boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, baseLength * stretchFactor);

        // �R���C�_�[�̈ʒu�����i�������L�т镪�A���S�����炷�j
        boxCollider.center = new Vector3(0, 0, (boxCollider.size.z - baseLength) / 2);

        previousPosition = transform.position;
    }
}

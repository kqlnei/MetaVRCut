/*************************************************************
 * 
 * ���̃X�N���v�g�͍ŏI�I�Ɏg�p����Ă��܂���B
 * 
 *************************************************************/


using UnityEngine;
using BLINDED_AM_ME;
public class Cutter : MonoBehaviour
{
    public GameObject victim;           // �ؒf�Ώۂ̃I�u�W�F�N�g
    public GameObject cuttingPlane;     // �ؒf�ʂ������v���[��
    public Material capMaterial;        // �ؒf�ʂɎg�p����}�e���A��
    public Vector3 cuttingBoxSize = new Vector3(2, 0.01f, 2);  // �ؒf�ʂ͈̔�

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PerformCut(victim);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Cuttable"))
        {
            PerformCut(other.gameObject);
        }
    }

    void PerformCut(GameObject target)
    {
        Vector3 anchorPoint = cuttingPlane.transform.position;
        Vector3 normalDirection = cuttingPlane.transform.up;

        // �^�[�Q�b�g�̃��b�V���o�E���h���擾
        MeshRenderer targetRenderer = target.GetComponent<MeshRenderer>();
        if (targetRenderer != null)
        {
            // �ؒf�ʂ̃v���[�����쐬
            Plane cuttingPlaneObject = new Plane(normalDirection, anchorPoint);

            // �I�u�W�F�N�g�̃o�E���h�{�b�N�X���擾
            Bounds targetBounds = targetRenderer.bounds;

            // �I�u�W�F�N�g�̃o�E���h�{�b�N�X���ؒf�ʂɐG��Ă��邩�m�F
            if (IsTouchingCuttingBox(targetBounds, cuttingPlane.transform, cuttingBoxSize))
            {
                GameObject[] pieces = MeshCut.Cut(target, anchorPoint, normalDirection, capMaterial);

                if (pieces != null)
                {
                    foreach (GameObject piece in pieces)
                    {
                        Rigidbody rb = piece.AddComponent<Rigidbody>();
                        rb.mass = 1;
                        rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
                        victim = piece;
                    }
                }
            }
            else
            {
                Debug.Log("�I�u�W�F�N�g���ؒf�ʂɐG��Ă��܂���B");
            }
        }
    }

    // �M�Y���͈̔͂ɃI�u�W�F�N�g�̃o�E���h�{�b�N�X���G��Ă��邩�m�F
    bool IsTouchingCuttingBox(Bounds bounds, Transform cuttingPlaneTransform, Vector3 boxSize)
    {
        // �I�u�W�F�N�g�̃o�E���h�{�b�N�X�̒��S���M�Y���̃��[�J����Ԃɕϊ�
        Vector3 localCenter = cuttingPlaneTransform.InverseTransformPoint(bounds.center);

        // ���[�J����Ԃ̃o�E���h�{�b�N�X�̃T�C�Y�𔼕��ɂ��Čv�Z
        Vector3 halfBoxSize = boxSize * 0.5f;

        // ���[�J����Ԃɂ����āA�o�E���h�{�b�N�X���M�Y���͈͓̔��ɂ��邩���m�F
        if (Mathf.Abs(localCenter.x) < halfBoxSize.x + bounds.extents.x &&
            Mathf.Abs(localCenter.y) < halfBoxSize.y + bounds.extents.y &&
            Mathf.Abs(localCenter.z) < halfBoxSize.z + bounds.extents.z)
        {
            return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (cuttingPlane != null)
        {
            Vector3 anchorPoint = cuttingPlane.transform.position;

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(anchorPoint, cuttingPlane.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cuttingBoxSize);
        }
    }
}

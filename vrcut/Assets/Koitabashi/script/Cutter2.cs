/*************************************************************
 * 
 * ���̃X�N���v�g�͍ŏI�I�Ɏg�p����Ă��܂���B
 * 
 *************************************************************/

using UnityEngine;
using BLINDED_AM_ME;

public class Cutter2 : MonoBehaviour
{
    public GameObject cuttingPlane;     // �ؒf�ʂ������v���[��
    public Material capMaterial;        // �ؒf�ʂɎg�p����}�e���A��
    public Vector3 cuttingBoxSize = new Vector3(2, 0.01f, 2);  // �ؒf�ʂ͈̔�
    public string targetTag = "Cuttable"; // �ؒf�\�ȃI�u�W�F�N�g�̃^�O

    void OnTriggerEnter(Collider other)
    {

        // �Ώۂ�����̃^�O�������Ă��邩�m�F
        if (other.CompareTag(targetTag))
        {
            
            GameObject target = other.gameObject;
            
            // �M�Y�����ΏۃI�u�W�F�N�g�ɐG��Ă��邩�m�F
            if (IsTouchingCuttingBox(target.GetComponent<Renderer>().bounds, cuttingPlane.transform, cuttingBoxSize))
            {
                Debug.Log("�G��Ă܂�");
                PerformCut(target);
            }
        }
    }

    void PerformCut(GameObject target)
    {
        Vector3 anchorPoint = cuttingPlane.transform.position;
        Vector3 normalDirection = cuttingPlane.transform.up;

        // �ؒf�ʂ̃v���[�����쐬
        Plane cuttingPlaneObject = new Plane(normalDirection, anchorPoint);

        // MeshCut���Ăяo���A�ΏۃI�u�W�F�N�g��ؒf
        GameObject[] pieces = MeshCut.Cut(target, anchorPoint, normalDirection, capMaterial);

        if (pieces != null)
        {
            foreach (GameObject piece in pieces)
            {
                // �e�ؒf���ꂽ�s�[�X��Rigidbody��ǉ����A�������Z��K�p
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = 1;
                rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
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

        // �o�E���h�{�b�N�X�̃G�N�X�e���g���l�������v�Z�ŁA�ꕔ�ł��͈͓��ɐG��Ă��邩�`�F�b�N
        if (Mathf.Abs(localCenter.x) - bounds.extents.x < halfBoxSize.x &&
            Mathf.Abs(localCenter.y) - bounds.extents.y < halfBoxSize.y &&
            Mathf.Abs(localCenter.z) - bounds.extents.z < halfBoxSize.z)
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

            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.TRS(anchorPoint, cuttingPlane.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cuttingBoxSize);
        }
    }
}

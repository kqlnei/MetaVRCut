
/*************************************************************
 * 
 * ���̃X�N���v�g�͍ŏI�I�Ɏg�p����Ă��܂���B
 * 
 *************************************************************/
using UnityEngine;
using BLINDED_AM_ME;
using System.Collections.Generic;

public class Cutter3 : MonoBehaviour
{
    public GameObject cuttingPlane;     // �ؒf�ʂ������v���[��
    public Material capMaterial;        // �ؒf�ʂɎg�p����}�e���A��
    public Vector3 cuttingBoxSize = new Vector3(2, 0.01f, 2);  // �ؒf�ʂ͈̔�
    public string targetTag = "Cuttable"; // �ؒf�\�ȃI�u�W�F�N�g�̃^�O
    private HashSet<GameObject> alreadyCutObjects = new HashSet<GameObject>(); // �ؒf�ς݂̃I�u�W�F�N�g���Ǘ�

    private void OnTriggerEnter(Collider other)
    {
        // �I�u�W�F�N�g�� "Cuttable" �^�O�������Ă��āA���܂��ؒf����Ă��Ȃ����m�F
        if (other.CompareTag(targetTag) && !alreadyCutObjects.Contains(other.gameObject))
        {
            // �I�u�W�F�N�g�̃o�E���h�{�b�N�X���擾
            MeshRenderer targetRenderer = other.GetComponent<MeshRenderer>();
            if (targetRenderer != null)
            {
                Bounds targetBounds = targetRenderer.bounds;

                // �ؒf�ʂ̃o�E���h�{�b�N�X���쐬���Č���������s��
                Bounds cuttingBounds = new Bounds(cuttingPlane.transform.position, cuttingBoxSize);

                if (cuttingBounds.Intersects(targetBounds)) // �������Ă���ΐؒf
                {
                    PerformCut(other.gameObject);
                    alreadyCutObjects.Add(other.gameObject); // �ؒf�ς݂Ƃ��ēo�^
                }
                else
                {
                    Debug.Log("�I�u�W�F�N�g���ؒf�ʂɐG��Ă��܂���B");
                }
            }
        }
    }

    void PerformCut(GameObject target)
    {
        Vector3 anchorPoint = cuttingPlane.transform.position;
        Vector3 normalDirection = cuttingPlane.transform.up;

        GameObject[] pieces = MeshCut.Cut(target, anchorPoint, normalDirection, capMaterial);

        if (pieces != null)
        {
            foreach (GameObject piece in pieces)
            {
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = 1;
                rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
                alreadyCutObjects.Add(piece); // �V���ɐ������ꂽ�s�[�X���ؒf�ς݂Ƃ��ēo�^
            }
        }
    }

    void OnDrawGizmos()
    {
        if (cuttingPlane != null)
        {
            Vector3 anchorPoint = cuttingPlane.transform.position;

            Gizmos.color = Color.black;
            Gizmos.matrix = Matrix4x4.TRS(anchorPoint, cuttingPlane.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, cuttingBoxSize);
        }
    }
}

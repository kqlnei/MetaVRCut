/*using UnityEngine;

public class SwordPlaneGenerator : MonoBehaviour
{
    // �ΏۃI�u�W�F�N�g��ؒf���邽�߂̕��ʂ𐶐�
    public void GeneratePlane(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection)
    {
        // victim�I�u�W�F�N�g�̃��[�J����Ԃɕϊ�
        Vector3 localNormal = victim.transform.InverseTransformDirection(-normalDirection.normalized);
        Vector3 localPoint = victim.transform.InverseTransformPoint(anchorPoint);

        Plane blade = new Plane(localNormal, localPoint);

        Debug.Log($"Plane generated with normal: {blade.normal} and distance: {blade.distance}");

        // �f�o�b�O�p�ɐؒf�ʂ̏�������
        DebugDrawPlane(blade, victim.transform);
    }

    private void DebugDrawPlane(Plane plane, Transform victimTransform, float size = 1.0f)
    {
        // Plane�̖@�����擾
        Vector3 planeNormal = victimTransform.TransformDirection(plane.normal);
        Vector3 planePoint = victimTransform.TransformPoint(planeNormal * plane.distance);

        // Plane�𒆐S�ɉ���
        Vector3 right = Vector3.Cross(planeNormal, Vector3.up).normalized * size;
        Vector3 up = Vector3.Cross(right, planeNormal).normalized * size;

        // Plane�̎l�����v�Z���ĕ`��
        Vector3 p1 = planePoint + right + up;
        Vector3 p2 = planePoint + right - up;
        Vector3 p3 = planePoint - right - up;
        Vector3 p4 = planePoint - right + up;

        //�ؒf�ʂ�\��
        Debug.DrawLine(p1, p2, Color.red, 2.0f);
        Debug.DrawLine(p2, p3, Color.red, 2.0f);
        Debug.DrawLine(p3, p4, Color.red, 2.0f);
        Debug.DrawLine(p4, p1, Color.red, 2.0f);
    }

    // Plane��Cube�ŉ�������
    public void VisualizePlane(Plane plane, Transform victimTransform, float size = 1.0f, Color color = default)
    {
        if (color == default) color = Color.cyan;

        // Plane�̖@���Ɗ�_
        Vector3 planeNormal = victimTransform.TransformDirection(plane.normal);
        Vector3 planePoint = victimTransform.TransformPoint(-plane.distance * plane.normal);

        GameObject planeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        planeCube.transform.position = planePoint;

        // �@�������Ɋ�Â��ĉ�]
        planeCube.transform.rotation = Quaternion.LookRotation(planeNormal);

        // Cube�̃X�P�[����ݒ�iPlane�̑傫�����V�~�����[�V�����j
        planeCube.transform.localScale = new Vector3(size, 0.01f, size);

        Renderer renderer = planeCube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
    }
}
*/
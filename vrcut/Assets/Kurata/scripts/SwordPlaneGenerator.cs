/*using UnityEngine;

public class SwordPlaneGenerator : MonoBehaviour
{
    // 対象オブジェクトを切断するための平面を生成
    public void GeneratePlane(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection)
    {
        // victimオブジェクトのローカル空間に変換
        Vector3 localNormal = victim.transform.InverseTransformDirection(-normalDirection.normalized);
        Vector3 localPoint = victim.transform.InverseTransformPoint(anchorPoint);

        Plane blade = new Plane(localNormal, localPoint);

        Debug.Log($"Plane generated with normal: {blade.normal} and distance: {blade.distance}");

        // デバッグ用に切断面の情報を可視化
        DebugDrawPlane(blade, victim.transform);
    }

    private void DebugDrawPlane(Plane plane, Transform victimTransform, float size = 1.0f)
    {
        // Planeの法線を取得
        Vector3 planeNormal = victimTransform.TransformDirection(plane.normal);
        Vector3 planePoint = victimTransform.TransformPoint(planeNormal * plane.distance);

        // Planeを中心に可視化
        Vector3 right = Vector3.Cross(planeNormal, Vector3.up).normalized * size;
        Vector3 up = Vector3.Cross(right, planeNormal).normalized * size;

        // Planeの四隅を計算して描画
        Vector3 p1 = planePoint + right + up;
        Vector3 p2 = planePoint + right - up;
        Vector3 p3 = planePoint - right - up;
        Vector3 p4 = planePoint - right + up;

        //切断面を表示
        Debug.DrawLine(p1, p2, Color.red, 2.0f);
        Debug.DrawLine(p2, p3, Color.red, 2.0f);
        Debug.DrawLine(p3, p4, Color.red, 2.0f);
        Debug.DrawLine(p4, p1, Color.red, 2.0f);
    }

    // PlaneをCubeで可視化する
    public void VisualizePlane(Plane plane, Transform victimTransform, float size = 1.0f, Color color = default)
    {
        if (color == default) color = Color.cyan;

        // Planeの法線と基準点
        Vector3 planeNormal = victimTransform.TransformDirection(plane.normal);
        Vector3 planePoint = victimTransform.TransformPoint(-plane.distance * plane.normal);

        GameObject planeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        planeCube.transform.position = planePoint;

        // 法線方向に基づいて回転
        planeCube.transform.rotation = Quaternion.LookRotation(planeNormal);

        // Cubeのスケールを設定（Planeの大きさをシミュレーション）
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
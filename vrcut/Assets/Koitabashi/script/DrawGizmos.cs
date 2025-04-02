using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    // 平面のサイズ
    public Vector3 size = new Vector3(1, 0.01f, 1);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // 平面の4つの頂点を計算
        Vector3 topLeft = transform.position + transform.TransformDirection(new Vector3(-size.x / 2, 0, size.z / 2));
        Vector3 topRight = transform.position + transform.TransformDirection(new Vector3(size.x / 2, 0, size.z / 2));
        Vector3 bottomLeft = transform.position + transform.TransformDirection(new Vector3(-size.x / 2, 0, -size.z / 2));
        Vector3 bottomRight = transform.position + transform.TransformDirection(new Vector3(size.x / 2, 0, -size.z / 2));

        // 頂点を結んで四角形を描画
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}

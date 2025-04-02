using UnityEngine;
using System.Collections.Generic;

public class SimpleMeshCut : MonoBehaviour
{
    public static GameObject[] Cut(GameObject objectToCut, Vector3 cutPlanePosition, Vector3 cutPlaneNormal)
    {
        Mesh mesh = objectToCut.GetComponent<MeshFilter>().mesh;
        Plane cutPlane = new Plane(cutPlaneNormal, cutPlanePosition);

        List<Vector3> aboveVertices = new List<Vector3>();
        List<Vector3> belowVertices = new List<Vector3>();
        List<int> aboveTriangles = new List<int>();
        List<int> belowTriangles = new List<int>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 v1 = mesh.vertices[mesh.triangles[i]];
            Vector3 v2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 v3 = mesh.vertices[mesh.triangles[i + 2]];

            bool v1Above = cutPlane.GetSide(v1);
            bool v2Above = cutPlane.GetSide(v2);
            bool v3Above = cutPlane.GetSide(v3);

            if (v1Above && v2Above && v3Above)
            {
                AddTriangleToList(aboveVertices, aboveTriangles, v1, v2, v3);
            }
            else if (!v1Above && !v2Above && !v3Above)
            {
                AddTriangleToList(belowVertices, belowTriangles, v1, v2, v3);
            }
            else
            {
                CutTriangle(cutPlane, v1, v2, v3, v1Above, v2Above, v3Above,
                            aboveVertices, aboveTriangles, belowVertices, belowTriangles);
            }
        }

        GameObject aboveObject = CreateMeshObject(aboveVertices, aboveTriangles, "Above", objectToCut);
        GameObject belowObject = CreateMeshObject(belowVertices, belowTriangles, "Below", objectToCut);


        return new GameObject[] { aboveObject, belowObject };
    }

    private static void AddTriangleToList(List<Vector3> vertices, List<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    private static void CutTriangle(Plane plane, Vector3 v1, Vector3 v2, Vector3 v3, bool v1Above, bool v2Above, bool v3Above,
                                    List<Vector3> aboveVertices, List<int> aboveTriangles,
                                    List<Vector3> belowVertices, List<int> belowTriangles)
    {
        Vector3 intersect1, intersect2;

        if (v1Above != v2Above)
        {
            intersect1 = GetIntersection(plane, v1, v2);
            if (v1Above != v3Above)
            {
                intersect2 = GetIntersection(plane, v1, v3);
                AddTriangleToList(v1Above ? aboveVertices : belowVertices,
                                  v1Above ? aboveTriangles : belowTriangles,
                                  v1, intersect1, intersect2);
                AddTriangleToList(v2Above ? aboveVertices : belowVertices,
                                  v2Above ? aboveTriangles : belowTriangles,
                                  v2, v3, intersect1);
                AddTriangleToList(v3Above ? aboveVertices : belowVertices,
                                  v3Above ? aboveTriangles : belowTriangles,
                                  v3, intersect2, intersect1);
            }
            else
            {
                intersect2 = GetIntersection(plane, v2, v3);
                AddTriangleToList(v1Above ? aboveVertices : belowVertices,
                                  v1Above ? aboveTriangles : belowTriangles,
                                  v1, intersect1, v3);
                AddTriangleToList(v1Above ? aboveVertices : belowVertices,
                                  v1Above ? aboveTriangles : belowTriangles,
                                  intersect1, intersect2, v3);
                AddTriangleToList(v2Above ? aboveVertices : belowVertices,
                                  v2Above ? aboveTriangles : belowTriangles,
                                  v2, intersect2, intersect1);
            }
        }
        else
        {
            intersect1 = GetIntersection(plane, v1, v3);
            intersect2 = GetIntersection(plane, v2, v3);
            AddTriangleToList(v1Above ? aboveVertices : belowVertices,
                              v1Above ? aboveTriangles : belowTriangles,
                              v1, v2, intersect1);
            AddTriangleToList(v1Above ? aboveVertices : belowVertices,
                              v1Above ? aboveTriangles : belowTriangles,
                              v2, intersect2, intersect1);
            AddTriangleToList(v3Above ? aboveVertices : belowVertices,
                              v3Above ? aboveTriangles : belowTriangles,
                              v3, intersect1, intersect2);
        }
    }

    private static Vector3 GetIntersection(Plane plane, Vector3 v1, Vector3 v2)
    {
        float distance;
        Ray ray = new Ray(v1, v2 - v1);
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    private static GameObject CreateMeshObject(List<Vector3> vertices, List<int> triangles, string name, GameObject originalObject)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject newObject = new GameObject(name);
        newObject.AddComponent<MeshFilter>().mesh = mesh;
        newObject.AddComponent<MeshRenderer>();

        // 元のオブジェクトの位置と回転を新しいオブジェクトに適用
        newObject.transform.position = originalObject.transform.position;
        newObject.transform.rotation = originalObject.transform.rotation;

        return newObject;
    }

}
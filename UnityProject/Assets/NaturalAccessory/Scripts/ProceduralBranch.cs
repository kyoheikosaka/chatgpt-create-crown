using UnityEngine;

public static class ProceduralBranch
{
    public static GameObject Create(Transform parent, Vector3 start, Vector3 middle, Vector3 end, float thickness, Material material)
    {
        GameObject obj = new GameObject("Branch");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = GenerateMesh(start, middle, end, thickness);

        if (material != null) renderer.sharedMaterial = material;
        return obj;
    }

    private static Mesh GenerateMesh(Vector3 start, Vector3 middle, Vector3 end, float thickness)
    {
        const int sides = 6;
        Vector3[] points = { start, middle, end };
        Vector3[] vertices = new Vector3[points.Length * sides];
        int[] triangles = new int[(points.Length - 1) * sides * 6];

        for (int p = 0; p < points.Length; p++)
        {
            Vector3 tangent = p == 0 ? points[1] - points[0] :
                              p == points.Length - 1 ? points[p] - points[p - 1] :
                              points[p + 1] - points[p - 1];
            tangent.Normalize();

            Vector3 normal = Vector3.Cross(tangent, Vector3.up);
            if (normal.sqrMagnitude < 0.001f) normal = Vector3.Cross(tangent, Vector3.right);
            normal.Normalize();
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

            for (int s = 0; s < sides; s++)
            {
                float angle = (Mathf.PI * 2f / sides) * s;
                Vector3 offset = (normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle)) * thickness;
                vertices[p * sides + s] = points[p] + offset;
            }
        }

        int triangleIndex = 0;
        for (int p = 0; p < points.Length - 1; p++)
        {
            for (int s = 0; s < sides; s++)
            {
                int next = (s + 1) % sides;
                int a = p * sides + s;
                int b = p * sides + next;
                int c = (p + 1) * sides + next;
                int d = (p + 1) * sides + s;

                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = d;
                triangles[triangleIndex++] = c;
            }
        }

        Mesh mesh = new Mesh { name = "ProceduralBranch" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

using UnityEngine;

public static class ProceduralBranch
{
    public static GameObject Create(
        Transform parent,
        Vector3[] points,
        float rootThickness,
        float tipThickness,
        Material material,
        string objectName = "Branch")
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = GenerateMesh(
            points,
            rootThickness,
            tipThickness
        );

        if (material != null)
            renderer.sharedMaterial = material;

        return obj;
    }

    private static Mesh GenerateMesh(
        Vector3[] points,
        float rootThickness,
        float tipThickness)
    {
        const int sides = 8;

        if (points == null || points.Length < 2)
            return new Mesh { name = "ProceduralBranch_Empty" };

        Vector3[] vertices = new Vector3[points.Length * sides];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(points.Length - 1) * sides * 6];

        for (int p = 0; p < points.Length; p++)
        {
            Vector3 tangent;

            if (p == 0)
                tangent = points[1] - points[0];
            else if (p == points.Length - 1)
                tangent = points[p] - points[p - 1];
            else
                tangent = points[p + 1] - points[p - 1];

            tangent.Normalize();

            Vector3 reference = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(tangent, reference)) > 0.95f)
                reference = Vector3.right;

            Vector3 normal = Vector3.Cross(tangent, reference).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

            float t = p / (float)(points.Length - 1);
            float thickness = Mathf.Lerp(rootThickness, tipThickness, t);

            for (int s = 0; s < sides; s++)
            {
                float angle = s * Mathf.PI * 2f / sides;
                Vector3 offset =
                    normal * Mathf.Cos(angle) * thickness +
                    binormal * Mathf.Sin(angle) * thickness;

                vertices[p * sides + s] = points[p] + offset;
                uvs[p * sides + s] = new Vector2(
                    s / (float)sides,
                    t
                );
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

        Mesh mesh = new Mesh
        {
            name = "ProceduralBranch"
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

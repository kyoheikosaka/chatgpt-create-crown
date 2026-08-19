using UnityEngine;

public static class ProceduralLeaf
{
    public static GameObject Create(Transform parent, Vector3 position, Vector3 direction, float width, float length, Material material)
    {
        GameObject obj = new GameObject("Leaf");
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.LookRotation(direction);

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = GenerateLeaf(width, length);

        if (material != null) renderer.sharedMaterial = material;
        return obj;
    }

    private static Mesh GenerateLeaf(float width, float length)
    {
        Vector3[] vertices =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(-width, length * 0.45f, 0f),
            new Vector3(0f, length, 0f),
            new Vector3(width, length * 0.45f, 0f),
            new Vector3(0f, length * 0.5f, 0.01f)
        };

        int[] triangles =
        {
            0, 1, 4,
            1, 2, 4,
            2, 3, 4,
            3, 0, 4
        };

        Mesh mesh = new Mesh { name = "ProceduralLeaf" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

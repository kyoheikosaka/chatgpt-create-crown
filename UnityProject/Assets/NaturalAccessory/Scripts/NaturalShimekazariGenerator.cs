using UnityEngine;

[ExecuteAlways]
public class NaturalShimekazariGenerator : MonoBehaviour
{
    [Header("=== Ring ===")]
    [Min(0.03f)] public float radius = 0.15f;
    [Min(3)] public int mainBranchCount = 20;

    [Header("=== Main Branch ===")]
    [Min(0.005f)] public float mainBranchThickness = 0.012f;
    [Min(0.01f)] public float mainBranchLength = 0.08f;
    [Range(0f, 1f)] public float mainCurve = 0.35f;

    [Header("=== Sub Branch ===")]
    [Min(0)] public int subBranchesPerMain = 2;
    [Min(0.002f)] public float subBranchThickness = 0.006f;
    [Min(0.005f)] public float subBranchLength = 0.045f;
    [Range(0f, 1f)] public float subBranchCurve = 0.4f;

    [Header("=== Leaves ===")]
    public bool generateLeaves = true;
    [Min(0)] public int leavesPerSubBranch = 2;
    [Min(0.002f)] public float leafWidth = 0.012f;
    [Min(0.002f)] public float leafLength = 0.025f;

    [Header("=== Natural Randomness ===")]
    [Range(0f, 1f)] public float randomness = 0.35f;
    public int seed = 12345;

    [Header("=== Materials ===")]
    public Material branchMaterial;
    public Material leafMaterial;

    [Header("=== Generation ===")]
    public bool generateOnStart = true;

    private Transform generatedRoot;
    private System.Random random;

    private void Start()
    {
        if (generateOnStart) Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        random = new System.Random(seed);

        generatedRoot = new GameObject("Generated_Shimekazari").transform;
        generatedRoot.SetParent(transform);
        generatedRoot.localPosition = Vector3.zero;
        generatedRoot.localRotation = Quaternion.identity;
        generatedRoot.localScale = Vector3.one;

        for (int i = 0; i < mainBranchCount; i++) GenerateMainBranch(i);
    }

    private void GenerateMainBranch(int index)
    {
        float angle = (360f / mainBranchCount) * index;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 start = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
        Vector3 tangent = new Vector3(-Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        Vector3 outward = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

        Vector3 direction = tangent;
        direction += outward * RandomFloat(-randomness, randomness);
        direction.y += RandomFloat(-randomness * 0.5f, randomness * 0.5f);
        direction.Normalize();

        Vector3 end = start + direction * mainBranchLength;
        Vector3 middle = Vector3.Lerp(start, end, 0.5f);
        Vector3 curveDirection = Vector3.Cross(direction, Vector3.up);
        middle += curveDirection * RandomFloat(-mainCurve, mainCurve) * mainBranchLength;
        middle += Vector3.up * RandomFloat(-mainCurve, mainCurve) * mainBranchLength;

        ProceduralBranch.Create(generatedRoot, start, middle, end, mainBranchThickness, branchMaterial);

        for (int i = 0; i < subBranchesPerMain; i++) GenerateSubBranch(start, end, direction);
    }

    private void GenerateSubBranch(Vector3 mainStart, Vector3 mainEnd, Vector3 mainDirection)
    {
        float position = RandomFloat(0.2f, 0.9f);
        Vector3 start = Vector3.Lerp(mainStart, mainEnd, position);
        Vector3 direction = Quaternion.AngleAxis(RandomFloat(-65f, 65f), Vector3.up) * mainDirection;
        direction += Vector3.up * RandomFloat(-0.5f, 0.8f);
        direction.Normalize();

        Vector3 end = start + direction * subBranchLength;
        Vector3 middle = Vector3.Lerp(start, end, 0.5f);
        middle += Random.insideUnitSphere * subBranchLength * subBranchCurve;

        ProceduralBranch.Create(generatedRoot, start, middle, end, subBranchThickness, branchMaterial);

        if (generateLeaves) GenerateLeaves(start, end, direction);
    }

    private void GenerateLeaves(Vector3 start, Vector3 end, Vector3 direction)
    {
        for (int i = 0; i < leavesPerSubBranch; i++)
        {
            float t = RandomFloat(0.25f, 1f);
            Vector3 position = Vector3.Lerp(start, end, t);
            Vector3 leafDirection = direction + Random.insideUnitSphere * randomness;
            leafDirection.Normalize();
            ProceduralLeaf.Create(generatedRoot, position, leafDirection, leafWidth, leafLength, leafMaterial);
        }
    }

    private float RandomFloat(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Transform oldRoot = transform.Find("Generated_Shimekazari");
        if (oldRoot == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(oldRoot.gameObject);
        else Destroy(oldRoot.gameObject);
#else
        Destroy(oldRoot.gameObject);
#endif
        generatedRoot = null;
    }
}

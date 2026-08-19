using UnityEngine;

[ExecuteAlways]
public class NaturalShimekazariGenerator : MonoBehaviour
{
    [Header("=== Ring ===")]
    [Min(0.03f)] public float radius = 0.15f;
    [Min(4)] public int mainBranchCount = 18;
    [Min(3)] public int pointsPerMainBranch = 6;
    [Range(0f, 0.08f)] public float ringWobble = 0.015f;
    [Range(0f, 0.05f)] public float verticalWobble = 0.008f;

    [Header("=== Main Branch ===")]
    [Min(0.001f)] public float mainBranchThickness = 0.012f;
    [Range(0.05f, 1f)] public float mainTaper = 0.45f;
    [Range(0f, 0.1f)] public float mainCurve = 0.025f;

    [Header("=== Sub Branch ===")]
    [Range(0f, 2f)] public float subBranchesPerMain = 0.7f;
    [Min(0.001f)] public float subBranchThickness = 0.006f;
    [Min(0.005f)] public float subBranchLength = 0.05f;
    [Range(0.1f, 1f)] public float subBranchTaper = 0.5f;
    [Range(0f, 1f)] public float subBranchCurve = 0.45f;

    [Header("=== Natural Randomness ===")]
    [Range(0f, 1f)] public float randomness = 0.3f;
    public int seed = 12345;

    [Header("=== Material ===")]
    public Material branchMaterial;

    [Header("=== Generation ===")]
    public bool autoGenerate = true;
    public bool generateOnStart = true;

    private Transform generatedRoot;
    private bool isGenerating;

    private void Start()
    {
        if (generateOnStart && Application.isPlaying)
            Generate();
    }

    private void OnValidate()
    {
        if (!autoGenerate || isGenerating)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.delayCall += DelayedGenerate;
#endif
    }

#if UNITY_EDITOR
    private void DelayedGenerate()
    {
        if (this == null || Application.isPlaying || !autoGenerate || isGenerating)
            return;

        Generate();
    }
#endif

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (isGenerating)
            return;

        isGenerating = true;

        Clear();

        generatedRoot = new GameObject("Generated_Shimekazari").transform;
        generatedRoot.SetParent(transform, false);

        Random.InitState(seed);

        for (int i = 0; i < mainBranchCount; i++)
            GenerateMainBranch(i);

        isGenerating = false;
    }

    private void GenerateMainBranch(int index)
    {
        float startAngle = index * 360f / mainBranchCount;
        float endAngle = (index + 1) * 360f / mainBranchCount;
        float angleStep = (endAngle - startAngle) / (pointsPerMainBranch - 1);

        Vector3[] points = new Vector3[pointsPerMainBranch];

        for (int p = 0; p < pointsPerMainBranch; p++)
        {
            float angle = startAngle + angleStep * p;
            float rad = angle * Mathf.Deg2Rad;

            float wave = Mathf.Sin(rad * 3f + seed * 0.013f) * ringWobble;
            float smallWave = Mathf.Sin(rad * 7f + seed * 0.031f) * ringWobble * 0.35f;
            float localRadius = radius + wave + smallWave;

            Vector3 point = new Vector3(
                Mathf.Cos(rad) * localRadius,
                Mathf.Sin(rad * 2.7f + seed * 0.021f) * verticalWobble,
                Mathf.Sin(rad) * localRadius
            );

            if (p > 0 && p < pointsPerMainBranch - 1)
            {
                Vector3 outward = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
                point += outward * Random.Range(-1f, 1f) * mainCurve * randomness * 0.35f;
                point.y += Random.Range(-1f, 1f) * mainCurve * randomness * 0.2f;
            }

            points[p] = point;
        }

        ProceduralBranch.Create(
            generatedRoot,
            points,
            mainBranchThickness,
            mainBranchThickness * mainTaper,
            branchMaterial,
            "MainBranch"
        );

        if (Random.value <= subBranchesPerMain / 2f)
        {
            int attachIndex = Random.Range(1, pointsPerMainBranch - 1);
            GenerateSubBranch(points[attachIndex]);
        }
    }

    private void GenerateSubBranch(Vector3 start)
    {
        Vector3 outward = new Vector3(start.x, 0f, start.z).normalized;
        Vector3 tangent = new Vector3(-outward.z, 0f, outward.x);

        Vector3 direction = outward * Random.Range(0.55f, 1f);
        direction += tangent * Random.Range(-0.65f, 0.65f) * randomness;
        direction.y += Random.Range(0.1f, 0.8f);
        direction.Normalize();

        int pointCount = 5;
        Vector3[] points = new Vector3[pointCount];
        points[0] = start;

        Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;
        if (side.sqrMagnitude < 0.001f)
            side = Vector3.right;

        for (int i = 1; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 point = start + direction * (subBranchLength * t);
            point += side * Mathf.Sin(t * Mathf.PI) * subBranchLength * subBranchCurve * randomness;
            point += Vector3.up * Mathf.Sin(t * Mathf.PI * 0.8f) * subBranchLength * 0.15f;
            points[i] = point;
        }

        ProceduralBranch.Create(
            generatedRoot,
            points,
            subBranchThickness,
            subBranchThickness * subBranchTaper,
            branchMaterial,
            "SubBranch"
        );
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Transform oldRoot = transform.Find("Generated_Shimekazari");
        if (oldRoot == null)
        {
            generatedRoot = null;
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(oldRoot.gameObject);
        else
            Destroy(oldRoot.gameObject);
#else
        Destroy(oldRoot.gameObject);
#endif

        generatedRoot = null;
    }
}

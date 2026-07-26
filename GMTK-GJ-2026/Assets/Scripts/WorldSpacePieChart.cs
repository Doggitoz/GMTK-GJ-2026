using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldSpacePieChart : MonoBehaviour
{
    [Range(0f, 1f)]
    public float Percentage = 0.75f;

    [Min(0.01f)]
    public float Radius = 1f;

    [Tooltip("Gap as a percentage of the full circle (0.02 = 2%)")]
    [Range(0f, 0.25f)]
    public float GapPercent = 0.02f;

    [Tooltip("Number of subdivisions for a full circle")]
    [Range(8, 256)]
    public int CircleResolution = 64;

    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Pie Mesh";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf.sharedMesh == null)
            {
                mesh = new Mesh();
                mf.sharedMesh = mesh;
            }
            else
            {
                mesh = mf.sharedMesh;
            }
        }

        BuildMesh();
    }

    void Start()
    {
        BuildMesh();
    }

    public void SetPercentage(float percent)
    {
        Percentage = Mathf.Clamp01(percent);
        BuildMesh();
    }

    public void BuildMesh()
    {
        if (mesh == null)
            return;

        mesh.Clear();

        if (Percentage <= 0f)
            return;

        float sweep = Mathf.PI * 2f * Percentage;

        // Apply gap to both ends
        float gapAngle = GapPercent * Mathf.PI * 2f;

        if (sweep <= gapAngle)
            return;

        float startAngle = gapAngle * 0.5f;
        float endAngle = sweep - gapAngle * 0.5f;

        int arcSegments = Mathf.Max(2,
            Mathf.CeilToInt(CircleResolution * (endAngle - startAngle) / (Mathf.PI * 2f)));

        Vector3[] vertices = new Vector3[arcSegments + 2];
        int[] triangles = new int[arcSegments * 3];
        Vector2[] uv = new Vector2[vertices.Length];

        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(.5f, .5f);

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            float x = Mathf.Cos(angle) * Radius;
            float y = Mathf.Sin(angle) * Radius;

            vertices[i + 1] = new Vector3(x, y, 0);
            uv[i + 1] = new Vector2(
                x / (Radius * 2f) + .5f,
                y / (Radius * 2f) + .5f);
        }

        for (int i = 0; i < arcSegments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
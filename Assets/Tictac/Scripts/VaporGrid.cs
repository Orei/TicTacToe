using System.Collections;
using UnityEngine;

/// <summary>
/// Dynamic vaporwave-type grid.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VaporGrid : MonoBehaviour
{
    public static VaporGrid Instance { get; private set; }

    [Tooltip("Size of the grid, total number of cells is gridSize * gridSize.")]
    [SerializeField] private int gridSize = 100;
    [Tooltip("Scales the grid cells, each cell is 1 unit by default.")]
    [SerializeField] private float scale = 1f;
    private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Coroutine routine = null;

    private MeshFilter filter = null;
    private new MeshRenderer renderer = null;

    private Vector3[] vertices = null;
    private int[] triangles = null;
    private Vector2[] uvs = null;
    private Vector3[] normals = null;

    public int NumCells => gridSize * gridSize;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("An instance of VaporGrid already exists.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();

        int tiles = gridSize * gridSize;

        vertices = new Vector3[tiles * 4];
        triangles = new int[tiles * 6];
        uvs = new Vector2[tiles * 4];
        normals = new Vector3[tiles * 4];

        // Create triangles
		for (int i = 0; i < triangles.Length; i += 6)
		{
            // Each quad uses 4 vertices, we map the triangles to these vertices
            var index = (i / 6) * 4;

            triangles[i] = index;
            triangles[i + 1] = index + 2;
            triangles[i + 2] = index + 1;

            triangles[i + 3] = index;
            triangles[i + 4] = index + 3;
            triangles[i + 5] = index + 2;
		}

        // Create uvs
        for (int i = 0; i < uvs.Length; i += 4)
        {
            uvs[i] = new Vector2(1f, 0f);
            uvs[i + 1] = new Vector2(0f, 0f);
            uvs[i + 2] = new Vector2(0f, 1f);
            uvs[i + 3] = new Vector2(1f, 1f);
        }

        // Create tiles
        for (int i = 0; i < tiles; i += 4)
        {
            var x = (i % gridSize) / 4;
            var z = (i / gridSize) / 4;

            var forward = Vector3.forward / 2f;
            var right = Vector3.right / 2f;
            var position = new Vector3(x, 0f, z);
            var offset = (right + forward) * (gridSize / 4f);

            position -= offset;

            vertices[i]     = (position + right + forward) * scale;
            vertices[i + 1] = (position - right + forward) * scale;
            vertices[i + 2] = (position - right - forward) * scale;
            vertices[i + 3] = (position + right - forward) * scale;
        }

        // Create the mesh with the data we just generated
        filter.mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs,
        };

        // We need to recalculate the bounds after changing the vertices
        filter.mesh.RecalculateBounds();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < NumCells; i += 4)
        {
            vertices[i + 0] = GetVertexXZ(i + 0) + GetVertexY(i + 0);
            vertices[i + 1] = GetVertexXZ(i + 1) + GetVertexY(i + 1);
            vertices[i + 2] = GetVertexXZ(i + 2) + GetVertexY(i + 2);
            vertices[i + 3] = GetVertexXZ(i + 3) + GetVertexY(i + 3);
        }

        filter.mesh.vertices = vertices;
        filter.mesh.RecalculateBounds();
    }

    private Vector3 GetVertexXZ(int index)
    {
        var position = vertices[index];
        position.y = 0f;

        return position;
    }

    private Vector3 GetVertexY(int index)
    {
        // Generate noise from the position of the vertex and some sin/cos animation
        var position = GetVertexXZ(index);
        var noise = Mathf.PerlinNoise(position.x, position.z) * 2f;
        var sin = Mathf.Sin((position.x + position.z) * Time.time / 50f);
        var cos = Mathf.Cos((position.x - position.z) * Time.time / 50f);
        var offset = Vector3.up * noise + Vector3.up * sin * cos;

        return offset;
    }

    public void SetColor(Color color, float duration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(_SetColor(color, duration));
    }

    private IEnumerator _SetColor(Color color, float duration)
    {
        Color original = renderer.material.GetColor("_WireColor");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Clamp01(timer / duration);
            float ease = curve.Evaluate(alpha);
            Color lerp = Color.Lerp(original, color, ease);
            renderer.material.SetColor("_WireColor", lerp);

            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
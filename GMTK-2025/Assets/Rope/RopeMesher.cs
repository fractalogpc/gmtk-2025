using UnityEngine;

public class RopeMesher : MonoBehaviour
{
    [Tooltip("Drag your segment transforms here, in order.")]
    public Transform[] segments; // e.g. 10 segments

    [Tooltip("Half the width of the rope cross-section.")]
    public float radius = 0.1f;

    [Tooltip("Local-space offset applied to each ring center.")]
    public Vector3 offset = Vector3.zero;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Awake()
    {
        mesh = new Mesh { name = "RopeMesh3D" };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        if (segments == null || segments.Length < 2) return;
        BuildMesh();
    }

    void BuildMesh()
    {
        int segCount = segments.Length;
        int vertCount = segCount * 4;
        int triCount = (segCount - 1) * 4 * 2;

        // Allocate arrays if needed
        if (vertices == null || vertices.Length != vertCount)
        {
            vertices  = new Vector3[vertCount];
            uvs       = new Vector2[vertCount];
            triangles = new int[triCount * 3];
        }

        // Precompute world offset
        Vector3 worldOffset = transform.TransformDirection(offset);

        // Build vertices & UVs
        for (int i = 0; i < segCount; i++)
        {
            Vector3 pos = segments[i].position;

            // Determine tangent direction
            Vector3 tangent;
            if (i < segCount - 1)
                tangent = (segments[i + 1].position - pos).normalized;
            else
                tangent = (pos - segments[i - 1].position).normalized;

            // Build coordinate frame: forward = tangent
            Quaternion ringRot = Quaternion.LookRotation(tangent, Vector3.up);

            // Square corners in ring-local space
            Vector3[] localCorners = new Vector3[]
            {
                new Vector3( radius,  0f, 0f),
                new Vector3( 0f,  radius, 0f),
                new Vector3(-radius,  0f, 0f),
                new Vector3( 0f, -radius, 0f)
            };

            for (int j = 0; j < 4; j++)
            {
                int vi = i * 4 + j;
                // Transform corner into world space
                vertices[vi] = pos + worldOffset + ringRot * localCorners[j];
                // UV: U around ring (0, .33, .66, 1), V along rope
                uvs[vi] = new Vector2(j / 3f, i / (float)(segCount - 1));
            }
        }

        // Build triangles
        int ti = 0;
        for (int i = 0; i < segCount - 1; i++)
        {
            int a = i * 4;
            int b = (i + 1) * 4;
            for (int j = 0; j < 4; j++)
            {
                int a0 = a + j;
                int a1 = a + (j + 1) % 4;
                int b0 = b + j;
                int b1 = b + (j + 1) % 4;

                // First triangle
                triangles[ti++] = a0;
                triangles[ti++] = b0;
                triangles[ti++] = b1;
                // Second triangle
                triangles[ti++] = a0;
                triangles[ti++] = b1;
                triangles[ti++] = a1;
            }
        }

        // Assign mesh data
        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uvs;
        mesh.RecalculateNormals();
    }
}

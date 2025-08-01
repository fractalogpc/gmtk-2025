using UnityEngine;

public class RopeMesher : MonoBehaviour
{
    [Tooltip("Drag your segment transforms here, in order.")]
    public Transform[] segments; // e.g. 10 segments

    [Tooltip("Half the width of the rope cross-section.")]
    public float radius = 0.1f;

    [Tooltip("Whether the rope should loop back to the first segment.")]
    public bool loop = false;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Awake()
    {
        mesh = new Mesh { name = "RopeMesh3D" };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetSegments(Transform[] newSegments)
    {
        segments = newSegments;
        BuildMesh();
    }

    void LateUpdate()
    {
        if (segments == null || segments.Length < 2) return;
        BuildMesh();
    }

    void BuildMesh()
    {
        int segCount = segments.Length;
        int loopOffset = loop ? 1 : 0;
        int vertCount = segCount * 4;
        int triCount = (segCount - 1 + loopOffset) * 4 * 2;

        // Allocate arrays
        if (vertices == null || vertices.Length != vertCount)
        {
            vertices = new Vector3[vertCount];
            uvs = new Vector2[vertCount];
        }

        if (triangles == null || triangles.Length != triCount * 3)
            triangles = new int[triCount * 3];

        // Build vertices & UVs
        for (int i = 0; i < segCount; i++)
        {
            Vector3 pos = segments[i].position;

            Vector3 tangent;
            if (i < segCount - 1)
                tangent = (segments[(i + 1) % segCount].position - pos).normalized;
            else if (loop)
                tangent = (segments[0].position - pos).normalized;
            else
                tangent = (pos - segments[i - 1].position).normalized;

            Quaternion ringRot = Quaternion.LookRotation(tangent, Vector3.up);

            Vector3[] localCorners = new Vector3[]
            {
      new Vector3( radius,  0f, 0f),
      new Vector3( 0f,  radius, 0f),
      new Vector3(-radius,  0f, 0f),
      new Vector3( 0f, -radius, 0f)
            };

            Vector3 center = transform.InverseTransformPoint(pos);

            for (int j = 0; j < 4; j++)
            {
                int vi = i * 4 + j;
                Vector3 worldCorner = ringRot * localCorners[j];
                vertices[vi] = center + transform.InverseTransformDirection(worldCorner);
                uvs[vi] = new Vector2(j / 3f, i / (float)(segCount - 1 + (loop ? 1 : 0)));
            }
        }

        // Build triangles
        int ti = 0;
        for (int i = 0; i < segCount - 1 + loopOffset; i++)
        {
            int a = i * 4;
            int b = ((i + 1) % segCount) * 4;

            for (int j = 0; j < 4; j++)
            {
                int a0 = a + j;
                int a1 = a + (j + 1) % 4;
                int b0 = b + j;
                int b1 = b + (j + 1) % 4;

                triangles[ti++] = a0;
                triangles[ti++] = b0;
                triangles[ti++] = b1;

                triangles[ti++] = a0;
                triangles[ti++] = b1;
                triangles[ti++] = a1;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

}

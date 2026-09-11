using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphereMesh : MonoBehaviour
{
    [Range(1, 32)]
    [SerializeField] private int resolution = 16;
    [SerializeField] private float radius = 1f;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            GenerateCubeSphere();
        }
    }

    private void Awake()
    {
        GenerateCubeSphere();
    }

    public void GenerateCubeSphere()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CubeSphere";

        int vertCountPerFace = (resolution + 1) * (resolution + 1);
        Vector3[] vertices = new Vector3[vertCountPerFace * 6];
        Vector2[] uv = new Vector2[vertCountPerFace * 6];
        int[] triangles = new int[resolution * resolution * 6 * 6];

        int vIndex = 0;
        int tIndex = 0;

        Vector3[] faceDirections = {
            Vector3.forward, Vector3.back, 
            Vector3.left, Vector3.right, 
            Vector3.up, Vector3.down
        };

        for (int f = 0; f < 6; f++)
        {
            Vector3 localUp = faceDirections[f];
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    Vector2 percent = new Vector2((float)x / resolution, (float)y / resolution);
                    Vector3 pointOnCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;
                    
                    Vector3 pointOnSphere = pointOnCube.normalized * radius;

                    vertices[vIndex] = pointOnSphere;
                    uv[vIndex] = percent;

                    if (x != resolution && y != resolution)
                    {
                        int i = vIndex;
                        int i_nextX = i + 1;
                        int i_nextY = i + (resolution + 1);
                        int i_diag = i_nextY + 1;

                        triangles[tIndex + 0] = i;
                        triangles[tIndex + 1] = i_nextY;
                        triangles[tIndex + 2] = i_nextX;

                        triangles[tIndex + 3] = i_nextX;
                        triangles[tIndex + 4] = i_nextY;
                        triangles[tIndex + 5] = i_diag;

                        tIndex += 6;
                    }

                    vIndex++;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.sharedMesh = mesh;
        }
    }
}
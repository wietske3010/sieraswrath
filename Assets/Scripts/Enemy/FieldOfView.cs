using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{

    [SerializeField] private LayerMask layerMask; // Layer mask for raycasting
    [SerializeField] private Material undetectedMaterial;

    private Mesh mesh;
    private Vector3 origin;
    private float startingAngle;
    private float fov; // Field of view in degrees
    private float viewDistance; // Distance of the field of view


    private void Start()
    {

        // Create mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero; // Set origin to (0, 0, 0)
        this.GetComponent<MeshRenderer>().material = undetectedMaterial;
    }

    private void LateUpdate()
    {
        // Define variables
        int rayCount = 80; // Number of rays to cast

        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        // define arrays for vertices, uv and triangles
        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        // set origin vertex
        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex = origin + Quaternion.Euler(0, 0, angle) * Vector3.right * viewDistance;
            RaycastHit2D rayCastHit = Physics2D.Raycast(origin, vertex - origin, viewDistance, layerMask);

            // Check if raycast hits
            if (rayCastHit.collider != null)
            {
                vertex = rayCastHit.point;
            }

            vertices[vertexIndex] = vertex;


            if (i > 0)
            {
                triangles[triangleIndex] = 0; // Origin vertex
                triangles[triangleIndex + 1] = vertexIndex - 1; // Current vertex
                triangles[triangleIndex + 2] = vertexIndex; // first vertex
                triangleIndex += 3;
            }


            // increase clockwise
            angle -= angleIncrease;
            vertexIndex++;
        }



        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 500f); // Set bounds to a large value to prevent culling


    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetFOVDirection(Vector3 direction)
    {
        startingAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + fov / 2f;
    }

    public void SetFOV(float fov)
    {
        this.fov = fov;
    }

    public void SetViewDistance(float viewDistance)
    {
        this.viewDistance = viewDistance;
    }
}

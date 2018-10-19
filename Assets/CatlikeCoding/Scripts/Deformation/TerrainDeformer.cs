using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainDeformer : MonoBehaviour, IDeformer
{
    private Mesh mDeformingMesh;
    private Vector3[] mOriginalVertices;
    private Vector3[] mDisplacedVertices;
    private Vector3[] mVertexVelocities;

    public float SpringForce = 20f;
    public float Damping = 5f;
    public float UniformScale = 1f;

    public int Rows = 10;
    public int Columns = 10;

    // Debug Variables
    private Vector3 mPoint;
    // EO Debug Variables

    private void Start()
    {
        mDeformingMesh = GetComponent<MeshFilter>().mesh;
        mOriginalVertices = mDeformingMesh.vertices;
        mDisplacedVertices = new Vector3[mOriginalVertices.Length];
        for (int i = 0; i < mOriginalVertices.Length; i++)
            mDisplacedVertices[i] = mOriginalVertices[i];

        mVertexVelocities = new Vector3[mOriginalVertices.Length];

        var mesh = GetComponent<PlaneMesh>();
        Rows = mesh.Rows;
        Columns = mesh.Columns;
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        mPoint = point;

        Debug.DrawLine(Camera.main.transform.position, point);
        for (int i = 0; i < mDisplacedVertices.Length; i++)
            AddForceToVertex(i, point, force);
    }

    private void AddForceToVertex(int i, Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        var pointToVertex = mDisplacedVertices[i] - point;
        pointToVertex *= UniformScale;
        var attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        var velocity = attenuatedForce * Time.deltaTime;
        mVertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    private void Update()
    {
        UniformScale = transform.localScale.x;
        for (int i = 0; i < mDisplacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }

        for (int i = 0; i <= Rows; i++)
        {
            // Bottom Row
            mDisplacedVertices[i] = mOriginalVertices[i];

            // Top Row
            mDisplacedVertices[(Rows + 1) * Columns + i] = mOriginalVertices[(Rows + 1) * Columns + i];

            // Left Column
            mDisplacedVertices[Columns * i + i] = mOriginalVertices[Columns * i + i];

            // Right Column
            mDisplacedVertices[Columns * (i + 1) + i] = mOriginalVertices[Columns * (i + 1) + i];
        }

        mDeformingMesh.vertices = mDisplacedVertices;
        mDeformingMesh.RecalculateNormals();
    }

    private void UpdateVertex(int i)
    {
        var velocity = mVertexVelocities[i];
        var displacement = mDisplacedVertices[i] - mOriginalVertices[i];
        displacement *= UniformScale;
        velocity -= displacement * SpringForce * Time.deltaTime;
        velocity *= 1f - Damping * Time.deltaTime;
        mVertexVelocities[i] = velocity;
        mDisplacedVertices[i] += velocity * (Time.deltaTime / UniformScale);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(mPoint, 0.05f);

        for (int i = 0; i < mDisplacedVertices.Length; i++)
        {
            Ray ray = new Ray(mPoint, mDisplacedVertices[i] - mPoint);
            //Gizmos.DrawRay(ray);
            Handles.Label(mOriginalVertices[i], $"{i}");
        }
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawRay(mVertices[i], mNormals[i])
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer: MonoBehaviour, IDeformer
{
    private Mesh mDeformingMesh;
    private Vector3[] mOriginalVertices;
    private Vector3[] mDisplacedVertices;
    private Vector3[] mVertexVelocities;

    public float SpringForce = 20f;
    public float Damping = 5f;
    public float UniformScale = 1f;

    private void Start()
    {
        mDeformingMesh = GetComponent<MeshFilter>().mesh;
        mOriginalVertices = mDeformingMesh.vertices;
        mDisplacedVertices = new Vector3[mOriginalVertices.Length];
        for (int i = 0; i < mOriginalVertices.Length; i++)
            mDisplacedVertices[i] = mOriginalVertices[i];

        mVertexVelocities = new Vector3[mOriginalVertices.Length];
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        DebugLines.DrawLine(transform.position, point, Color.green);
        for (int i = 0; i < mDisplacedVertices.Length; i++)
        {
            //DebugLines.DrawLine(point, transform.TransformPoint(mDisplacedVertices[i]), Color.blue);
            AddForceToVertex(i, point, force);
        }
    }

    private void AddForceToVertex(int i, Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        var pointToVertex = mDisplacedVertices[i] - point;
        //if (pointToVertex.magnitude >= 0.5f)
        //    return;

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
}

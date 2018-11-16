using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MassSpringCloth2
    : MonoBehaviour
{
    [Header("Grid settings")]
    public int Rows = 60;
    public int Columns = 60;

    [Header("Cloth settings")]
    public float VertexMass = 20;
    public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    public float Damping = 0.025f;
    public int ConstraintIterations = 2;
    private Mesh mMesh;

    public bool mWindEnabled = false;

    [Header("Collisions - The colider of the sphere to collide with")]
    public SphereCollider mSphereCollider;

    private struct Spring
    {
        public int V1Idx;
        public int V2Idx;
        public float RestDistance; // The Original Distance
        public Color Color;

        public Spring(int v1Idx, int v2Idx, Vector3[] vertices, Color color)
        {
            Color = color;
            V1Idx = v1Idx;
            V2Idx = v2Idx;
            RestDistance = (vertices[v2Idx] - vertices[v1Idx]).magnitude;
        }
    };

    private Vector3[] mPositions;
    private Vector3[] mPrevPositions;
    private Vector3[] mAccelerations;

    private Vector3[] mOriginalPositions;

    private List<Spring> mSprings = new List<Spring>();

    private int RCToIdx(int row, int col)
    {
        return row * Columns + col;
    }

    private void IdxToRC(int idx, out int row, out int col)
    {
        row = idx / Columns;
        col = idx % Columns;
    }

    public void Start()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GenerateNonInclusivePlaneMesh(Rows, Columns);
        mMesh = filter.mesh;

        mPositions = new Vector3[Rows * Columns];
        mPrevPositions = new Vector3[mPositions.Length];
        mAccelerations = new Vector3[mPositions.Length];
        mOriginalPositions = new Vector3[mPositions.Length];

        Debug.AssertFormat(mPositions.Length == mMesh.vertexCount, "Vertex count is wrong");

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                mPositions[row * Columns + col] = new Vector3(col / (float)Columns, 0.0f, 1 - (row - 1) / (float)Rows);
                mPrevPositions[row * Columns + col] = mPositions[row * Columns + col];
                mAccelerations[row * Columns + col] = Vector3.zero;
                mOriginalPositions[row * Columns + col] = mPositions[row * Columns + col];

                // Strutcural Springs
                // Top
                if (row > 0)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col), RCToIdx(row, col), mPositions, Color.green));

                // Left
                if (col > 0)
                    mSprings.Add(new Spring(RCToIdx(row, col - 1), RCToIdx(row, col), mPositions, Color.green));

                // Shear Springs
                // Top Left
                if (row > 0 && col > 0)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col - 1), RCToIdx(row, col), mPositions, Color.yellow));

                // Top Right
                if (row > 0 && col < Columns - 1)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col + 1), RCToIdx(row, col), mPositions, Color.yellow));

                // Bending Springs
                // Top skip 1
                if (row > 1)
                    mSprings.Add(new Spring(RCToIdx(row - 2, col), RCToIdx(row, col), mPositions, Color.blue));

                if (col > 1)
                    mSprings.Add(new Spring(RCToIdx(row, col - 2), RCToIdx(row, col), mPositions, Color.blue));
            }
        }
    }

    public void FixedUpdate()
    {
        // Add Force
        for (int i = 0; i < mAccelerations.Length; i++)
        {
            

            // Wind
            if (mWindEnabled)
            {
                var pos = mPositions[i];
                mAccelerations[i] += new Vector3(
                                                    Mathf.Sin(Mathf.Cos(5 * pos.x * pos.y * pos.z)),
                                                    -Mathf.Cos(pos.z * Time.fixedDeltaTime),
                                                    Mathf.Sin(pos.x * pos.y * Time.fixedDeltaTime)
                                                );
            }
            else
            {
                // Currently the wind is not strong enough to go against the gravity,
                // so for demo purposes this hack is on.
                mAccelerations[i] += (Gravity / VertexMass);
            }
        }

        // Move particles
        for (int i = 0; i < mPositions.Length; i++)
        {
            var curr = mPositions[i];
            var prev = mPrevPositions[i];

            curr = curr + (curr - prev) * (1.0f - Damping) + mAccelerations[i] * Time.fixedDeltaTime * Time.fixedDeltaTime;

            mPrevPositions[i] = mPositions[i];
            mPositions[i] = curr;

            mAccelerations[i] = Vector3.zero;
        }


        // Fix Constraints
        for (int step = 0; step < ConstraintIterations; step++)
        {
            for (int i = 0; i < mSprings.Count; i++)
            {
                var spring = mSprings[i];
                var diff = mPositions[spring.V2Idx] - mPositions[spring.V1Idx];
                var dist = diff.magnitude;
                var correction = (diff * (1 - spring.RestDistance / dist)) * 0.25f;

                mPositions[spring.V1Idx] += correction;
                mPositions[spring.V2Idx] -= correction;
            }
        }

        CollideWithSphere(mSphereCollider);





        var steps = Columns / 5;
        for (int i = mPositions.Length - 1; i >= mPositions.Length - steps; i--)
        {
            mPositions[i] = mOriginalPositions[i];
            //mPositions[i - steps * 2] = mOriginalPositions[i - steps * 2];
            mPositions[i - steps * 4] = mOriginalPositions[i - steps * 4];
        }

        //mPositions[(Rows / 2) * Columns + (Columns / 2)] = mOriginalPositions[(Rows / 2) * Columns + (Columns / 2)];

        //var steps = Columns / 5;
        //for (int i = 0; i < steps; i++)
        //{
        //    mPositions[i] = mOriginalPositions[i];
        //    mPositions[i + steps * 2] = mOriginalPositions[i + steps * 2];
        //    mPositions[i + steps * 4] = mOriginalPositions[i + steps * 4];
        //}

        // Lock top corners.
        // Top Left
        //mVertices[0 * Columns + 0] = mInitialVertices[0 * Columns + 0];
        //mVertices[0 * Columns + Columns - 1] = mInitialVertices[0 * Columns + Columns - 1];

        //mVertices[0 * Columns + (Columns / 4)] = mInitialVertices[0 * Columns + (Columns / 4)];


        //// Top Middle
        //mVertices[0 * Columns + (Columns / 2)] = mInitialVertices[0 * Columns + (Columns / 2)];

        //mVertices[0 * Columns + ((Columns / 4) * 3)] = mInitialVertices[0 * Columns + ((Columns / 4) * 3)];

        //// Top Right

        // Top Left
        //mVertices[Rows * Columns - 1] = mInitialVertices[Rows * Columns - 1];
        // Top Right
        //mVertices[Rows * Columns - Columns] = mInitialVertices[Rows * Columns - Columns];

        mMesh.vertices = mPositions;
        mMesh.RecalculateNormals();

    }

    private void CollideWithSphere(SphereCollider collider)
    {
        var localCollider = transform.InverseTransformPoint(collider.transform.position);
        var scaledRadius = collider.transform.localScale.x * collider.radius;
        for (int i = 0; i < mPositions.Length; i++)
        {
            var dist = mPositions[i] - localCollider;
            var length = dist.magnitude;
            if (length < scaledRadius)
            {
                mPositions[i] += dist.normalized * (scaledRadius - length);
            }
        }
    }


    public void OnDrawGizmos()
    {
        if (mPositions == null)
            return;

        //// Draw Vertices
        //Gizmos.color = Color.black;
        //for (int i = 0; i < mPositions.Length; i++)
        //{
        //    var vertexWorld = transform.TransformPoint(mPositions[i]);
        //    Gizmos.color = Color.black;
        //    Gizmos.DrawSphere(vertexWorld, 0.01f);
        //    //Gizmos.color = Color.yellow;
        //    //Handles.Label(vertexWorld, $"{i}");
        //}

        //// Draw all the springs
        //Gizmos.color = Color.white;

        //for (int i = 0; i < mSprings.Count; i++)
        //{
        //    var spring = mSprings[i];
        //    Gizmos.color = spring.Color;
        //    var v1World = transform.TransformPoint(mPositions[spring.V1Idx]);
        //    var v2World = transform.TransformPoint(mPositions[spring.V2Idx]);
        //    //if (spring.Color == Color.blue)
        //    //{
        //        Gizmos.DrawLine(v1World, v2World);
        //    //}
        //}

        // Essentially just drawing the mesh here with gizmos, just so I don't have to deal with the mesh itself.
        //for (int row = 0; row < Rows; row++)
        //{
        //    for (int col = 0; col < Columns; col++)
        //    {
        //        var v = transform.TransformPoint(mPositions[row * Columns + col]);
        //        // Structural Bindings
        //        // Top
        //        //if (row > 0)
        //        //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col]));
        //        //// Left
        //        //if (col > 0)
        //        //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[row * Columns + col - 1]));


        //        // Top Left
        //        //if (row > 0 && col > 0)
        //        //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col - 1]));


        //        // Top Right
        //        //if (row > 0 && col < Columns - 1)
        //        //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col + 1]));

        //    }
        //}


    }

}

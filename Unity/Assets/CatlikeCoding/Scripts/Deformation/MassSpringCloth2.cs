using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MassSpringCloth2 : MonoBehaviour
{
    [Header("Grid settings")]
    public int Rows = 60;
    public int Columns = 60;

    [Header("Cloth settings")]
    public float VertexMass = 20;
    public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    //public float RestDistance = 1;
    public float Damping = 0.025f;
    public int ConstraintIterations = 2;

    Mesh mMesh;

    private struct Spring
    {
        public int V1Idx;
        public int V2Idx;
        public float RestDistance; // The Original Distance

        public Spring(int v1Idx, int v2Idx, Vertex[] vertices)
        {
            V1Idx = v1Idx;
            V2Idx = v2Idx;
            RestDistance = (vertices[v2Idx].Position - vertices[v1Idx].Position).magnitude;
        }
    };

    private struct Vertex
    {
        public int Idx;
        public Vector3 Position;
        public Vector3 PrevPosition;
        public Vector3 Acceleration;
    }

    private Vertex[] mInitialVertices;

    private Vertex[] mVertices;

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
        //var filter = GetComponent<MeshFilter>();
        //filter.mesh = Procedural.GeneratePlaneMesh(Rows, Columns);
        //mMesh = filter.mesh;
        

        mVertices = new Vertex[Rows * Columns];
        mInitialVertices = new Vertex[mVertices.Length];
        

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                mVertices[row * Columns + col].Position = new Vector3(col / (float)Columns , 0.0f, 1 - (row - 1) / (float)Rows);
                mVertices[row * Columns + col].Idx = RCToIdx(row, col);
                mVertices[row * Columns + col].PrevPosition = mVertices[row * Columns + col].Position;
                mVertices[row * Columns + col].Acceleration = Vector3.zero;




                mInitialVertices[row * Columns + col] = mVertices[row * Columns + col];



                // Top
                if (row > 0)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col), RCToIdx(row, col), mVertices));

                // Left
                if (col > 0)
                    mSprings.Add(new Spring(RCToIdx(row, col - 1), RCToIdx(row, col), mVertices));

                // Top Left
                if (row > 0 && col > 0)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col - 1), RCToIdx(row, col), mVertices));

                //// Top Right
                if (row > 0 && col < Columns - 1)
                    mSprings.Add(new Spring(RCToIdx(row - 1, col + 1), RCToIdx(row, col), mVertices));
            }
        }
    }

    public void FixedUpdate()
    {
        // Add Force
        for (int i = 0; i < mVertices.Length; i++)
        {
            mVertices[i].Acceleration += (Gravity / VertexMass);
        }


        // Fix Constraints
        for (int step = 0; step < ConstraintIterations; step++)
        {
            for (int i = 0; i < mSprings.Count; i++)
            {
                var spring = mSprings[i];
                var diff = mVertices[spring.V2Idx].Position - mVertices[spring.V1Idx].Position;
                var dist = diff.magnitude;
                var correction = (diff * (1 - spring.RestDistance / dist)) * 0.25f;

                mVertices[spring.V1Idx].Position += correction;
                mVertices[spring.V2Idx].Position -= correction;
            }

        }

        // Move particles
        for (int i = 0; i < mVertices.Length; i++)
        {
            var curr = mVertices[i].Position;
            var prev = mVertices[i].PrevPosition;

            //curr = curr + (curr - prev) * (1.0f - Damping) + mVertices[i].Acceleration * Time.fixedDeltaTime * 0.1f;
            curr = curr + (curr - prev) * (1.0f - Damping) + mVertices[i].Acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;

            mVertices[i].PrevPosition = mVertices[i].Position;
            mVertices[i].Position = curr;

            mVertices[i].Acceleration = Vector3.zero;

        }
        


        var steps = Columns / 5;
        for (int i = 0; i < steps; i++)
        {
            mVertices[i] = mInitialVertices[i];
            mVertices[i + steps * 2] = mInitialVertices[i + steps * 2];
            mVertices[i + steps * 4] = mInitialVertices[i + steps * 4];


            //mVertices[i + (Columns / 4 * 3)] = mInitialVertices[i + (Columns / 4 * 3)];


        }

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
    }

    public void OnDrawGizmos()
    {
        if (mVertices == null)
            return;

        // Draw Vertices
        //Gizmos.color = Color.black;
        //for (int i = 0; i < mVertices.Length; i++)
        //{
        //    var vertexWorld = transform.TransformPoint(mVertices[i].Position);
        //    Gizmos.color = Color.black;
        //    Gizmos.DrawSphere(vertexWorld, 0.01f);
        //    Gizmos.color = Color.yellow;
        //    Handles.Label(vertexWorld, $"{i}");
        //}

        // Draw all the springs
        for (int i = 0; i < mSprings.Count; i++)
        {
            var spring = mSprings[i];
            var v1World = transform.TransformPoint(mVertices[spring.V1Idx].Position);
            var v2World = transform.TransformPoint(mVertices[spring.V2Idx].Position);
            Gizmos.DrawLine(v1World, v2World);
        }

        // Essentially just drawing the mesh here with gizmos, just so I don't have to deal with the mesh itself.
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var v = transform.TransformPoint(mVertices[row * Columns + col].Position);
                // Structural Bindings
                // Top
                //if (row > 0)
                //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col]));
                //// Left
                //if (col > 0)
                //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[row * Columns + col - 1]));


                // Top Left
                //if (row > 0 && col > 0)
                //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col - 1]));


                // Top Right
                //if (row > 0 && col < Columns - 1)
                //    Gizmos.DrawLine(v, transform.TransformPoint(mVertices[(row - 1) * Columns + col + 1]));

            }
        }


    }

}

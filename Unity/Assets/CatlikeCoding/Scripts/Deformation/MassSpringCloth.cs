using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

// Broken attempt, try to redo!
public class MassSpringCloth : MonoBehaviour
{
    [Header("Grid settings")]
    public int Rows = 1;
    public int Columns = 1;

    [Header("Cloth settings")]
    public float VertexMass = 1;
    public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    public float SpringConstant = 1;
    public float SpringDamping = 1;

    private struct Spring
    {
        public int V1Idx;
        public int V2Idx;
        public float RestDistance; // The Original Distance
    };

    private struct Vertex
    {
        public int Idx;
        public Vector3 TotalForce;
        public Vector3 Position;
        public Vector3 Velocity;
    }

    //private Vector3[] mVertices;
    private Vertex[] mInitialVertices;

    private Vertex[] mCurrentVertices;
    private Vertex[] mTargetVertices;

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
        mCurrentVertices = new Vertex[Rows * Columns];
        mInitialVertices = new Vertex[mCurrentVertices.Length];
        mTargetVertices = new Vertex[mCurrentVertices.Length];

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                mCurrentVertices[row * Columns + col].Position = new Vector3(col / (float)Columns, 0.0f, 1 - ((row + 1) / (float)Rows));
                mCurrentVertices[row * Columns + col].Idx = RCToIdx(row, col);

                mInitialVertices[row * Columns + col] = mCurrentVertices[row * Columns + col];
                mTargetVertices[row * Columns + col] = mCurrentVertices[row * Columns + col];


                // Top
                if (row > 0)
                    mSprings.Add(new Spring()
                    {
                        V1Idx = RCToIdx(row - 1, col),
                        V2Idx = RCToIdx(row, col),
                        RestDistance = 1.0f,
                    });

                // Left
                if (col > 0)
                    mSprings.Add(new Spring()
                    {
                        V1Idx = RCToIdx(row, col - 1),
                        V2Idx = RCToIdx(row, col),
                        RestDistance = 1.0f,
                    });

                // Top Left
                if (row > 0 && col > 0)
                    mSprings.Add(new Spring()
                    {
                        V1Idx = RCToIdx(row - 1, col - 1),
                        V2Idx = RCToIdx(row, col),
                        RestDistance = 1.0f,
                    });

                // Top Right
                if (row > 0 && col < Columns - 1)
                    mSprings.Add(new Spring()
                    {
                        V1Idx = RCToIdx(row - 1, col + 1),
                        V2Idx = RCToIdx(row, col),
                        RestDistance = 1.0f,
                    });
            }
        }
    }

    public void FixedUpdate()
    {
        Vector3 wind = Vector3.zero;
        Vector3 airResistance = Vector3.zero;
        Vector3 massVec = new Vector3(VertexMass, VertexMass, VertexMass);

        #region BrokenAttempt
        ////Calculate force for each vertex
        //for (int i = 0; i < mCurrentVertices.Length; i++)
        //    {
        //        mCurrentVertices[i].TotalForce.x = Gravity.x / (1 / VertexMass);
        //        mCurrentVertices[i].TotalForce.y = Gravity.y / (1 / VertexMass);
        //        mCurrentVertices[i].TotalForce.z = Gravity.z / (1 / VertexMass);

        //        mCurrentVertices[i].TotalForce.x += (-SpringDamping * mCurrentVertices[i].Velocity.x);
        //        mCurrentVertices[i].TotalForce.y += (-SpringDamping * mCurrentVertices[i].Velocity.y);
        //        mCurrentVertices[i].TotalForce.z += (-SpringDamping * mCurrentVertices[i].Velocity.z);

        //    }

        //// Update forces based on Springs
        //for (int i = 0; i < mSprings.Count; i++)
        //{
        //    var spring = mSprings[i];
        //    var deltaPosition = mCurrentVertices[spring.V1Idx].Position - mCurrentVertices[spring.V2Idx].Position;

        //    var distance = (deltaPosition).magnitude;
        //    var hTerm = (distance - spring.RestDistance) * SpringConstant;

        //    // Doing without damping first
        //    var deltaVelocity = mCurrentVertices[spring.V1Idx].Velocity - mCurrentVertices[spring.V2Idx].Velocity;
        //    var dTerm = (Vector3.Dot(deltaPosition, deltaVelocity) * SpringDamping) / distance;

        //    var springForce = deltaPosition.normalized;
        //    springForce.x *= -(hTerm + dTerm);
        //    springForce.y *= -(hTerm + dTerm);
        //    springForce.z *= -(hTerm + dTerm);


        //    mCurrentVertices[spring.V1Idx].TotalForce += springForce;
        //    mCurrentVertices[spring.V2Idx].TotalForce += springForce;

        //}


        //// Update positions for all vertices based on force

        //var deltaMass = Time.fixedDeltaTime * (1 / VertexMass);
        //for (int i = 0; i < mCurrentVertices.Length; i++)
        //{
        //    mTargetVertices[i].Velocity.x = mCurrentVertices[i].Velocity.x + (mCurrentVertices[i].TotalForce.x * deltaMass);
        //    mTargetVertices[i].Velocity.y = mCurrentVertices[i].Velocity.y + (mCurrentVertices[i].TotalForce.y * deltaMass);
        //    mTargetVertices[i].Velocity.z = mCurrentVertices[i].Velocity.z + (mCurrentVertices[i].TotalForce.z * deltaMass);

        //    mTargetVertices[i].Position.x = mCurrentVertices[i].Position.x + (mCurrentVertices[i].Velocity.x * Time.fixedDeltaTime);
        //    mTargetVertices[i].Position.y = mCurrentVertices[i].Position.y + (mCurrentVertices[i].Velocity.y * Time.fixedDeltaTime);
        //    mTargetVertices[i].Position.z = mCurrentVertices[i].Position.z + (mCurrentVertices[i].Velocity.z * Time.fixedDeltaTime);
        //}
        #endregion

        #region NewAttempt

        //var deltaMass = Time.deltaTime * (1 / VertexMass);
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var force = CalculateSpringSum(mInitialVertices, mCurrentVertices, row, col, SpringConstant);
                mCurrentVertices[RCToIdx(row, col)].TotalForce.x = Gravity.x; // - force.x;
                mCurrentVertices[RCToIdx(row, col)].TotalForce.y = Gravity.y; // - force.y;
                mCurrentVertices[RCToIdx(row, col)].TotalForce.z = Gravity.z; // - force.z;
            }
        }


        for (int i = 0; i < mCurrentVertices.Length; i++)
        {
            int row = 0;
            int col = 0;
            IdxToRC(i, out row, out col);
            var force = CalculateSpringSum(mInitialVertices, mCurrentVertices, row, col, SpringConstant);

            mTargetVertices[i].Velocity.x = (Gravity.x * Time.fixedDeltaTime + force.x * Time.fixedDeltaTime);
            mTargetVertices[i].Velocity.y = (Gravity.y * Time.fixedDeltaTime + force.y * Time.fixedDeltaTime);
            mTargetVertices[i].Velocity.z = (Gravity.z * Time.fixedDeltaTime + force.z * Time.fixedDeltaTime);

            var vel = mTargetVertices[i].Velocity;
            var displacement = mCurrentVertices[i].Position - mInitialVertices[i].Position;
            vel -= displacement * force.x * Time.deltaTime;
            vel *= 1f - SpringDamping * Time.deltaTime;

            mTargetVertices[i].Velocity = vel;

            mTargetVertices[i].Position.x = mCurrentVertices[i].Position.x + (mTargetVertices[i].Velocity.x * Time.fixedDeltaTime);
            mTargetVertices[i].Position.y = mCurrentVertices[i].Position.y + (mTargetVertices[i].Velocity.y * Time.fixedDeltaTime);
            mTargetVertices[i].Position.z = mCurrentVertices[i].Position.z + (mTargetVertices[i].Velocity.z * Time.fixedDeltaTime);
        }

        #endregion
        // Lock top corners.
        // Top Left
        mCurrentVertices[0 * Columns + 0] = mInitialVertices[0 * Columns + 0];
        // Top Right
        mCurrentVertices[0 * Columns + Columns - 1] = mInitialVertices[0 * Columns + Columns - 1];

        // Top Left
        mCurrentVertices[Rows * Columns - 1] = mInitialVertices[Rows * Columns - 1];
        // Top Right
        mCurrentVertices[Rows * Columns - Columns] = mInitialVertices[Rows * Columns - Columns];

        // Swap the two.
        var tmp = mTargetVertices;
        mTargetVertices = mCurrentVertices;
        mCurrentVertices = mTargetVertices;

    }

    private Vector3 CalculateSpringSum(Vertex[] originalVertices, Vertex[] currentVertices, int row, int col, float constant)
    {
        Vector3 force = Vector3.one;

        
        // Top Left
        if (row > 0 && col > 0)
        {
            var rest = originalVertices[RCToIdx(row - 1, col - 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row - 1, col - 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Top
        if (row > 0)
        {
            var rest = originalVertices[RCToIdx(row - 1, col)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row - 1, col)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Top Right
        if (row > 0 && col < Columns - 1)
        {
            var rest = originalVertices[RCToIdx(row - 1, col + 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row - 1, col + 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Left
        if (col > 0)
        {
            var rest = originalVertices[RCToIdx(row, col - 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row, col - 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Right
        if (col < Columns - 1)
        {
            var rest = originalVertices[RCToIdx(row, col + 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row, col + 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Bottom Left
        if (row < Rows - 1 && col > 0)
        {
            var rest = originalVertices[RCToIdx(row + 1, col - 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row + 1, col - 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Bottom
        if (row < Rows - 1)
        {
            var rest = originalVertices[RCToIdx(row + 1, col)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row + 1, col)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        // Bottom Right
        if (row < Rows - 1 && col < Columns - 1)
        {
            var rest = originalVertices[RCToIdx(row + 1, col + 1)].Position - originalVertices[RCToIdx(row, col)].Position;
            var curr = currentVertices[RCToIdx(row + 1, col + 1)].Position - currentVertices[RCToIdx(row, col)].Position;
            var restlen = rest.magnitude;

            var f = (curr.magnitude - restlen) * constant;
            force += curr * f;
        }

        return force;
        //return new Vector3(sum, sum, sum);
    }

    public void OnDrawGizmos()
    {
        if (mCurrentVertices == null)
            return;

        // Draw Vertices
        Gizmos.color = Color.black;
        for (int i = 0; i < mCurrentVertices.Length; i++)
        {
            var vertexWorld = transform.TransformPoint(mCurrentVertices[i].Position);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertexWorld, 0.01f);
            Gizmos.color = Color.yellow;
            Handles.Label(vertexWorld, $"{i}");
        }

        // Draw all the springs
        for (int i = 0; i < mSprings.Count; i++)
        {
            var spring = mSprings[i];
            var v1World = transform.TransformPoint(mCurrentVertices[spring.V1Idx].Position);
            var v2World = transform.TransformPoint(mCurrentVertices[spring.V2Idx].Position);
            Gizmos.DrawLine(v1World, v2World);
        }

        // Essentially just drawing the mesh here with gizmos, just so I don't have to deal with the mesh itself.
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var v = transform.TransformPoint(mCurrentVertices[row * Columns + col].Position);
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

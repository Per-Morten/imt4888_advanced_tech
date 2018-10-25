using System.Collections;
using UnityEditor;
using UnityEngine;

// Class showing the creation of the cube-mesh animated.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnimatedCubeSphere
    : MonoBehaviour
{
    public int GridSize;

    public float Radius = 1;

    public float WaitTime = 0.05f;
    private WaitForSeconds mWait;

    private Mesh mMesh;
    private Vector3[] mVertices;
    private Vector3[] mNormals;
    private Color32[] mCubeUV;

    ///
    /// For animation
    ///
    private enum State
    {
        None,
        Perimiter,
        TopFirstRow,
        TopMiddleRows,
        TopLastRow,
        BottomFirstRow,
        BottomMiddleRows,
        BottomLastRow,
    }

    // Super hacky way of getting t out of the create top, and bottom functions when using co routines.
    private int mTriangleIdx = 0;
    private State mState = State.None;

    private int idx1;
    private int idx2;
    private int idx3;

    private void Awake()
    {
        mWait = new WaitForSeconds(WaitTime);
        StartCoroutine(Generate());
    }

    [ContextMenu("Generate")]
    public void StartGeneration()
    {
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        mMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mMesh;

        yield return CreateVertices();
        yield return CreateTriangles();
        gameObject.AddComponent<SphereCollider>();
    }

    private IEnumerator CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (GridSize + GridSize + GridSize - 3) * 4;
        int faceVertices = (
            (GridSize - 1) * (GridSize - 1) +
            (GridSize - 1) * (GridSize - 1) +
            (GridSize - 1) * (GridSize - 1)) * 2;

        mVertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        mNormals = new Vector3[mVertices.Length];
        mCubeUV = new Color32[mVertices.Length];

        // Create Perimeter of the cube
        int v = 0; // Vertex
        for (int y = 0; y <= GridSize; y++)
        {
            for (int x = 0; x <= GridSize; x++)
            {
                SetVertex(v++, x, y, 0);
                yield return mWait;
            }

            for (int z = 1; z <= GridSize; z++)
            {
                SetVertex(v++, GridSize, y, z);
                yield return mWait;
            }

            for (int x = GridSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, GridSize);
                yield return mWait;
            }

            for (int z = GridSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
                yield return mWait;
            }
        }

        // Add top and bottom faces.
        for (int z = 1; z < GridSize; z++)
        {
            for (int x = 1; x < GridSize; x++)
            {
                SetVertex(v++, x, GridSize, z);
                yield return mWait;
            }
        }

        for (int z = 1; z < GridSize; z++)
        {
            for (int x = 1; x < GridSize; x++)
            {
                SetVertex(v++, x, 0, z);
                yield return mWait;
            }
        }

        mMesh.vertices = mVertices;
        mMesh.normals = mNormals;
        mMesh.colors32 = mCubeUV;
        //return null;
        yield return mWait;
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        var v = new Vector3(x, y, z) * 2f / GridSize - Vector3.one;

        // Re-read math on this!
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        mNormals[i] = s;
        mVertices[i] = mNormals[i] * Radius;
        mCubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    private IEnumerator CreateTriangles()
    {
        mState = State.Perimiter;
        int quads = (GridSize * GridSize + GridSize * GridSize + GridSize * GridSize) * 2;
        int[] triangles = new int[quads * 6];

        int ring = (GridSize + GridSize) * 2;
        int t = 0; // Triangle
        int v = 0; // Vertex

        for (int y = 0; y < GridSize; y++, v++)
        {
            // q = quad
            // Need to reuse the second and fourth vertex in each ring to wrap around properly,
            // otherwise the quad will go diagonally up.
            for (int q = 0; q < ring - 1; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
                idx1 = v;
                yield return UpdateTriangles(triangles);
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
            idx1 = v;
            yield return UpdateTriangles(triangles);
        }

        //t = CreateTopFace(triangles, t, ring);
        yield return CreateTopFace(triangles, t, ring);
        t = mTriangleIdx;
        yield return CreateBottomFace(triangles, t, ring);

        yield return mWait;
    }

    private IEnumerator CreateTopFace(int[] triangles, int t, int ring)
    {
        mState = State.TopFirstRow;

        // Create first row
        int v = ring * GridSize;
        for (int x = 0; x < GridSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            idx1 = v;
            yield return UpdateTriangles(triangles);
        }
        // v + 2
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        idx1 = v;
        yield return UpdateTriangles(triangles);

        int vMin = ring * (GridSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        mState = State.TopMiddleRows;
        for (int z = 1; z < GridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            // Middle rows
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + GridSize - 1);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
            for (int x = 1; x < GridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + GridSize - 1, vMid + GridSize);
                idx1 = vMid; idx2 = vMin; idx3 = vMax;
                yield return UpdateTriangles(triangles);

            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + GridSize - 1, vMax + 1);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);

        }

        mState = State.TopLastRow;
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        idx1 = vMid; idx2 = vMin; idx3 = vTop;
        yield return UpdateTriangles(triangles);

        for (int x = 1; x < GridSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            idx1 = vMid; idx2 = vMin; idx3 = vTop;
            yield return UpdateTriangles(triangles);

        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        idx1 = vMid; idx2 = vMin; idx3 = vTop;
        yield return UpdateTriangles(triangles);

        mTriangleIdx = t;
    }

    private IEnumerator CreateBottomFace(int[] triangles, int t, int ring)
    {
        // First Row
        mState = State.BottomFirstRow;
        int v = 1;
        int vMid = mVertices.Length - (GridSize - 1) * (GridSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        idx1 = vMid; idx2 = v;
        yield return UpdateTriangles(triangles);

        for (int x = 1; x < GridSize - 1; x++, vMid++, v++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            idx1 = vMid; idx2 = v;
            yield return UpdateTriangles(triangles);

        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);
        idx1 = vMid; idx2 = v;
        yield return UpdateTriangles(triangles);

        // Middle Rows
        // First index on "left row" when viewed from below and have index 0 in top left corner.
        mState = State.BottomMiddleRows;
        int vMin = ring - 2;
        vMid -= GridSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < GridSize - 1; z++, vMin--, vMax++, vMid++)
        {
            t = SetQuad(triangles, t, vMin, vMid + GridSize - 1, vMin + 1, vMid);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
            for (int x = 1; x < GridSize - 1; x++, vMid++, v++)
            {
                t = SetQuad(triangles, t, vMid + GridSize - 1, vMid + GridSize, vMid, vMid + 1);
                idx1 = vMid; idx2 = vMin; idx3 = vMax;
                yield return UpdateTriangles(triangles);
            }
            t = SetQuad(triangles, t, vMid + GridSize - 1, vMax + 1, vMid, vMax);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
        }

        // Last Row
        mState = State.BottomMiddleRows;
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        idx1 = vMid; idx2 = vTop;
        yield return UpdateTriangles(triangles);
        for (int x = 1; x < GridSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            idx1 = vMid; idx2 = vTop;
            yield return UpdateTriangles(triangles);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
        idx1 = vMid; idx2 = vTop;
        yield return UpdateTriangles(triangles);

        mTriangleIdx = t;

        mState = State.None;
    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = v01;
        triangles[i + 2] = v10;
        triangles[i + 3] = v10;
        triangles[i + 4] = v01;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private IEnumerator UpdateTriangles(int[] triangles)
    {
        mMesh.triangles = triangles;
        yield return mWait;
    }

    private void OnDrawGizmos()
    {
        if (mVertices == null)
            return;

        Gizmos.color = Color.black;
        for (int i = 0; i < mVertices.Length; i++)
        {
            var vertexWorldSpace = transform.localToWorldMatrix * mVertices[i];
            Gizmos.color = Color.black;
            //Gizmos.DrawSphere(vertexWorldSpace, 0.05f);
            Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(mVertices[i], mNormals[i]);
        }

        if (mState == State.None)
            return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        if (mState == State.Perimiter)
        {
            Handles.Label(mVertices[idx1], "v", style);
        }
        if (mState == State.TopFirstRow)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
        }
        if (mState == State.TopMiddleRows)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
            Handles.Label(mVertices[idx2], "vMin", style);
            Handles.Label(mVertices[idx3], "vMax", style);
        }
        if (mState == State.TopLastRow)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
            Handles.Label(mVertices[idx2], "vMin", style);
            Handles.Label(mVertices[idx3], "vTop", style);
        }
        if (mState == State.BottomFirstRow)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
        }
        if (mState == State.BottomMiddleRows)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
            Handles.Label(mVertices[idx2], "vMin", style);
            Handles.Label(mVertices[idx3], "vMax", style);
        }
        if (mState == State.BottomLastRow)
        {
            Handles.Label(mVertices[idx1], "vMid", style);
            Handles.Label(mVertices[idx2], "vMin", style);
            Handles.Label(mVertices[idx3], "vTop", style);
        }

    }
}

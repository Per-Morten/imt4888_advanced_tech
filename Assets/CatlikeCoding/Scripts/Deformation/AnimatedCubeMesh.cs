using System.Collections;
using UnityEditor;
using UnityEngine;

// Class showing the creation of the cube-mesh animated.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnimatedCubeMesh
    : MonoBehaviour
{
    public int XSize;
    public int YSize;
    public int ZSize;

    public float WaitTime = 0.05f;
    private WaitForSeconds mWait;

    private Mesh mMesh;
    private Vector3[] mVertices;

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

    private IEnumerator Generate()
    {
        mMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mMesh;

        yield return CreateVertices();
        yield return CreateTriangles();
    }

    private IEnumerator CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (XSize + YSize + ZSize - 3) * 4;
        int faceVertices = (
            (XSize - 1) * (YSize - 1) +
            (XSize - 1) * (ZSize - 1) +
            (YSize - 1) * (ZSize - 1)) * 2;

        mVertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        // Create Perimeter of the cube
        int v = 0; // Vertex
        for (int y = 0; y <= YSize; y++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                mVertices[v++] = new Vector3(x, y, 0);
                //yield return mWait;
            }

            for (int z = 1; z <= ZSize; z++)
            {
                mVertices[v++] = new Vector3(XSize, y, z);
                //yield return mWait;
            }

            for (int x = XSize - 1; x >= 0; x--)
            {
                mVertices[v++] = new Vector3(x, y, ZSize);
                //yield return mWait;
            }

            for (int z = ZSize - 1; z > 0; z--)
            {
                mVertices[v++] = new Vector3(0, y, z);
                //yield return mWait;
            }
        }

        // Add top and bottom faces.
        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, YSize, z);
                //yield return mWait;
            }
        }

        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, 0, z);
                //yield return mWait;
            }
        }

        mMesh.vertices = mVertices;
        return null;
        //yield return mWait;
    }

    private IEnumerator CreateTriangles()
    {
        mState = State.Perimiter;
        int quads = (XSize * YSize + XSize * ZSize + YSize * ZSize) * 2;
        int[] triangles = new int[quads * 6];

        int ring = (XSize + ZSize) * 2;
        int t = 0; // Triangle
        int v = 0; // Vertex

        for (int y = 0; y < YSize; y++, v++)
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
        Debug.Log($"Ring size: {ring}");
        // Create first row
        int v = ring * YSize;
        for (int x = 0; x < XSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            idx1 = v;
            yield return UpdateTriangles(triangles);
        }
        // v + 2 
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        idx1 = v;
        yield return UpdateTriangles(triangles);

        int vMin = ring * (YSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        mState = State.TopMiddleRows;
        for (int z = 1; z < ZSize - 1; z++, vMin--, vMid++, vMax++)
        {
            // Middle rows
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + XSize - 1);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
            for (int x = 1; x < XSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + XSize - 1, vMid + XSize);
                idx1 = vMid; idx2 = vMin; idx3 = vMax;
                yield return UpdateTriangles(triangles);

            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + XSize - 1, vMax + 1);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);

        }

        mState = State.TopLastRow;
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        idx1 = vMid; idx2 = vMin; idx3 = vTop;
        yield return UpdateTriangles(triangles);

        for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
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
        int vMid = mVertices.Length - (XSize - 1) * (ZSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        idx1 = vMid; idx2 = v;
        yield return UpdateTriangles(triangles);

        for (int x = 1; x < XSize - 1; x++, vMid++, v++)
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
        vMid -= XSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < ZSize - 1; z++, vMin--, vMax++, vMid++)
        {
            t = SetQuad(triangles, t, vMin, vMid + XSize - 1, vMin + 1, vMid);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
            for (int x = 1; x < XSize - 1; x++, vMid++, v++)
            {
                t = SetQuad(triangles, t, vMid + XSize - 1, vMid + XSize, vMid, vMid + 1);
                idx1 = vMid; idx2 = vMin; idx3 = vMax;
                yield return UpdateTriangles(triangles);
            }
            t = SetQuad(triangles, t, vMid + XSize - 1, vMax + 1, vMid, vMax);
            idx1 = vMid; idx2 = vMin; idx3 = vMax;
            yield return UpdateTriangles(triangles);
        }

        // Last Row
        mState = State.BottomMiddleRows;
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        idx1 = vMid; idx2 = vTop;
        yield return UpdateTriangles(triangles);
        for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            idx1 = vMid; idx2 = vTop;
            yield return UpdateTriangles(triangles);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
        idx1 = vMid; idx2 = vTop;
        yield return UpdateTriangles(triangles);

        mTriangleIdx = t;
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
            Gizmos.DrawSphere(vertexWorldSpace, 0.05f);
            //Handles.Label(vertexWorldSpace, $"index: {i}");
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

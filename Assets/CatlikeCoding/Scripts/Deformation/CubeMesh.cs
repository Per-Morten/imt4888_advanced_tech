using System.Collections;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMesh
    : MonoBehaviour
{
    public int XSize;
    public int YSize;
    public int ZSize;

    public float WaitTime = 0.05f;
    private WaitForSeconds mWait;

    private Mesh mMesh;
    private Vector3[] mVertices;



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
                yield return mWait;
            }

            for (int z = 1; z <= ZSize; z++)
            {
                mVertices[v++] = new Vector3(XSize, y, z);
                yield return mWait;
            }

            for (int x = XSize - 1; x >= 0; x--)
            {
                mVertices[v++] = new Vector3(x, y, ZSize);
                yield return mWait;
            }

            for (int z = ZSize - 1; z > 0; z--)
            {
                mVertices[v++] = new Vector3(0, y, z);
                yield return mWait;
            }
        }

        // Add top and bottom faces.
        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, YSize, z);
                yield return mWait;
            }
        }

        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, 0, z);
                yield return mWait;
            }
        }


        //yield return wait;
        mMesh.vertices = mVertices;
        //yield return mWait;
    }

    private IEnumerator CreateTriangles()
    {
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
                mMesh.triangles = triangles;
                yield return mWait;
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }

        t = CreateTopFace(triangles, t, ring);

        mMesh.triangles = triangles;

        yield return mWait;
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        Debug.Log($"Ring size: {ring}");
        // Create first row
        int v = ring * YSize;
        for (int x = 0; x < XSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        // v + 2 
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        int vMin = ring * (YSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < ZSize - 1; z++, vMin--, vMid++, vMax++)
        {
            // Middle rows
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + XSize - 1);
            for (int x = 1; x < XSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + XSize - 1, vMid + XSize);
            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + XSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
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


    private void OnDrawGizmos()
    {
        if (mVertices == null)
            return;

        Gizmos.color = Color.black;
        for (int i = 0; i < mVertices.Length; i++)
        {
            var vertexWorldSpace = transform.localToWorldMatrix * mVertices[i];
            Gizmos.DrawSphere(vertexWorldSpace, 0.1f);
            Handles.Label(vertexWorldSpace, $"index: {i}");
        }

    }
}

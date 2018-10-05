using System.Collections;
using UnityEditor;
using UnityEngine;

// Class showing the creation of the cube-mesh animated.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMesh
    : MonoBehaviour
{
    public int XSize;
    public int YSize;
    public int ZSize;

    private Mesh mMesh;
    private Vector3[] mVertices;



    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        mMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mMesh;

        CreateVertices();
        CreateTriangles();
    }

    private void CreateVertices()
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
            }

            for (int z = 1; z <= ZSize; z++)
            {
                mVertices[v++] = new Vector3(XSize, y, z);
            }

            for (int x = XSize - 1; x >= 0; x--)
            {
                mVertices[v++] = new Vector3(x, y, ZSize);
            }

            for (int z = ZSize - 1; z > 0; z--)
            {
                mVertices[v++] = new Vector3(0, y, z);
            }
        }

        // Add top and bottom faces.
        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, YSize, z);
            }
        }

        for (int z = 1; z < ZSize; z++)
        {
            for (int x = 1; x < XSize; x++)
            {
                mVertices[v++] = new Vector3(x, 0, z);
            }
        }

        mMesh.vertices = mVertices;
    }

    private void CreateTriangles()
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
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }

        t = CreateTopFace(triangles, t, ring);
        t = CreateBottomFace(triangles, t, ring);

        mMesh.triangles = triangles;
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        // Create first row
        int v = ring * YSize;
        for (int x = 0; x < XSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
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

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        // First Row
        int v = 1;
        int vMid = mVertices.Length - (XSize - 1) * (ZSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < XSize - 1; x++, vMid++, v++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        // Middle Rows
        int vMin = ring - 2;
        vMid -= XSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < ZSize - 1; z++, vMin--, vMax++, vMid++)
        {
            t = SetQuad(triangles, t, vMin, vMid + XSize - 1, vMin + 1, vMid);
            for (int x = 1; x < XSize - 1; x++, vMid++, v++)
            {
                t = SetQuad(triangles, t, vMid + XSize - 1, vMid + XSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + XSize - 1, vMax + 1, vMid, vMax);
        }

        // Last Row
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

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
}

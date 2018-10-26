using System.Collections;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnimatedPlaneMesh
    : MonoBehaviour
{
    public int XSize = 1;
    public int YSize = 1;

    private Vector3[] mVertices;
    private Mesh mMesh;

    private void Awake()
    {
        StartCoroutine(GenerateNonInclusive());
    }

    private IEnumerator Generate()
    {
        // Code from: https://catlikecoding.com/unity/tutorials/procedural-grid/
        // TODO: Want to setup vertices so that triangles are drawn in the same manner as other unity items.
        var wait = new WaitForSeconds(0.05f);

        mMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mMesh;
        mMesh.name = "Procedural Grid";

        // Setup Vertices, uvs and tangents
        // +1 to get extra vertices for corner of each quad.
        mVertices = new Vector3[(XSize + 1) * (YSize + 1)];
        var uvs = new Vector2[mVertices.Length];
        var tangents = new Vector4[mVertices.Length];

        for (int i = 0, y = 0; y <= YSize; y++)
        {
            for (int x = 0; x <= XSize; x++, i++)
            {
                mVertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2(x / (float)XSize, y / (float)YSize);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        mMesh.vertices = mVertices;
        mMesh.uv = uvs;
        mMesh.tangents = tangents;

        // Setup Triangles
        int[] triangles = new int[XSize * YSize * 6];

        // ti = triangleIndex, vi = vertexIdx
        // Extra incrementing on the vertex index since there is one more vertex than tiles per row.
        for (int ti = 0, vi = 0, y = 0; y < YSize; y++, vi++)
        {
            for (int x = 0; x < XSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + XSize + 1;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + XSize + 1;
                triangles[ti + 5] = vi + XSize + 2;
            }
            mMesh.triangles = triangles;
            yield return wait;

        }

        mMesh.triangles = triangles;
        mMesh.RecalculateNormals();

        yield return wait;
    }

    private IEnumerator GenerateNonInclusive()
    {
        // Code from: https://catlikecoding.com/unity/tutorials/procedural-grid/
        // TODO: Want to setup vertices so that triangles are drawn in the same manner as other unity items.
        var wait = new WaitForSeconds(0.05f);

        mMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mMesh;
        mMesh.name = "Procedural Grid";

        // Setup Vertices, uvs and tangents
        // +1 to get extra vertices for corner of each quad.
        mVertices = new Vector3[XSize * YSize];
        var uvs = new Vector2[mVertices.Length];
        var tangents = new Vector4[mVertices.Length];

        for (int i = 0, y = 0; y < YSize; y++)
        {
            for (int x = 0; x < XSize; x++, i++)
            {
                mVertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2(x / (float)XSize, y / (float)YSize);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        mMesh.vertices = mVertices;
        mMesh.uv = uvs;
        mMesh.tangents = tangents;

        // Setup Triangles
        int[] triangles = new int[(XSize - 1) * (YSize - 1) * 6];

        // ti = triangleIndex, vi = vertexIdx
        // Extra incrementing on the vertex index since there is one more vertex than tiles per row.
        // Redo Index logic, once it is in place, stuff should work properly.
        for (int ti = 0, vi = 0, y = 0; y < YSize - 1; y++, vi++)
        {
            for (int x = 0; x < XSize - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + XSize;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + XSize;
                triangles[ti + 5] = vi + XSize + 1;
            }
            mMesh.triangles = triangles;
            yield return wait;
        }


        //for (int ti = 0, vi = 0, y = 0; y < YSize; y++)
        //{
        //    for (int x = 0; x < XSize; x++, ti += 6, vi++)
        //    {
        //        triangles[ti] = vi;
        //        triangles[ti + 1] = vi + XSize + 1;
        //        triangles[ti + 2] = vi + 1;
        //        triangles[ti + 3] = vi + 1;
        //        triangles[ti + 4] = vi + XSize + 1;
        //        triangles[ti + 5] = vi + XSize + 2;
        //    }
        //    mMesh.triangles = triangles;
        //    yield return wait;

        //}

        //mMesh.triangles = triangles;
        //mMesh.RecalculateNormals();

        yield return wait;
    }

    private void OnDrawGizmos()
    {
        if (mVertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < mVertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(mVertices[i]), 0.1f);
            //Handles.Label(transform.TransformPoint(mVertices[i]), $"{i}");
        }
    }

}

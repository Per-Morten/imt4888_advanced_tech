using UnityEngine;

public class Procedural : MonoBehaviour
{
    // Change signature so it takes: Rows, Columns, XSize, YSize
    // Generates a PlaneMesh consisting of individual Quads.
    // Heads up: Vertex coordinates are not "triangle triangle triangle"
    //           But rather follows the entire row all the way to the end.
    //           See: https://catlikecoding.com/unity/tutorials/procedural-grid/02-grid-indices.png
    public static Mesh GeneratePlaneMesh(int XSize, int YSize)
    {
        // Code from: https://catlikecoding.com/unity/tutorials/procedural-grid/
        // TODO: Want to setup vertices so that triangles are drawn in the same manner as other unity items.
        

        // Setup Vertices, uvs and tangents
        // +1 to get extra vertices for corner of each quad.
        var vertices = new Vector3[(XSize + 1) * (YSize + 1)];
        var uvs = new Vector2[vertices.Length];
        var tangents = new Vector4[vertices.Length];

        for (int i = 0, y = 0; y <= YSize; y++)
        {
            for (int x = 0; x <= XSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2(x / (float)XSize, y / (float)YSize);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

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
        }

        var mesh = new Mesh
        {
            name = "Procedural Grid",
            vertices = vertices,
            uv = uvs,
            tangents = tangents,
            triangles = triangles
        };
        mesh.RecalculateNormals();

        return mesh;
    }
}

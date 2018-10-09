using System;
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

    // Create standard mesh without roundedness & without normals
    // Create this so that the pivot is still in the middle of the object, so we don't mess up when rotating the object.
    public static Mesh GenerateCubeMesh(int XSize, int YSize, int ZSize)
    {
        int cornerVertices = 8;
        int edgeVertices = (XSize + YSize + ZSize - 3) * 4;
        int faceVertices = (
            (XSize - 1) * (YSize - 1) +
            (XSize - 1) * (ZSize - 1) +
            (YSize - 1) * (ZSize - 1)) * 2;

        var vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        // Create Vertices
        {
            // Create Perimeter of the cube
            int v = 0; // Vertex
            for (int y = 0; y <= YSize; y++)
            {
                for (int x = 0; x <= XSize; x++)
                {
                    vertices[v++] = new Vector3(x, y, 0);
                }

                for (int z = 1; z <= ZSize; z++)
                {
                    vertices[v++] = new Vector3(XSize, y, z);
                }

                for (int x = XSize - 1; x >= 0; x--)
                {
                    vertices[v++] = new Vector3(x, y, ZSize);
                }

                for (int z = ZSize - 1; z > 0; z--)
                {
                    vertices[v++] = new Vector3(0, y, z);
                }
            }

            // Add top and bottom faces.
            for (int z = 1; z < ZSize; z++)
            {
                for (int x = 1; x < XSize; x++)
                {
                    vertices[v++] = new Vector3(x, YSize, z);
                }
            }

            for (int z = 1; z < ZSize; z++)
            {
                for (int x = 1; x < XSize; x++)
                {
                    vertices[v++] = new Vector3(x, 0, z);
                }
            }
        }

        var triangles = new int[(XSize * YSize + XSize * ZSize + YSize * ZSize) * 2 * 6];
        // Create triangles
        {
            Func<int, int, int, int, int, int> SetQuad = (i, v00, v10, v01, v11) =>
            {
                triangles[i] = v00;
                triangles[i + 1] = v01;
                triangles[i + 2] = v10;
                triangles[i + 3] = v10;
                triangles[i + 4] = v01;
                triangles[i + 5] = v11;
                return i + 6;
            };

            int ring = 0;
            int t = 0;
            // Create Perimiter Triangles
            {
                //int quads = (XSize * YSize + XSize * ZSize + YSize * ZSize) * 2;
                //triangles = new int[quads * 6];

                ring = (XSize + ZSize) * 2;
                t = 0; // Triangle
                int v = 0; // Vertex

                for (int y = 0; y < YSize; y++, v++)
                {
                    // q = quad
                    // Need to reuse the second and fourth vertex in each ring to wrap around properly,
                    // otherwise the quad will go diagonally up.
                    for (int q = 0; q < ring - 1; q++, v++)
                    {
                        t = SetQuad(t, v, v + 1, v + ring, v + ring + 1);
                    }
                    t = SetQuad(t, v, v - ring + 1, v + ring, v + 1);
                }
            }

            // Create Top Triangles
            {
                // Create first row
                int v = ring * YSize;
                for (int x = 0; x < XSize - 1; x++, v++)
                {
                    t = SetQuad(t, v, v + 1, v + ring - 1, v + ring);
                }
                t = SetQuad(t, v, v + 1, v + ring - 1, v + 2);

                int vMin = ring * (YSize + 1) - 1;
                int vMid = vMin + 1;
                int vMax = v + 2;

                for (int z = 1; z < ZSize - 1; z++, vMin--, vMid++, vMax++)
                {
                    // Middle rows
                    t = SetQuad(t, vMin, vMid, vMin - 1, vMid + XSize - 1);
                    for (int x = 1; x < XSize - 1; x++, vMid++)
                    {
                        t = SetQuad(t, vMid, vMid + 1, vMid + XSize - 1, vMid + XSize);
                    }
                    t = SetQuad(t, vMid, vMax, vMid + XSize - 1, vMax + 1);
                }

                int vTop = vMin - 2;
                t = SetQuad(t, vMin, vMid, vTop + 1, vTop);
                for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
                {
                    t = SetQuad(t, vMid, vMid + 1, vTop, vTop - 1);
                }
                t = SetQuad(t, vMid, vTop - 2, vTop, vTop - 1);
            }

            // Create Bottom Triangles
            {
                // First Row
                int v = 1;
                int vMid = vertices.Length - (XSize - 1) * (ZSize - 1);
                t = SetQuad(t, ring - 1, vMid, 0, 1);
                for (int x = 1; x < XSize - 1; x++, vMid++, v++)
                {
                    t = SetQuad(t, vMid, vMid + 1, v, v + 1);
                }
                t = SetQuad(t, vMid, v + 2, v, v + 1);

                // Middle Rows
                int vMin = ring - 2;
                vMid -= XSize - 2;
                int vMax = v + 2;

                for (int z = 1; z < ZSize - 1; z++, vMin--, vMax++, vMid++)
                {
                    t = SetQuad(t, vMin, vMid + XSize - 1, vMin + 1, vMid);
                    for (int x = 1; x < XSize - 1; x++, vMid++, v++)
                    {
                        t = SetQuad(t, vMid + XSize - 1, vMid + XSize, vMid, vMid + 1);
                    }
                    t = SetQuad(t, vMid + XSize - 1, vMax + 1, vMid, vMax);
                }

                // Last Row
                int vTop = vMin - 1;
                t = SetQuad(t, vTop + 1, vTop, vTop + 2, vMid);
                for (int x = 1; x < XSize - 1; x++, vTop--, vMid++)
                {
                    t = SetQuad(t, vTop, vTop - 1, vMid, vMid + 1);
                }
                t = SetQuad(t, vTop, vTop - 1, vMid, vTop - 2);

            }
        }
        var mesh = new Mesh
        {
            name = "Procedural Cube",
            vertices = vertices,
            triangles = triangles,
        };

        return mesh;
    }

    public static Mesh GenerateCubeSphere(int GridSize, float Radius)
    {
        int cornerVertices = 8;
        int edgeVertices = (GridSize + GridSize + GridSize - 3) * 4;
        int faceVertices = (
            (GridSize - 1) * (GridSize - 1) +
            (GridSize - 1) * (GridSize - 1) +
            (GridSize - 1) * (GridSize - 1)) * 2;

        Action<Vector3[], Vector3[], Color32[], int, float, int, int, int, int> SetVertex = (vertices_, normals_, uvs_, gridsize, radius,i, x, y, z) =>
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
            normals_[i] = s;
            vertices_[i] = normals_[i] * Radius;
            uvs_[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
        };

        var vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        var normals = new Vector3[vertices.Length];
        var uvs = new Color32[vertices.Length];

        // Create Vertices
        {
            // Create Perimeter of the cube
            int v = 0; // Vertex
            for (int y = 0; y <= GridSize; y++)
            {
                for (int x = 0; x <= GridSize; x++)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, x, y, 0);
                }

                for (int z = 1; z <= GridSize; z++)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, GridSize, y, z);
                }

                for (int x = GridSize - 1; x >= 0; x--)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, x, y, GridSize);
                }

                for (int z = GridSize - 1; z > 0; z--)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, 0, y, z);
                }
            }

            // Add top and bottom faces.
            for (int z = 1; z < GridSize; z++)
            {
                for (int x = 1; x < GridSize; x++)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, x, GridSize, z);
                }
            }

            for (int z = 1; z < GridSize; z++)
            {
                for (int x = 1; x < GridSize; x++)
                {
                    SetVertex(vertices, normals, uvs, GridSize, Radius, v++, x, 0, z);
                }
            }
        }

        int quads = (GridSize * GridSize + GridSize * GridSize + GridSize * GridSize) * 2;
        var triangles = new int[quads * 6];
        //var triangles = new int[(GridSize * GridSize + GridSize * GridSize + GridSize * GridSize) * 2 * 6];
        // Create triangles
        {
            Func<int, int, int, int, int, int> SetQuad = (i, v00, v10, v01, v11) =>
            {
                triangles[i] = v00;
                triangles[i + 1] = v01;
                triangles[i + 2] = v10;
                triangles[i + 3] = v10;
                triangles[i + 4] = v01;
                triangles[i + 5] = v11;
                return i + 6;
            };

            int ring = 0;
            int t = 0;
            // Create Perimiter Triangles
            {
                //int quads = (XSize * YSize + XSize * ZSize + YSize * ZSize) * 2;
                //triangles = new int[quads * 6];

                ring = (GridSize + GridSize) * 2;
                t = 0; // Triangle
                int v = 0; // Vertex

                for (int y = 0; y < GridSize; y++, v++)
                {
                    // q = quad
                    // Need to reuse the second and fourth vertex in each ring to wrap around properly,
                    // otherwise the quad will go diagonally up.
                    for (int q = 0; q < ring - 1; q++, v++)
                    {
                        t = SetQuad(t, v, v + 1, v + ring, v + ring + 1);
                    }
                    t = SetQuad(t, v, v - ring + 1, v + ring, v + 1);
                }
            }

            // Create Top Triangles
            {
                // Create first row
                int v = ring * GridSize;
                for (int x = 0; x < GridSize - 1; x++, v++)
                {
                    t = SetQuad(t, v, v + 1, v + ring - 1, v + ring);
                }
                t = SetQuad(t, v, v + 1, v + ring - 1, v + 2);

                int vMin = ring * (GridSize + 1) - 1;
                int vMid = vMin + 1;
                int vMax = v + 2;

                for (int z = 1; z < GridSize - 1; z++, vMin--, vMid++, vMax++)
                {
                    // Middle rows
                    t = SetQuad(t, vMin, vMid, vMin - 1, vMid + GridSize - 1);
                    for (int x = 1; x < GridSize - 1; x++, vMid++)
                    {
                        t = SetQuad(t, vMid, vMid + 1, vMid + GridSize - 1, vMid + GridSize);
                    }
                    t = SetQuad(t, vMid, vMax, vMid + GridSize - 1, vMax + 1);
                }

                int vTop = vMin - 2;
                t = SetQuad(t, vMin, vMid, vTop + 1, vTop);
                for (int x = 1; x < GridSize - 1; x++, vTop--, vMid++)
                {
                    t = SetQuad(t, vMid, vMid + 1, vTop, vTop - 1);
                }
                t = SetQuad(t, vMid, vTop - 2, vTop, vTop - 1);
            }

            // Create Bottom Triangles
            {
                // First Row
                int v = 1;
                int vMid = vertices.Length - (GridSize - 1) * (GridSize - 1);
                t = SetQuad(t, ring - 1, vMid, 0, 1);
                for (int x = 1; x < GridSize - 1; x++, vMid++, v++)
                {
                    t = SetQuad(t, vMid, vMid + 1, v, v + 1);
                }
                t = SetQuad(t, vMid, v + 2, v, v + 1);

                // Middle Rows
                int vMin = ring - 2;
                vMid -= GridSize - 2;
                int vMax = v + 2;

                for (int z = 1; z < GridSize - 1; z++, vMin--, vMax++, vMid++)
                {
                    t = SetQuad(t, vMin, vMid + GridSize - 1, vMin + 1, vMid);
                    for (int x = 1; x < GridSize - 1; x++, vMid++, v++)
                    {
                        t = SetQuad(t, vMid + GridSize - 1, vMid + GridSize, vMid, vMid + 1);
                    }
                    t = SetQuad(t, vMid + GridSize - 1, vMax + 1, vMid, vMax);
                }

                // Last Row
                int vTop = vMin - 1;
                t = SetQuad(t, vTop + 1, vTop, vTop + 2, vMid);
                for (int x = 1; x < GridSize - 1; x++, vTop--, vMid++)
                {
                    t = SetQuad(t, vTop, vTop - 1, vMid, vMid + 1);
                }
                t = SetQuad(t, vTop, vTop - 1, vMid, vTop - 2);

            }
        }
        var mesh = new Mesh
        {
            name = "Procedural Cube",
            vertices = vertices,
            triangles = triangles,
            normals = normals,
            colors32 = uvs,
        };

        return mesh;
    }
}

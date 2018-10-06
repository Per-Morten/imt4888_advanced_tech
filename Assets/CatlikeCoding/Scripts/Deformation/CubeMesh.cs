using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMesh : MonoBehaviour
{
    Mesh mMesh;

    void Start()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GenerateCubeMesh(4, 4, 4);
        mMesh = filter.mesh;
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMesh : MonoBehaviour
{
    Mesh mMesh;
    public int XSize = 8;
    public int YSize = 8;
    public int ZSize = 8;
    public int Roundness = 0;


    void Start()
    {
        SetupCube();
    }

    [ContextMenu("Generate")]
    public void Regenerate()
    {
        SetupCube();
    }

    private void SetupCube()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GenerateCubeMesh(XSize, YSize, ZSize);
        mMesh = filter.mesh;


        var mat = GetComponent<Renderer>().material;
        mat.SetInt("_XSize", XSize);
        mat.SetInt("_YSize", YSize);
        mat.SetInt("_ZSize", ZSize);
        mat.SetInt("_Roundness", Roundness);


        //mat.GetFloat("XSize")
    }
}

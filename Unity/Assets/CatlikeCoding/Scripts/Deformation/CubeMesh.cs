using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMesh : MonoBehaviour
{
    private Mesh mMesh;
    public int XSize = 8;
    public int YSize = 8;
    public int ZSize = 8;
    public int Roundness = 0;

    Vector3[] mVertices;

    private void Start()
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

        mVertices = mMesh.vertices;
        //mat.GetFloat("XSize")
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
            Gizmos.DrawSphere(vertexWorldSpace, 0.05f);
            Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(mVertices[i], mNormals[i]);
            Handles.Label(vertexWorldSpace, $"{i}");
        }

    }
}

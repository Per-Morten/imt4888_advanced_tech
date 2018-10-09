using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour
{
    Mesh mMesh;
    public int GridSize = 1;
    public int Radius = 1;


    void Start()
    {
        SetupSphere();
        gameObject.AddComponent<SphereCollider>();
    }

    [ContextMenu("Generate")]
    public void Regenerate()
    {
        SetupSphere();
    }

    private void SetupSphere()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GenerateCubeSphere(GridSize, Radius);
        mMesh = filter.mesh;
    }
}

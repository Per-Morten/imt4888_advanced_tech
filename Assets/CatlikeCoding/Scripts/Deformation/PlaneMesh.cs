using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlaneMesh : MonoBehaviour
{
    private Mesh mMesh;
    public int Rows = 10;
    public int Columns = 10;
    
    void Awake()
    {
        SetupPlane();
        gameObject.AddComponent<BoxCollider>();
    }

    [ContextMenu("Generate")]
    public void Regenerate()
    {
        SetupPlane();
    }

    private void SetupPlane()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GeneratePlaneMesh(Rows, Columns);
        mMesh = filter.mesh;
    }
}

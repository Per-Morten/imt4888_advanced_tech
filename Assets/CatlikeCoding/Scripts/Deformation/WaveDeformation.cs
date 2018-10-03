using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaveDeformation : MonoBehaviour
{
    Mesh mMesh;
    Vector3[] mOriginalVertices;
    Vector3[] mModifiedVertices;
    float mWave = 0;

    // Use this for initialization
    void Start()
    {
        var filter = GetComponent<MeshFilter>();
        filter.mesh = Procedural.GeneratePlaneMesh(45, 45);
        mMesh = filter.mesh;
        mOriginalVertices = mMesh.vertices;
        
        mModifiedVertices = (Vector3[])mOriginalVertices.Clone();
    }

    // Update is called once per frame
    void Update()
    {
        // -1.0
        // -0.5
        //  0
        //  0.5
        //  1.0

        // Wavy effect
        for (int i = 0; i < mOriginalVertices.Length; i++)
        {
            var orig = mOriginalVertices[i];
            var zOffset = Mathf.Sin(Mathf.Deg2Rad * (mWave + i));
            mModifiedVertices[i] = new Vector3(orig.x, orig.y, orig.z + zOffset);
        }
        mWave += 0.5f;

        mMesh.vertices = mModifiedVertices;
        mMesh.RecalculateNormals();

        Debug.Log(Mathf.Sin(Time.timeSinceLevelLoad));
    }
}

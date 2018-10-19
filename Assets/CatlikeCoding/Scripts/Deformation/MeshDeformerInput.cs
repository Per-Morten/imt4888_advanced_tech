using UnityEngine;
using System.Collections;

public class MeshDeformerInput : MonoBehaviour
{
    public float Force = 10f;
    public float ForceOffset = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                var deformer = hit.collider.GetComponent<IDeformer>();
                if (deformer != null)
                {
                    var point = hit.point;
                    point += hit.normal * ForceOffset;
                    deformer.AddDeformingForce(point, Force);
                }
            }
        }
    }
}

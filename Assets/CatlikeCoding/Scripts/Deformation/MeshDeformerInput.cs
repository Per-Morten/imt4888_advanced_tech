using UnityEngine;
using System.Collections.Generic;

public class MeshDeformerInput : MonoBehaviour
{
    public float Force = 10f;
    public float ForceOffset = 0.1f;
    public float Radius = 0.01f;

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            PushDeformation();
        }
        if (Input.GetKey(KeyCode.K))
        {
            RingDeformation();
        }
    }

    private void PushDeformation()
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

    private void RingDeformation()
    {
        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //var mouseDir = mouseRay.origin + mouseRay.direction;
        var step = (Mathf.PI * 2) / 16;
        var dirs = new List<Ray>();
        for (float i = 0; i < Mathf.PI * 2; i += step)
        {
            var ray = mouseRay;
            ray.direction = mouseRay.direction + new Vector3(mouseRay.direction.x + Radius * Mathf.Cos(i), mouseRay.direction.y + Radius * Mathf.Sin(i));
            //ray.direction = new Vector3(mouseRay.direction.x + Radius * Mathf.Cos(i),
            //                            mouseRay.direction.y + Radius * Mathf.Sin(i));

            dirs.Add(ray);
        }

        var hits = new List<RaycastHit>();
        for (int i = 0; i < dirs.Count; i++)
        {
            var r = dirs[i];
            RaycastHit hit;
            if (Physics.Raycast(r, out hit))
            {
                hits.Add(hit);
            }
        }

        for (int i = 0; i < hits.Count; i++)
        {
            var hit = hits[i];
            var deformer = hit.collider.GetComponent<IDeformer>();
            if (deformer != null)
            {
                var point = hit.point;
                point += hit.normal * ForceOffset;

                // Force should be based on total number of hits, rather than the number of dirs.
                deformer.AddDeformingForce(point, Force / hits.Count);
            }
        }

        Debug.Log($"{mouseRay}");
        //var mouseInWorld = Camera.main.ScreenToWorldPoint(mousePos);

        for (int i = 0; i < dirs.Count; i++)
        {
            //var worldPos = Camera.main.ScreenToWorldPoint(points[i]);
            DebugLines.DrawLine(dirs[i].origin, dirs[i].origin + dirs[i].direction, Color.red);
        }
    }
}
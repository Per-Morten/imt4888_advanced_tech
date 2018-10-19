using System.Collections.Generic;
using UnityEngine;


public class DebugLines : MonoBehaviour
{
    public void Start()
    {
        mParentObject = new GameObject("DebugLines");
    }

    public static void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        var go = new GameObject("DebugLine");
        go.transform.parent = mParentObject.transform;
        var line = go.AddComponent<LineRenderer>();


        line.material = new Material(Shader.Find("Diffuse"));
        line.material.color = color;
        line.positionCount = 2;

        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;

        mLines.Add(go);
    }

    public static void DrawAxes(Transform t)
    {
        DrawLine(t.position, t.position + t.forward * 0.25f, Color.blue);
        DrawLine(t.position, t.position + t.right * 0.25f, Color.red);
        DrawLine(t.position, t.position + t.up * 0.25f, Color.green);
    }

    // Draws the angle between from and to.
    // Draws it as if it was a circle where drawCenter was in the middle.
    // Drawing draws from drawDirection. So if you want to draw right infront of the headset for example, use headset.forward as drawDirection.
    // If middleOut is enabled, rather than drawing from drawDirection we draw so that the middle of the angle ends up at drawDirection.
    public static void DrawAngleBetween(Vector3 from, Vector3 to, Vector3 drawCenter, Vector3 drawDirection, Color color, bool middleOut = false)
    {
        var from2D = new Vector3(from.x, 0.0f, from.z).normalized;
        var to2D = new Vector3(to.x, 0.0f, to.z).normalized;
        var end = Vector3.Angle(from2D, to2D);

        var angle = (middleOut) ? (end / 2f) : 0f;

        const int Segments = 30;
        var dir = drawDirection.normalized;
        for (int i = 0; i <= Segments; i++)
        {
            var previous = Quaternion.AngleAxis(angle, Vector3.up) * dir;
            angle += (end / Segments);
            var next = Quaternion.AngleAxis(angle, Vector3.up) * dir;
            DrawLine(drawCenter + previous, drawCenter + next, color);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var item in mLines)
        {
            Destroy(item);
        }
        mLines.Clear();
    }

    private void OnGUI()
    {
        // Want to remove all lines so that they won't show up next frame.
        foreach (var item in mLines)
        {
            Destroy(item);
        }
        mLines.Clear();
    }

    private static List<GameObject> mLines = new List<GameObject>();
    private static GameObject mParentObject;
}

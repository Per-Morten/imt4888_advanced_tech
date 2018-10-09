using UnityEngine;
using System.Collections;

public class CircleGizmo : MonoBehaviour
{
    public int Resolution = 10;

    private void OnDrawGizmosSelected()
    {
        float step = 2f / Resolution;
        for (int i = 0; i <= Resolution; i++)
        {
            ShowPoint(i * step - 1f, -1f);
            ShowPoint(i * step - 1f, 1f);
        }
        for (int i = 1; i < Resolution; i++)
        {
            ShowPoint(-1f, i * step - 1f);
            ShowPoint(1f, i * step - 1f);
        }
    }

    private void ShowPoint(float x, float y)
    {
        var square = new Vector2(x, y);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(square, 0.025f);

        //var circle = square.normalized;
        Vector2 circle;
        circle.x = square.x * Mathf.Sqrt(1f - square.y * square.y * 0.5f);
        circle.y = square.y * Mathf.Sqrt(1f - square.x * square.x * 0.5f);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(circle, 0.025f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(square, circle);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(circle, Vector2.zero);
    }
}

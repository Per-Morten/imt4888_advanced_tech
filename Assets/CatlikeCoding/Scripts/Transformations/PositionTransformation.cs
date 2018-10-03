using UnityEngine;
using System.Collections;

public class PositionTransformation : Transformation
{
    [SerializeField]
    private Vector3 mPosition;

    //public override Vector3 Apply(Vector3 point)
    //{
    //    return point + mPosition;
    //}

    public override Matrix4x4 Matrix
    {
        get
        {
            var matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(1f, 0f, 0f, mPosition.x));
            matrix.SetRow(1, new Vector4(0f, 1f, 0f, mPosition.y));
            matrix.SetRow(2, new Vector4(0f, 0f, 1f, mPosition.z));
            matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
            return matrix;
        }
    }
}

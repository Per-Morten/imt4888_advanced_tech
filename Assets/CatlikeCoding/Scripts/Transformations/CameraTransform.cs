using UnityEngine;
using System.Collections;

public class CameraTransform : Transformation

{
    [SerializeField]
    private float mFocalLength = 1f;

    public override Matrix4x4 Matrix
    {
        get
        {
            var matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(mFocalLength, 0f, 0f, 0f));
            matrix.SetRow(1, new Vector4(0f, mFocalLength, 0f, 0f));
            matrix.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
            matrix.SetRow(3, new Vector4(0f, 0f, 1f, 0f));
            return matrix;
        }
    }
}

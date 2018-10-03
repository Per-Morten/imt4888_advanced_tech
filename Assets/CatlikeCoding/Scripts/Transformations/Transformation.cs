﻿using UnityEngine;
using System.Collections;

public abstract class Transformation : MonoBehaviour
{
    public Vector3 Apply(Vector3 point)
    {
        return Matrix.MultiplyPoint(point);
    }

    public abstract Matrix4x4 Matrix { get; }
}

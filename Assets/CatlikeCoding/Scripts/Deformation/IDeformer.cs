using UnityEngine;
using System.Collections;

public interface IDeformer
{
    void AddDeformingForce(Vector3 point, float force);
}

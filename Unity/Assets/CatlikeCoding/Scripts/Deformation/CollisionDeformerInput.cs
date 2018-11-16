using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CollisionDeformerInput : MonoBehaviour
{
    bool mIsColliding = false;
    ContactPoint[] mContactPoints;

    IDeformer mDeformer;

    public float mCollisionDeformerForce = 10;

    private void Start()
    {
         mDeformer = GetComponent<IDeformer>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter");
        mIsColliding = true;
        mContactPoints = collision.contacts;
        //Debug.Log("Enter");
        //collision.rigidbody.isKinematic = false;
        //rb.isKinematic = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit");

        mIsColliding = false;
        mContactPoints = collision.contacts;
        //Debug.Log("Leave");
        //rb.isKinematic = true;
        //collision.rigidbody.isKinematic = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Stay");
        mContactPoints = collision.contacts;
        // Debug-draw all contact points and normals
        //foreach (ContactPoint contact in collisions.contacts)
        //{
        //    Debug.Log("Collision");
        //    DebugLines.DrawLine(contact.point, contact.point + contact.normal, Color.red);
        //    //Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
        //}
    }

    private void Update()
    {
        if (mIsColliding)
        {
            foreach (var contact in mContactPoints)
            {
                var point = contact.point;
                point -= contact.normal * 0.1f;
                mDeformer.AddDeformingForce(point, mCollisionDeformerForce);
                DebugLines.DrawLine(transform.position, point, Color.red);
            }
        }
    }


    //private void OnTriggerStay(Collider other)
    //{
    //    RaycastHit hit;
    //    if (Physics.Raycast(transform.position, transform.position - other.transform.position, out hit))
    //    {
    //        Debug.Log("Point of contact: " + hit.point);
    //        DebugLines.DrawLine(transform.position, hit.collider.gameObject.transform.position, Color.blue);
    //    }
    //    else
    //    {
    //        Debug.Log("Not hit");
    //    }

    //    //        other.attachedRigidbody.

    //    Debug.Log("Triggered");
    //}
}

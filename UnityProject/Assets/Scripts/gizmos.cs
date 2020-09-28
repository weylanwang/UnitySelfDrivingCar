using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizmos : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.white;
        Vector3 direction = transform.TransformDirection(Vector3.up) * 5f;
        Gizmos.DrawRay(transform.position, direction);
    }
}
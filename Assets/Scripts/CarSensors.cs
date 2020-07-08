using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSensors : MonoBehaviour
{
    #region Variables
    [SerializeField]
    [Tooltip("The max range of a sensor")]
    private float range;

    private Ray2D shootRay;
    private RaycastHit2D shootHit;
    private int shootableMask;
    private LineRenderer sensorLine;
    private Transform parentthing;
    #endregion
    void Start() {
        sensorLine = GetComponent<LineRenderer>();
        sensorLine.enabled = true;
        parentthing = GetComponentInParent<Transform>();
        shootableMask = LayerMask.GetMask("Wall");
    }

    private void Update()
    {
        sensorLine.SetPosition(0, transform.position);
        sensorLine.SetPosition(1, transform.position + transform.up * range);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, range, shootableMask);

        if (hit.collider != null) {
            sensorLine.SetPosition(1, hit.point);
            Debug.Log("Hit object: " + hit.point);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSensors : MonoBehaviour
{
    #region Variables
    [SerializeField]
    [Tooltip("The max range of a sensor")]
    private float range;

    private int shootableMask;
    private LineRenderer sensorLine;
    private float distance;
    #endregion
    void Start() {
        sensorLine = GetComponent<LineRenderer>();
        sensorLine.enabled = true;
        shootableMask = LayerMask.GetMask("Wall");
    }

    private void Update() {
        sensorLine.SetPosition(0, transform.position);
        sensorLine.SetPosition(1, transform.position + transform.up * range);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, range, shootableMask);

        if (hit.collider != null) {
            sensorLine.SetPosition(1, hit.point);
            distance = hit.distance;
        }
        else distance = range;
    }

    public float GetSensorReading() {
        return distance;
    }
}

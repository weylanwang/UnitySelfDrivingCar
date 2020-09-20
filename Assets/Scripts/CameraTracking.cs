using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    private Transform target;

    // Updates the camera to the position of the best car
    private void LateUpdate() {
        if (target != null)
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    // Sets the camera to track the best performing car
    public void SetTarget(Transform bestCar) {
        target = bestCar;
    }
}

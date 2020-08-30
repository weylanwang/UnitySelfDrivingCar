using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    private Transform target;

    private void LateUpdate() {
        if (target != null)
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    public void SetTarget(Transform bestCar) {
        target = bestCar;
    }
}

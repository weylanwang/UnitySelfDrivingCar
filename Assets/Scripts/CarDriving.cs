using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriving : MonoBehaviour
{
    #region Variables
    [SerializeField]
    [Tooltip("How fast the car can accelerate, as well as its max speed")]
    private float enginePower;

    [SerializeField]
    [Tooltip("Car's turning/handling power")]
    private float turningPower;

    private float horizontalAxis;
    private float verticalAxis;
    private float velocity;
    private float rotation;
    private Rigidbody2D carRB;
    #endregion

    #region Start/Update
    private void Start()
    {
        velocity = 0;
        carRB = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");
        ApplyManualControls();
    }
    #endregion

    #region Car Control Functions
    private void ApplyManualControls() {
        velocity = verticalAxis * enginePower;
        rotation = Mathf.Sign(Vector2.Dot(carRB.velocity, carRB.GetRelativeVector(Vector2.up)));

        //Calculation for how much to turn the car
        carRB.rotation += (-horizontalAxis) * turningPower * carRB.velocity.magnitude * rotation;
        carRB.AddRelativeForce(Vector2.up * velocity);

        //Small drift effect to make the car feel more realistic
        carRB.AddRelativeForce(-Vector2.right * carRB.velocity.magnitude * (-horizontalAxis / 2));
    }
    #endregion
}

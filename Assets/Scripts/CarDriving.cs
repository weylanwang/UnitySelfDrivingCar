using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriving : MonoBehaviour
{
    #region Variables
    #region Serialized Variables
    [SerializeField]
    [Tooltip("How fast the car can accelerate, as well as its max speed")]
    private float enginePower;

    [SerializeField]
    [Tooltip("Car's turning/handling power")]
    private float turningPower;

    [SerializeField]
    [Tooltip("The parent game object holding all the sensors")]
    private GameObject sensorParent;

    [SerializeField]
    [Tooltip("Is this car AI controlled?")]
    private bool AIControlled;

    [SerializeField]
    [Tooltip("The number of sensors that this car prefab has")]
    private int numSensors;
    #endregion

    #region Public Variables
    public delegate void WallCollision();
    public static event WallCollision WallCollisionEvent;
    #endregion

    #region Private variables
    private float horizontalAxis;
    private float verticalAxis;
    private float velocity;
    private float rotation;
    private Rigidbody2D carRB;
    private CarSensors[] sensors;
    #endregion
    #endregion

    #region Start/Update
    private void Start()
    {
        AIControlled = false;
        velocity = 0;
        carRB = GetComponent<Rigidbody2D>();

        sensors = new CarSensors[numSensors];
        for (int i = 0; i < numSensors; i++) {
            sensors[i] = sensorParent.transform.GetChild(i).GetComponent<CarSensors>();
        }
    }

    void FixedUpdate()
    {
        if (!AIControlled) {
            horizontalAxis = Input.GetAxis("Horizontal");
            verticalAxis = Input.GetAxis("Vertical");
        }
        ApplyControls();
    }
    #endregion

    #region Car Control Functions
    private void ApplyControls() {
        velocity = verticalAxis * enginePower;
        rotation = Mathf.Sign(Vector2.Dot(carRB.velocity, carRB.GetRelativeVector(Vector2.up)));

        //Calculation for how much to turn the car
        carRB.rotation += (-horizontalAxis) * turningPower * carRB.velocity.magnitude * rotation;
        carRB.AddRelativeForce(Vector2.up * velocity);

        //Small drift effect to make the car feel more realistic
        carRB.AddRelativeForce(-Vector2.right * carRB.velocity.magnitude * (-horizontalAxis / 2));
    }

    //Values should be between -1 and 1
    public void SetCarInputs(float[] inputs) {
        horizontalAxis = inputs[0];
        verticalAxis = inputs[1];
    }

    public float[] GetSensorReadings() {
        float[] sensorReadings = new float[numSensors];
        for (int i = 0; i < numSensors; i++) {
            sensorReadings[i] = sensors[i].GetSensorReading();
        }
        return sensorReadings;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        WallCollisionEvent();
    }
    #endregion
}

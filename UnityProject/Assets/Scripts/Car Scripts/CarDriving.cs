using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriving : MonoBehaviour {
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
    public int NumSensors { get { return numSensors; } }
    #endregion

    #region Public Variables
    // Contains the event to destroy the car when it collides with a wall
    public delegate void WallCollision();
    public event WallCollision WallCollisionEvent;
    #endregion

    #region Private variables
    // Horizontal axis input
    private float horizontalAxis;
    // Vertical axis input
    private float verticalAxis;
    private float velocity;
    public float Velocity { get { return velocity; } }
    private float rotation;
    private Rigidbody2D carRB;
    private int score;
    private int lastCheckpoint;
    private Vector3 trackPiecePosition;
    private float[] sensorReadings;

    public CarSensors[] sensors;
    public Agent agentScript;
    #endregion
    #endregion

    #region Start/Update
    // Set up scoring, backtrack checking, and references to sensors
    private void Start() {
        score = 0;
        lastCheckpoint = 0;
        trackPiecePosition = Vector3.zero;
        velocity = 0;
        carRB = GetComponent<Rigidbody2D>();
        sensorReadings = new float[numSensors];

        sensors = new CarSensors[numSensors];
        for (int i = 0; i < numSensors; i++) {
            sensors[i] = sensorParent.transform.GetChild(i).GetComponent<CarSensors>();
        }
    }

    // Apply controls (whether they're manual or AI driven) every physics frame
    void FixedUpdate() {
        if (!AIControlled) {
            horizontalAxis = Input.GetAxis("Horizontal");
            verticalAxis = Input.GetAxis("Vertical");
        }
        ApplyControls();
    }
    #endregion

    #region Car Control Functions
    private void ApplyControls() {
        // Speed up car per the engine's power
        velocity = verticalAxis * enginePower;

        // Calculate the rotational force of the car
        rotation = Mathf.Sign(Vector2.Dot(carRB.velocity, carRB.GetRelativeVector(Vector2.up)));

        // Application of the vertical and horizontal forces
        carRB.rotation += (-horizontalAxis) * turningPower * carRB.velocity.magnitude * rotation;
        carRB.AddRelativeForce(Vector2.up * velocity);

        // Small drift effect to make the car feel more realistic
        carRB.AddRelativeForce(-Vector2.right * carRB.velocity.magnitude * (-horizontalAxis / 2));
    }

    // Set the car's inputs if driven by AI. Values should be between -1 and 1
    public void SetCarInputs(float[] inputs) {
        horizontalAxis = inputs[0];
        verticalAxis = inputs[1];
        verticalAxis = inputs[1];
    }

    // Gets the data from each sensor (how far the line went before detecting an object)
    public float[] GetSensorReadings() {
        for (int i = 0; i < numSensors; i++) {
            sensorReadings[i] = sensors[i].GetSensorReading();
        }
        return sensorReadings;
    }

    // Trigger the wall collision event whenever a wall has been collided into
    private void OnCollisionEnter2D(Collision2D collision) {
        WallCollisionEvent();
    }
    #endregion

    #region Scoring
    // Returns the score of the car
    public int GetScore() {
        return score;
    }

    // Decreases the score of the car
    public void DecrementScore() {
        if (score > 0)
            score--;
    }

    // Tracks the last checkpoint the car has passed
    public void SetLastCheckpoint(int checkpoint, Vector3 newPosition) {
        int difference = checkpoint - lastCheckpoint;
        if (difference >= 1 && difference <= 2)
            score++;
        else if (difference <= -2 && trackPiecePosition != newPosition)
            score++;

        trackPiecePosition = newPosition;
        lastCheckpoint = checkpoint;
    }

    // Returns the last checkpoint the car has passed
    public int GetLastCheckpoint() {
        return lastCheckpoint;
    }

    // Awards points to the car when the car has hit the finish line
    public void AwardPoints(int points) {
        if (points > 0)
            score += points;
    }
    
    // Calls the event to destroy a car when it's not moving or wiggling
    public void FakeCollision() {
        Collision2D collision = null;
        OnCollisionEnter2D(collision);
    }
    #endregion
}

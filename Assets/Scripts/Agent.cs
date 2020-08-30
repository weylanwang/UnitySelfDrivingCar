using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    #region Variables
    // Contains the NeuralNetwork for the agent
    private NeuralNetwork brain;

    // Returns the NeuralNetwork of this agent
    public NeuralNetwork Brain { get { return brain; } }

    // Contains the Prefab to help instantiate the car
    private GameObject carPrefab;

    // Contains the script to grab the necessary information from CarDriving
    private CarDriving carScript;

    // Will contain the instantiated car
    private GameObject car;

    // Contains the score of the agent
    private int score;

    // Determines if the agent is alive or not
    private bool isAlive;

    // Returns if the agent is alive or not.
    public bool IsAlive { get { return isAlive; } }

    // Holds a paralyzed score to determine if the agent is not moving or wiggling
    private int paralyzed;

    // Additional score to determine if the agent is not moving or wiggling
    private int pastScore;

    // Getter and setter for the score of the agent
    public int Score { get { return score; } private set { score = value; } }

    // Contains the reference to the agent manager
    private AgentManager agentManager;
    #endregion

    #region Assignment Region
    // Agent receives information from AgentManager necessary to create the car and neural network
    public void Creation(NeuralNetwork brain, GameObject carPrefab, AgentManager agentManager) {
        this.agentManager = agentManager;
        this.carPrefab = carPrefab;
        this.brain = brain.DeepCopy();
        isAlive = true;
        paralyzed = 0;
        pastScore = 0;
    }
    #endregion

    #region StartUp/Update/Disable
    // Creation of this agent's car
    public void Begin() {
        car = Instantiate(carPrefab, Vector3.right, Quaternion.Euler(0, 0, -90));
        carScript = car.GetComponent<CarDriving>();
        carScript.agentScript = this;
        score = 0;

        // Subscribe to the event in CarDriving that triggers upon hitting a wall
        carScript.WallCollisionEvent += DestroyCar;

        // Begin timer for existence
        InvokeRepeating("PunishSlowCars", UnityEngine.Random.Range(200, 301) / 100f, 5f);
        InvokeRepeating("CheckEngine", UnityEngine.Random.Range(100, 200) / 100f, 2.5f);
    }

    private void FixedUpdate() {
        // Check if car exists and sensors have been initiated
        if (car == null || carScript.sensors.Length == 0)
            return;

        // Send sensor readings to this agent's neural network,
        // then take neural network outputs and apply to car
        float[] inputs = brain.NetworkProcessing(carScript.GetSensorReadings());
        carScript.SetCarInputs(inputs);

        // Car score is updated separate from Agent score and Agent only copies Car score when the car is destroyed
    }

    // Unsubscribe from events and stop running coroutines
    private void OnDisable() {
        CancelInvoke();
        carScript.WallCollisionEvent -= DestroyCar;
    }
    #endregion

    #region WaitForEvent
    public void DestroyCar() {
        // The same car can call Destroy Car multiple times
        if (!isAlive)
            return;

        score = carScript.GetScore();
        Destroy(car);
        car = null;
        isAlive = false;
        agentManager.AgentDeath();
    }
    #endregion

    #region Repeatedly Invoked Functions
    // Punish cars for existing to select faster cars
    private void PunishSlowCars() {
        if (isAlive)
            carScript.DecrementScore();
    }

    // Destroy cars that stall to speed up generations
    private void CheckEngine() {
        if (isAlive)
        {
            int currentScore = carScript.GetScore();
            if (paralyzed > 2)
                carScript.FakeCollision();
            else if (currentScore <= pastScore)
                paralyzed++;

            pastScore = currentScore;
        }
    }
    #endregion

    #region Utility Functions
    // Returns the string version of this Agent's Neural Network
    public string GetNNString() {
        return brain.ToString();
    }

    // Returns a new copy of the gameObject this Agent Script is attached to
    public Agent DeepCopy(GameObject agentPrefab) {
        GameObject NewAgent = Instantiate(agentPrefab);
        Agent NewAgentScript = NewAgent.GetComponent<Agent>();
        NewAgentScript.brain = brain.DeepCopy();
        NewAgentScript.carPrefab = carPrefab;
        NewAgentScript.score = score;
        NewAgentScript.agentManager = this.agentManager;
        return NewAgentScript;
    }

    // Return the transform of the car this Agent is tracking
    public Transform GetCarTransform() {
        return this.car.transform;
    }

    // Get the score of the car this Agent is tracking
    public int GetCarScore() {
        return this.carScript.GetScore();
    }

    // Pings event in the CarDriving script
    public void CrashCar() {
        this.carScript.FakeCollision();
    }
    #endregion
}

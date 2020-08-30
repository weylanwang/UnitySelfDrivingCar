using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    #region Variables
    private NeuralNetwork brain;
    public NeuralNetwork Brain { get { return brain; } }
    private GameObject carPrefab;
    private CarDriving carScript;
    private GameObject car;
    private int score;
    private bool isAlive;
    public bool IsAlive { get { return isAlive; } }
    private int paralyzed;
    public int Score { get { return score; } private set { score = value; } }
    private AgentManager agentManager;
    private IEnumerator existenceCoroutine;
    private IEnumerator engineCoroutine;
    #endregion

    #region Assignment Region
    // Agent receives information from AgentManager necessary to create the car and neural network
    public void Creation(NeuralNetwork brain, GameObject carPrefab, AgentManager agentManager) {
        this.agentManager = agentManager;
        this.carPrefab = carPrefab;
        this.brain = brain.DeepCopy();
        isAlive = true;
        paralyzed = 0;
        existenceCoroutine = ExistenceIsPain();
        engineCoroutine = CheckEngine();
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
        StartCoroutine(existenceCoroutine);
        StartCoroutine(engineCoroutine);
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
        StopCoroutine(existenceCoroutine);
        StopCoroutine(engineCoroutine);
        carScript.WallCollisionEvent -= DestroyCar;
    }
    #endregion

    #region WaitForEvent
    public void DestroyCar() {
        agentManager.AgentDeath();
        score = carScript.GetScore();
        Destroy(car);
        car = null;
        isAlive = false;
    }
    #endregion

    #region Coroutines
    // Punish cars for existing to select faster cars
    private IEnumerator ExistenceIsPain() {
        while (isAlive) {
            yield return new WaitForSeconds(10.0f);
            carScript.DecrementScore();
        }
    }

    // Destroy cars that stall to speed up generations
    private IEnumerator CheckEngine() {
        while (isAlive) {
            yield return new WaitForSeconds(5.0f);
            if (carScript == null) continue;
            int currentScore = carScript.GetScore();
            if (paralyzed >= currentScore)
                carScript.FakeCollision();
            else
                paralyzed = currentScore;
        }
    }
    #endregion

    #region Utility Functions
    //Returns the string version of this Agent's Neural Network
    public string GetNNString() {
        return brain.ToString();
    }

    //Returns a new copy of the gameObject this Agent Script is attached to
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
    #endregion
}

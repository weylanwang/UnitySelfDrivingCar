using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    #region Variables
    private NeuralNetwork brain;
    private GameObject carPrefab;
    private CarDriving carScript;
    private GameObject car;
    private int score;
    private AgentManager agentManager;
    #endregion

    #region Assignment Region
    //Agent receives information from AgentManager necessary to create the car and neural network
    public void Creation(uint[] sizes, GameObject carPrefab, AgentManager agentManager, Int32 seed) {
        this.agentManager = agentManager;
        this.carPrefab = carPrefab;
        brain = new NeuralNetwork(sizes, seed);
    }
    #endregion

    #region StartUp/Update/Disable
    //Creation of this agent's car
    public void Begin() {
        car = Instantiate(carPrefab, Vector3.right, Quaternion.Euler(0, 0, -90));
        carScript = car.GetComponent<CarDriving>();
        carScript.agentScript = this;
        score = 0;

        //Subscribe to the event in CarDriving that triggers upon hitting a wall
        carScript.WallCollisionEvent += DestroyCar;
    }

    private void FixedUpdate() {
        //Check if car exists and sensors have been initiated
        if (car == null || carScript.sensors.Length == 0)
            return;

        // Send sensor readings to this agent's neural network,
        // then take neural network outputs and apply to car
        float[] inputs = brain.NetworkProcessing(carScript.GetSensorReadings());
        carScript.SetCarInputs(inputs);

        // Update Agent score based on time and checkpoints
        // Send Event when score fully updated
        //float time = AgentManager.GetTimeElapsed();
    }

    private void OnDisable() {
        carScript.WallCollisionEvent -= DestroyCar;
    }
    #endregion

    #region WaitForEvent
    public void DestroyCar() {
        agentManager.AgentDeath();
        score = carScript.GetScore();
        Destroy(car);
        car = null;
    }
    #endregion

    #region Utility Functions
    public string GetNN()
    {
        return brain.ToString();
    }
    #endregion
}

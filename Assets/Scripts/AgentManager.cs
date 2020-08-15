using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    [Tooltip("The number of cars per generation")]
    [Range(1, 50)]
    private int carsPerGeneration;

    [SerializeField]
    [Tooltip("Drag in a car prefab to use for testing and training")]
    private GameObject carPrefab;

    [SerializeField]
    [Tooltip("Agent Prefab")]
    private GameObject agentPrefab;

    [SerializeField]
    [Tooltip("Input the size of each neural network layer (First layer MUST match number of sensors)")]
    private uint[] layerSizes;

    private Agent[] agentList;
    private int numAliveAgents;
    #endregion
    
    //Initialization of the Agent Manager, which oversees each agent
    private void Start() {
        //Create a random number generator
        System.Random random = new System.Random();

        //Create the list of agents
        agentList = new Agent[carsPerGeneration];

        //Instantiate agents and assign their relevant data fields
        for (int i = 0; i < carsPerGeneration; i++) {
            GameObject agent = Instantiate(agentPrefab);
            Agent agentScript = agent.GetComponent<Agent>();
            agentList[i] = agentScript;
            Int32 seed = random.Next();
            agentScript.Creation(layerSizes, carPrefab, this, seed);
            agentScript.Begin();
        }
        numAliveAgents = agentList.Length;

        //PrintNeuralNetworks("Test.txt");
    }

    public void AgentDeath() {
        numAliveAgents--;
    }

    #region Utility Functions
    public void PrintNeuralNetworks(string filename)
    {
        StreamWriter sw;

        // Check if filename is valid
        Debug.Assert(!filename.Contains("/"), "filename cannot have \"/\" character");
        if (filename.Substring(filename.Length - 4) != ".txt")
            filename += ".txt";

        string path = "Assets/Files/" + filename;

        using (sw = File.CreateText(path))
        {
            for (int i = 0; i < 5; i++)
                sw.WriteLine(agentList[i].GetNN());
        }
    }

    public void ReadNeuralNetwork(string filename)
    {
        StreamReader sr;
        string path = "Assets/Files/" + filename;

        string line = "";
        using (sr = new StreamReader(path))
        {
            while ((line = sr.ReadLine()) != null)
            {
                Debug.Log(line);
            }
        }
    }
    #endregion
}

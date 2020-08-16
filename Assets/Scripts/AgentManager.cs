using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    #region Private Variables
    [Header("Import/Export Settings")]

    [SerializeField]
    [Tooltip("Would you like to import Neural Networks from a file?")]
    private bool importNetworks = false;

    [SerializeField]
    [Tooltip("Name of file from which to import Neural Networks")]
    [TextArea(1,2)]
    private string importFile;

    [SerializeField]
    [Tooltip("Name of the file to which to export Neural Networks")]
    [TextArea(1,2)]
    private string exportFile;

    [Header("Simulation Settings")]

    [SerializeField]
    [Tooltip("The number of cars per generation")]
    [Range(3, 50)]
    private int carsPerGeneration;
    public int CarsPerGeneration { get { return carsPerGeneration; } private set { carsPerGeneration = value;  } }

    [SerializeField]
    [Tooltip("The number of cars passing their AI to the next generation")]
    [Range(1, 5)]
    private int parentsPerGeneration;

    [SerializeField]
    [Tooltip("Input the size of each neural network layer (First layer MUST match number of sensors)")]
    private uint[] layerSizes;
    public uint[] LayerSizes { get { return layerSizes; } }

    [Header("Required Prefabs")]

    [SerializeField]
    [Tooltip("Drag in a car prefab to use for testing and training")]
    private GameObject carPrefab;

    [SerializeField]
    [Tooltip("Agent Prefab")]
    private GameObject agentPrefab;

    [SerializeField]
    [Tooltip("Track Maker goes here")]
    private GameObject TrackMaker;

    private System.Random random;

    private Agent[] agentList;  
    private int numAliveAgents;
    private int leadAgent = 0;

    private Agent[] parents;
    public Agent[] Parents { get { return parents; } }
    private List<Agent> parentsList;

    private CameraTracking cameraTracker;
    private IEnumerator cameraCoroutine;
    #endregion

    #region Public Variables
    public delegate void SimulationConcluded();
    public event SimulationConcluded SimulationEndedEvent;
    #endregion

    #region StartUp
    //Initialization of the Agent Manager, which oversees each agent
    private void Start() {
        //Create a random number generator
        random = new System.Random();

        //Create the list of agents
        agentList = new Agent[carsPerGeneration];

        //Set up main camera
        cameraTracker = Camera.main.transform.GetComponent<CameraTracking>();
        cameraCoroutine = UpdateCamera();

        // Check if filename is valid
        if (exportFile.Contains("/"))
            throw new System.Exception("Export file cannot have \"/\" character");

        //Instantiate agents and assign their relevant data fields
        NeuralNetwork[] networks = new NeuralNetwork[carsPerGeneration];
        if (importNetworks) {
            int agentsCreated = ReadNeuralNetwork(networks);
            if (agentsCreated < carsPerGeneration)
                FillAgents(agentsCreated, networks);
        }
        else
            for (int i = 0; i < carsPerGeneration; i++)
                networks[i] = new NeuralNetwork(layerSizes, random.Next());

        StartCoroutine(AwaitTrackCompletion(networks));
    }
    #endregion

    #region Coroutines
    private IEnumerator GradeAgents() {
        if (agentList.Length < parentsPerGeneration)
            parentsPerGeneration = agentList.Length;

        yield return new WaitForEndOfFrame();

        parentsList = new List<Agent>(agentList);
        parentsList.Sort(CompareScore);

        parents = new Agent[parentsPerGeneration];
        for (int i = 0; i < parentsPerGeneration; i++)
            parents[i] = parentsList[i];

        yield return new WaitForEndOfFrame();

        SimulationEndedEvent();
    }

    private IEnumerator AwaitTrackCompletion(NeuralNetwork[] networks) {
        CreateTrack trackScript = TrackMaker.GetComponent<CreateTrack>();
        while (!trackScript.CreationSuccessful)
            yield return null;

        StartSimulation(networks);
    }
    #endregion

    #region Private Functions
    private void FillAgents(int importSize, NeuralNetwork[] nets) {
        for (int i = importSize; i < carsPerGeneration; i++) {
            nets[i] = new NeuralNetwork(layerSizes, random.Next());
            Debug.Log("Agent no." + (i + 1) + " created randomly to fill carsPerGeneration");
        }
    }
    #endregion

    #region Public Functions
    // Adjusts the number of living agents when an agent dies
    // Also begins sorting of Agents when 
    public void AgentDeath() {
        numAliveAgents--;

        if (numAliveAgents <= 0) {
            StartCoroutine(GradeAgents());
            StopCoroutine(cameraCoroutine);
        }
    }

    // Given the NN of the newGeneration, begins the simulation
    public void StartSimulation(NeuralNetwork[] networks) {
        for (int i = 0; i < carsPerGeneration; i++) {
            GameObject agent = Instantiate(agentPrefab);
            Agent agentScript = agent.GetComponent<Agent>();
            agentList[i] = agentScript;
            Int32 seed = random.Next();
            agentScript.Creation(networks[i], carPrefab, this);
            agentScript.Begin();
        }
        numAliveAgents = agentList.Length;
        StartCoroutine(cameraCoroutine);
    }

    // Destory all remaining agents and the cars they track
    public void DestroyAgents() {
        for (int i = 0; i < agentList.Length; i++)
            Destroy(agentList[i].gameObject);
        
        foreach (GameObject zombieCar in GameObject.FindGameObjectsWithTag("Car"))
            Destroy(zombieCar);
    }
    #endregion

    #region Utility Functions
    // Convert Neural Networks into a text file
    public void PrintNeuralNetworks() {
        if (parentsList == null || parentsList.Count == 0) {
            Debug.Log("There are no networks to print");
            return;
        }    

        string directoryPath = "Assets/Results/" + System.DateTime.Today.ToString("d").Replace("/","-");
        System.IO.Directory.CreateDirectory(directoryPath);

        if (exportFile == "")
            exportFile = System.DateTime.Now.ToString("HHmm") + ".txt";
        else if (exportFile.Substring(exportFile.Length - 4) != ".txt")
            exportFile += ".txt";

        string path = directoryPath + "/" + exportFile;
        StreamWriter sw;
        using (sw = File.CreateText(path)) {
            for (int i = 0; i < parentsList.Count; i++)
                sw.WriteLine(parentsList[i].GetNNString());
        }
        sw.Close();
        Debug.Log("Results printed to file " + directoryPath + "/" + exportFile);
    }

    // Convert a text file into Neural Networks
    private int ReadNeuralNetwork(NeuralNetwork[] nets) {
        int agentNumber = 0;
        try {
            using (StreamReader sr = new StreamReader(importFile)) {
                string line, network = "";
                while ((line = sr.ReadLine()) != null) {
                    if (line == "") {
                        NeuralNetwork tempNetwork = new NeuralNetwork(network);
                        if (!CheckLegalNetwork(tempNetwork)) {
                            Debug.Log("Imported Network no." + (agentNumber + 1) + " does not match layer sizes. Replacing with random network");
                            tempNetwork = new NeuralNetwork(layerSizes, random.Next());
                        }
                        nets[agentNumber] = tempNetwork;
                        network = "";
                        agentNumber++;
                        if (agentNumber >= carsPerGeneration) {
                            int excess = agentNumber;
                            while ((line = sr.ReadLine()) != null)
                                if (line == "")
                                    excess++;
                            excess -= agentNumber;
                            if (excess > 0)
                                Debug.Log("Number of imported networks exceeds carsPerGeneration by " + excess + ". Taking first " + carsPerGeneration + " networks only");
                            break;
                        }
                    }
                    else
                        network += line + "\n";
                }
                sr.Close();
            }
        }
        catch {
            throw new System.Exception("File could not be read or error in imported networks");
        }
        return agentNumber;
    }

    // Check if the layer sizes of the network match the layer sizes given
    private bool CheckLegalNetwork(NeuralNetwork network) {
        int interLayers = network.Layers.Length;
        int sizesLength = layerSizes.Length;

        if (interLayers != sizesLength - 1)
            return false;
        for (int i = 0; i < interLayers; i++)
            if (network.Layers[i].GetDimensions(1) != LayerSizes[i] + 1)
                return false;
        return (network.Layers[interLayers - 1].GetDimensions(0) == LayerSizes[interLayers]);
    }

    // Compare the car scores of 2 agents
    private int CompareScore(Agent a1, Agent a2) {
        return a2.Score.CompareTo(a1.Score);
    }
    #endregion

    #region Coroutines
    // Update the camera to follow the highest scoring alive car
    private IEnumerator UpdateCamera() {
        while (true)
        {
            if (!agentList[leadAgent].IsAlive)
                for (int i = 0; i < agentList.Length; i++)
                    if (agentList[i].IsAlive)
                    {
                        leadAgent = i;
                        break;
                    }

            yield return new WaitForSeconds(1f);
            for (int i = 1; i < agentList.Length; i++) {
                if (!agentList[i].IsAlive) continue;
                if (agentList[i].GetCarScore() > agentList[leadAgent].GetCarScore())
                    leadAgent = i;
            }

            if (agentList[leadAgent].IsAlive)
                cameraTracker.SetTarget(agentList[leadAgent].GetCarTransform());
        }
    }
    #endregion

    #region OnDisable
    private void OnDisable() {
        StopCoroutine(cameraCoroutine);
    }
    #endregion
}

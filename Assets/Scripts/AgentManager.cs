using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;

public class AgentManager : MonoBehaviour
{
    #region Private Variables
    [Header("Import/Export Settings")]

    [SerializeField]
    [Tooltip("Name of file from which to import Neural Networks")]
    [TextArea(1,2)]
    private string importFile;

    [SerializeField]
    [Tooltip("Name of the file to which to export Neural Networks")]
    [TextArea(1,2)]
    private string exportFile = "";
    private bool overrideExportPath = false;

    [Header("Simulation Settings")]

    [SerializeField]
    [Tooltip("The number of cars per generation")]
    [Range(3, 50)]
    private int carsPerGeneration;
    public int CarsPerGeneration { get { return carsPerGeneration; } private set { carsPerGeneration = value;  } }

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

    // Array of Agents used in this generation
    private Agent[] agentList;

    // Number of remaining agents for the generation
    private int numAliveAgents;

    // Index of Agent with the highest score of those alive
    private int leadAgent = 0;

    // Array of top parentsPerGeneration Agents in sorted order after simulation ends
    private Agent[] parents;
    public Agent[] Parents { get { return parents; } }

    // List of Agents used for sorting
    private List<Agent> parentsList;

    // Number of parents passed to Evolution Manager
    private int parentsPerGeneration = 10;

    // Variables for moving the camera
    private CameraTracking cameraTracker;
    private IEnumerator cameraCoroutine;

    [Header("UI Elements")]

    [SerializeField]
    [Tooltip("Best score text element")]
    private Text scoreText;

    [SerializeField]
    [Tooltip("Num agents alive text element")]
    private Text agentsAliveText;

    private bool canTerminateButton = false;
    #endregion

    #region Public Variables
    // Event subscription to determine if simulation has ended
    public delegate void SimulationConcluded();
    public event SimulationConcluded SimulationEndedEvent;
    #endregion

    #region StartUp
    // Initialization of the Agent Manager, which oversees each agent
    private void Start() {
        // Create a random number generator
        random = new System.Random();

        // Create the list of agents
        agentList = new Agent[carsPerGeneration];

        // Set up main camera
        cameraTracker = Camera.main.transform.GetComponent<CameraTracking>();

        // Check if filename is valid
        if (!overrideExportPath && exportFile.Contains("/"))
            throw new System.Exception("Export file cannot have \"/\" character");

        // Check if number of sensors is the same as the first layer of the layer sizes.
        if (layerSizes[0] != carPrefab.GetComponent<CarDriving>().NumSensors) {
            Debug.Log("Number of sensors on car does not match expected inputs for neural network. Change first element of layer sizes to " + carPrefab.GetComponent<CarDriving>().NumSensors); ;
            throw new System.Exception("Number of sensors does not match first layer.");
        }

        // Instantiate agents and assign their relevant data fields
        NeuralNetwork[] networks = new NeuralNetwork[carsPerGeneration];
        if (importFile != "") {
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
    // Initialization of Values
    public void Initialize(string importFile, int carsPerGeneration, int layerTemplate, GameObject carPrefab,
        GameObject agentPrefab, GameObject trackMaker, Text scoreText, Text agentsAlive)
    {
        this.importFile = importFile;
        this.carsPerGeneration = carsPerGeneration;
        this.parentsPerGeneration = 10;
        if (layerTemplate == 0)
            layerSizes = new uint[4] { 6, 4, 3, 2 };
        else if (layerTemplate == 2)
            layerSizes = new uint[4] { 10, 6, 3, 2 };
        else
            layerSizes = new uint[4] { 8, 6, 4, 2 };
        this.carPrefab = carPrefab;
        this.agentPrefab = agentPrefab;
        this.TrackMaker = trackMaker;
        this.scoreText = scoreText;
        this.agentsAliveText = agentsAlive;
        this.overrideExportPath = true;
        this.exportFile = "Assets/Resources/saveFile.txt";
    }

    // Adjusts the number of living agents when an agent dies
    // Also begins sorting of Agents when all Agents are dead
    public void AgentDeath() {
        numAliveAgents--;
        agentsAliveText.text = numAliveAgents.ToString();

        if (numAliveAgents <= 0) {
            canTerminateButton = false;
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
        agentsAliveText.text = agentList.Length.ToString();
        numAliveAgents = agentList.Length;
        cameraCoroutine = UpdateCamera();
        scoreText.text = "0";
        StartCoroutine(cameraCoroutine);
        Invoke("EnableTerminateButton", 2.0f);
    }

    // Destory all remaining agents and the cars they track
    public void DestroyAgents() {
        for (int i = 0; i < agentList.Length; i++)
            Destroy(agentList[i].gameObject);

        int zombieCounter = 0;
        foreach (GameObject zombieCar in GameObject.FindGameObjectsWithTag("Car")) {
            Destroy(zombieCar);
            zombieCounter++;
        }
        if (zombieCounter != 0) Debug.Log(zombieCounter + " zombies found!");
    }

    // End the generation by destroying any remaining cars
    public void TerminateGeneration() {
        if (!canTerminateButton) {
            Debug.Log("Can't terminate generation yet. Try again in a few seconds.");
            return;
        }

        if (agentList == null || agentList.Length == 0) {
            Debug.Log("There is no generation to terminate");
            return;
        }

        for (int i = 0; i < agentList.Length; i++)
            if (agentList[i].IsAlive)
                agentList[i].CrashCar();
    }
    #endregion

    #region Utility Functions
    // Convert Neural Networks into a text file
    public void PrintNeuralNetworks() {
        if (parentsList == null || parentsList.Count == 0) {
            Debug.Log("There are no networks to print");
            return;
        }

        string path;
        if (overrideExportPath)
            path = exportFile;
        else
        {
            string directoryPath = "Assets/Results/" + System.DateTime.Today.ToString("d").Replace("/", "-");
            System.IO.Directory.CreateDirectory(directoryPath);

            if (exportFile == "")
                exportFile = System.DateTime.Now.ToString("HHmm") + ".txt";
            else if (exportFile.Substring(exportFile.Length - 4) != ".txt")
                exportFile += ".txt";

            path = directoryPath + "/" + exportFile;
        }
        StreamWriter sw;
        using (sw = File.CreateText(path)) {
            for (int i = 0; i < parentsList.Count; i++)
                sw.WriteLine(parentsList[i].GetNNString());
        }
        sw.Close();
        //Debug.Log("Results printed to file " + directoryPath + "/" + exportFile);

        using (sw = File.CreateText("Assets/Resources/prefsFile.txt"))
        {
            sw.WriteLine(ScreenManager.GetPrefsTxt());
        }
        sw.Close();
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

    private void EnableTerminateButton() {
        canTerminateButton = true;
    }
    #endregion

    #region Coroutines
    // Update the camera to follow the highest scoring alive car
    private IEnumerator UpdateCamera() {
        while (true) {
            yield return new WaitForSeconds(0.5f);

            leadAgent = -1;
            int bestScore = -1;

            for (int i = 0; i < agentList.Length; i++) {
                if (!agentList[i].IsAlive) continue;
                if (agentList[i].GetCarScore() > bestScore) {
                    leadAgent = i;
                    bestScore = agentList[i].GetCarScore();
                }
            }

            if (leadAgent == -1) continue;
            cameraTracker.SetTarget(agentList[leadAgent].GetCarTransform());
            scoreText.text = bestScore.ToString();
        }
    }
    #endregion

    #region OnDestroy
    private void OnDestroy() {
        StopCoroutine(cameraCoroutine);
    }
    #endregion
}

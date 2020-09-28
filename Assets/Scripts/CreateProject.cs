using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class CreateProject : MonoBehaviour
{
    #region Public Variables
    // All the prefabs possibly required to initialize AgentManager and EvolutionManager
    [Header("Agent Prefab")]
    public GameObject agent;

    [Header("Track Maker Prefabs")]
    public GameObject easyTrackMaker;
    public GameObject mediumTrackMaker;
    public GameObject hardTrackMaker;

    [Header("Pre-made Track Prefabs")]
    public GameObject easyPremadeTrack;
    public GameObject mediumPremadeTrack;
    public GameObject hardPremadeTrack;
    public GameObject extremePremadeTrack;

    [Header("Car Prefabs")]
    public GameObject manualDrive;
    public GameObject basicCar;
    public GameObject basicReduced;
    public GameObject basicMinimal;
    public GameObject basicExtended;
    public GameObject basic360;
    public GameObject advanced360;

    [Header("UI Elements")]
    public Text scoreText;
    public Text agentsAlive;
    public Text generationText;
    #endregion

    #region Private Variables
    // CreateProject creates a TrackMaker and a Management object
    // The TrackMaker has the CreateTrack component
    private GameObject trackMaker;

    // The Management has both the AgentManager and the EvolutionManager
    private GameObject management;
    private AgentManager agentManager;
    private EvolutionManager evolutionManager;

    // The file paths for the files used
    // The saveFile stores a generation for later use
    private string saveFile = "Assets/Resources/saveFile.txt";

    // The demoFile stores a pre-trained car for demo purposes
    private string demoFile = "Assets/Resources/demo.txt";

    // The prefsFile stores the PlayerPrefs of the generation stored in saveFile
    private string prefsFile = "Assets/Resources/prefsFile.txt";
    #endregion

    #region StartUp
    // Start is called before the first frame update
    void Start()
    {
        // Set enabled to false to prevent Start() from running until we re-enable it
        // onDisable() and onEnable will be called
        management = new GameObject { name = "Management" };
        agentManager = management.AddComponent<AgentManager>();
        agentManager.enabled = false;
        evolutionManager = management.AddComponent<EvolutionManager>();
        evolutionManager.enabled = false;

        // If the user selected Demo, use default settings and pre-trained car
        if (PlayerPrefs.GetString("Demo") == "true")
        {
            trackMaker = Instantiate(extremePremadeTrack);
            trackMaker.SetActive(false);

            agentManager.Initialize(demoFile, 30, 2, advanced360, agent, trackMaker, scoreText, agentsAlive);
            evolutionManager.Initialize(30, 0.1f, 0.75f, 1f, "BestTwo", generationText);
            StartCoroutine(DelayedStart());
            return;
        }

        string importFile = "";

        // If the user selected Import, import the generation from the saveFile and the PlayerPrefs from the prefsFile
        // Only difficulty and track type can be changed if importing
        if (PlayerPrefs.GetString("NetworkSource") == "Import")
        {
            importFile = saveFile;
            if (!File.Exists(saveFile))
                File.Create(saveFile).Close();
            ImportPrefs();
        }

        int carCount;
        if (PlayerPrefs.GetString("DriveType") == "ManualDriving")
            carCount = 2;
        else
            carCount = System.Convert.ToInt32(PlayerPrefs.GetString("carCount"));

        // Layers/Hidden layers are hard coded in
        // 6 input cars have 6, 4, 3, 2
        // 8 input cars have 8, 6, 4, 2
        // 10 input cars have 10, 6, 3, 2
        int layerType;
        if (PlayerPrefs.GetString("DriveType") == "ManualDriving")
            layerType = 0;
        else if (PlayerPrefs.GetString("CarSelection") == "360Advanced")
            layerType = 2;
        else if (PlayerPrefs.GetString("CarSelection") == "360Basic")
            layerType = 1;
        else
            layerType = 0;

        // Initialize both the AgentManager and EvolutionManager
        trackMaker = Instantiate(ChooseTrack());
        trackMaker.SetActive(false);
        agentManager.Initialize(importFile, carCount, layerType, ChooseCar(), agent, trackMaker, scoreText, agentsAlive);

        float recombinationChance = (float)System.Convert.ToDouble(PlayerPrefs.GetString("recombinationChance"));
        float mutationChance = (float)System.Convert.ToDouble(PlayerPrefs.GetString("mutationChance"));
        float mutationAmount = (float)System.Convert.ToDouble(PlayerPrefs.GetString("mutationAmount"));
        string algorithm = PlayerPrefs.GetString("Algorithm");
        uint genCount = 30;
        if (PlayerPrefs.GetString("DriveType") == "ManualDriving")
            genCount = 2000;
        evolutionManager.Initialize(genCount, recombinationChance, mutationChance, mutationAmount, algorithm, generationText);

        StartCoroutine(DelayedStart());
    }
    #endregion

    #region Public Functions
    // Ends the current generation
    public void TerminateGeneration()
    {
        agentManager.TerminateGeneration();
    }

    // Saves the previous generation
    public void Save()
    {
        agentManager.PrintNeuralNetworks();
    }    

    // Return to the main menu
    // Does not save a generation before returning
    public void Quit()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Private Functions
    // Start the gameObjects on the same frame
    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        trackMaker.SetActive(true);
        agentManager.enabled = true;
        evolutionManager.enabled = true;
    }

    // Return the correct car prefab
    private GameObject ChooseCar()
    {
        if (PlayerPrefs.GetString("DriveType") == "ManualDriving")
            return manualDrive;

        switch(PlayerPrefs.GetString("CarSelection"))
        {
            case "360Advanced": return advanced360;
            case "360Basic": return basic360;
            case "BasicCar": return basicCar;
            case "BasicExtended": return basicExtended;
            case "BasicMinimal": return basicMinimal;
            case "BasicReduced": return basicReduced;
            default: return manualDrive;
        }
    }

    // Return the correct TrackMaker prefab
    private GameObject ChooseTrack()
    {
        string trackType = PlayerPrefs.GetString("TrackType");
        string difficulty = PlayerPrefs.GetString("Difficulty");

        if (trackType == "Randomly Created")
        {
            if (difficulty == "Hard")
                return hardTrackMaker;
            else if (difficulty == "Easy")
                return easyTrackMaker;
            else if (difficulty == "Medium")
                return mediumTrackMaker;
        }
        else if (trackType == "Standard")
        {
            if (difficulty == "Hard")
                return hardPremadeTrack;
            else if (difficulty == "Easy")
                return easyPremadeTrack;
            else if (difficulty == "Medium")
                return mediumPremadeTrack;
            else if (difficulty == "Extreme")
                return extremePremadeTrack;
        }
        else if (trackType == "")
            return extremePremadeTrack;

        Debug.Log("Error: Chosen Track unknown. TrackType is " + trackType + ". Difficulty is " + difficulty);
        return null;
    }

    // Import PlayerPrefs from prefsFile
    // Only difficulty and Track Type can be changed
    private void ImportPrefs()
    {
        string difficulty = PlayerPrefs.GetString("Difficulty");
        string trackType = PlayerPrefs.GetString("TrackType");
        ScreenManager.ResetPrefsToDefault();

        try
        {
            using (StreamReader sr = new StreamReader(prefsFile))
            {
                string line;
                string[] prefsArray;
                while ((line = sr.ReadLine()) != null)
                {
                    prefsArray = line.Split(' ');
                    if (PlayerPrefs.HasKey(prefsArray[0]))
                        PlayerPrefs.SetString(prefsArray[0], prefsArray[1]);
                }
                sr.Close();
            }
        }
        catch
        {
            // If a prefsFile does not exist, create one
            WritePrefs();
        }

        PlayerPrefs.SetString("Difficulty", difficulty);
        PlayerPrefs.SetString("TrackType", trackType);
    }

    // Store Player Prefs in prefsFile
    private void WritePrefs()
    {
        StreamWriter sw;
        using (sw = File.CreateText(prefsFile))
        {
            sw.WriteLine(ScreenManager.GetPrefsTxt());
        }
        sw.Close();
    }
    #endregion
}

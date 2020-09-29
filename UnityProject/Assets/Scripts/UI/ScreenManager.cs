using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenManager : MonoBehaviour
{
    #region Private Variables
    private static ScreenManager instance;
    public static ScreenManager Instance { get { return instance; } }

    private Dictionary<string, List<string>> ScreenMap;
    private bool dictionaryReady = false;

    private Dictionary<string, string> prefs;

    private LinkedList<string> screenList;

    private static List<string> allCategories = new List<string> { "NetworkSource", "DriveType", "CarSelection", "Difficulty", "TrackType", "Algorithm",
            "carCount", "recombinationChance", "mutationChance", "mutationAmount" };
    #endregion

    // Delete this instance if an instance already exists
    private void Awake() {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        ButtonTransition.ScreensMappedEvent += CreateDictionary;
    }

    public void CreateDictionary()  {
        ScreenMap = ButtonTransition.GetScreenMap();
        prefs = new Dictionary<string, string>();
        screenList = new LinkedList<string>(ButtonTransition.GetScreenList());

        dictionaryReady = true;
    }

    public void Back(string currentScreen)
    {
        string driveType = "";
        if (prefs.ContainsKey("DriveType"))
            driveType = prefs["DriveType"];

        if (currentScreen == "Difficulty" && prefs["NetworkSource"] == "Import")
            ButtonTransition.DisplayButtons("NetworkSource", false);
        else if (currentScreen == "Difficulty" && driveType == "ManualDriving")
            ButtonTransition.DisplayButtons("DriveType", false);
        else if (currentScreen == "Algorithm" && driveType == "AIDriven" && prefs["Difficulty"] == "Extreme")
            ButtonTransition.DisplayButtons("Difficulty", false);
        else if (currentScreen == "Start") {
            if (driveType == "Demo")
                ButtonTransition.DisplayButtons("DriveType", false);
            else if (driveType == "ManualDriving")  {
                if (prefs["Difficulty"] == "Extreme")
                    ButtonTransition.DisplayButtons("Difficulty", false);
                else
                    ButtonTransition.DisplayButtons("TrackType", false);
            }
            else
                ButtonTransition.DisplayButtons(screenList.Find(currentScreen).Previous.Value, false);
        }
        else
            ButtonTransition.DisplayButtons(screenList.Find(currentScreen).Previous.Value, false);
    }

    public void Next(string currentScreen) {
        ButtonTransition.DisplayButtons(screenList.Find(currentScreen).Next.Value);
    }

    public void ButtonPress(string buttonName) {
        string screenName;

        if (ButtonTransition.Transition)
            // Dont' transition to another screen if the buttons are still moving
            return;
        else if (!FindScreen(buttonName, out screenName)) {
            Debug.Log("Button: " + buttonName + " not recognized");
            return;
        }

        prefs[screenName] = buttonName;

        if (buttonName == "Demo")
            ButtonTransition.DisplayButtons("Start");
        else if (buttonName == "Import") {
            prefs["DriveType"] = "AIDriven";
            ButtonTransition.DisplayButtons("Difficulty");
        }
        else if (buttonName == "ManualDriving")
            ButtonTransition.DisplayButtons("Difficulty");
        else if (buttonName == "Extreme") {
            if (prefs["DriveType"] == "ManualDriving")
                ButtonTransition.DisplayButtons("Start");
            else if (prefs["DriveType"] == "AIDriven")
                ButtonTransition.DisplayButtons("Algorithm");
        }
        else if (screenName == "TrackType" && prefs["DriveType"] == "ManualDriving")
            ButtonTransition.DisplayButtons("Start");
        else
            ButtonTransition.DisplayButtons(screenList.Find(screenName).Next.Value);
    }

    public void Begin() {
        // If a category is missing, add it
        foreach (string category in allCategories)
            if (!prefs.ContainsKey(category))
                prefs.Add(category, GetDefault(category));

        // Convert all saved prefs to PlayerPrefs
        foreach (KeyValuePair<string, string> pair in prefs)
            PlayerPrefs.SetString(pair.Key, pair.Value);

        CallScene();
    }

    // Updates Slider Values. Values not yet truncated
    public void SliderUpdate(Slider slider) {
        string[] wordArray = slider.name.Split(' ');
        int sliderNumber = System.Convert.ToInt32(wordArray[wordArray.Length - 1]);

        string sliderValue = slider.value.ToString();

        switch(sliderNumber)
        {
            case 1: prefs["carCount"] = sliderValue; break;
            case 2: prefs["recombinationChance"] = sliderValue; break;
            case 3: prefs["mutationChance"] = sliderValue; break;
            case 4: prefs["mutationAmount"] = sliderValue; break;
            default: Debug.Log("Slider number " + sliderNumber + " not set to preference key"); break;
        }
    }

    public static void ResetPrefsToDefault() {
        foreach (string category in allCategories)
            PlayerPrefs.SetString(category, GetDefault(category));
    }

    public static string GetPrefsTxt() {
        string txt = "DriveType AIDriven" + '\n';
        txt += "CarSelection " + PlayerPrefs.GetString("CarSelection") + '\n';
        return txt;
    }

    private bool FindScreen(string buttonName, out string screen) {
        if (!dictionaryReady) {
            screen = "";
            Debug.Log("Dictionary not ready");
            return false;
        }

        foreach (KeyValuePair<string, List<string>> pair in ScreenMap)
            if (pair.Value.Contains(buttonName)) {
                screen = pair.Key;
                return true;
            }

        screen = "";
        return false;
    }

    private static string GetDefault(string category) {
        switch(category)
        {
            case "NetworkSource": return "Restart";
            case "DriveType": return "Demo";
            case "CarSelection": return "360Advanced";
            case "Difficulty": return "Extreme";
            case "TrackType": return "Standard";
            case "Algorithm": return "BestTwo";
            case "carCount": return "30";
            case "recombinationChance": return "0.1";
            case "mutationChance": return "0.75";
            case "mutationAmount": return "3";
            default:
                Debug.Log("Category " + category + " not found in defaults");
                return "";
        }
    }

    private void CallScene() {
        SceneManager.LoadScene(1);
    }
}

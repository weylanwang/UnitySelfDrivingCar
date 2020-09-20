using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ButtonTransition : MonoBehaviour
{
    #region Private Variables
    private static ButtonTransition instance;
    public static ButtonTransition Instance { get { return instance; } }

    // Dictionary of Screens
    private static Dictionary<string, RectTransform> ScreenDictionary;

    // List of keys for dictionary
    private static List<string> ScreenList;
    public static List<string> GetScreenList() { return ScreenList; }

    // Button Screens Displayed
    private static string currentScreen;
    private static string previousScreen;
    private static float timer;

    // Currently in transition?
    private static bool transition;
    public static bool Transition { get { return transition; } }
    #endregion

    #region Public Variables
    // Time duration of a screen transition
    public static float transitionTime = 0.5f;
    public static string startingScreen = "NetworkSource";

    public delegate void ScreensMapped();
    public static event ScreensMapped ScreensMappedEvent;
    #endregion

    #region Awake/Start/Update
    // Delete this instance if an instance already exists
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else instance = this;
    }

    private void Start()
    {
        ScreenDictionary = new Dictionary<string, RectTransform>();
        ScreenList = new List<string>();
        currentScreen = startingScreen;
        transition = false;

        // Add Screen name and transform to Dictionary as a pair
        foreach (Transform child in transform)
        {
            ScreenDictionary.Add(child.name, (RectTransform)child);
            ScreenList.Add(child.name);
            foreach (Transform trans in child)
                trans.gameObject.SetActive(true);
            child.gameObject.SetActive(child.name == currentScreen);
        }

        ScreensMappedEvent();

        // Begin Coroutine for Testing
        //StartCoroutine(ScreenTest());
    }

    private void Update()
    {
        float screenLength = Screen.width;
        if (transition)
        {
            if (Time.realtimeSinceStartup > timer)
            {
                SetTransitionState();
                timer = 0;
                return;
            }
            else
            {
                ScreenDictionary[previousScreen].localPosition += new Vector3(screenLength / transitionTime * Time.deltaTime, 0, 0);
                ScreenDictionary[currentScreen].localPosition += new Vector3(screenLength / transitionTime * Time.deltaTime, 0, 0);
            }
        }
    }
    #endregion

    #region Public Functions
    // Begin Button Transition
    public static void DisplayButtons(string screen)
    {
        // Throw error if key not found
        // Return if the button screen is already displayed
        if (!ScreenDictionary.ContainsKey(screen))
            throw new System.Exception(screen + " screen not found in dictionary");
        else if (currentScreen == screen)
            return;
        else if (transition)
        {
            Debug.Log("Still finishing past transition. Can't transition now");
            return;
        }

        // Begin the movement of buttons
        previousScreen = currentScreen;
        currentScreen = screen;
        transition = true;
        ScreenDictionary[currentScreen].localPosition = new Vector3(-Screen.width, 0f, 0f);
        ScreenDictionary[currentScreen].gameObject.SetActive(true);
        timer = Time.realtimeSinceStartup + transitionTime;
    }

    // Return a dictionary matching screen names with the names of the buttons in the list
    public static Dictionary<string, List<string>> GetScreenMap()
    {
        Dictionary<string, List<string>> returnDictionary = new Dictionary<string, List<string>>();
        foreach (KeyValuePair<string, RectTransform> pair in ScreenDictionary)
        {
            List<string> buttonsInScreen = new List<string>();
            foreach (Transform trans in pair.Value)
                buttonsInScreen.Add(trans.name);
            returnDictionary.Add(pair.Key, buttonsInScreen);
        }

        return returnDictionary;
    }
    #endregion

    #region Private Functions
    // Terminate Button Transition
    private void SetTransitionState()
    {
        // Terminate the movement of Buttons and disable any buttons not currently shown
        RectTransform previous = ScreenDictionary[previousScreen];
        RectTransform current = ScreenDictionary[currentScreen];
        previous.gameObject.SetActive(false);
        previous.localPosition = Vector3.zero;
        current.localPosition = Vector3.zero;
        current.gameObject.SetActive(true);
        transition = false;
    }

    private IEnumerator ScreenTest()
    {
        yield return new WaitForSeconds(1f);
        int selector = 0;
        while(true)
        {
            string tempKey = ScreenList[selector++ % ScreenList.Count];
            if (tempKey == currentScreen)
                continue;

            DisplayButtons(tempKey);
            yield return new WaitForSeconds(4f);
        }
    }
    #endregion
}

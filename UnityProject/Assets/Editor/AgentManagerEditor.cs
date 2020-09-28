using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AgentManager))]
public class AgentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AgentManager agentManager = (AgentManager)target;

        if (GUILayout.Button("Print Networks")) {
            agentManager.PrintNeuralNetworks();
        }

        if (GUILayout.Button("Terminate Generation")) {
            agentManager.TerminateGeneration();
        }

        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}

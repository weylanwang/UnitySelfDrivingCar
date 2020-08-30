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

        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}

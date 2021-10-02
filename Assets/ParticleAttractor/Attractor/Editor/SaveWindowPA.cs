using System;
using UnityEditor;
using UnityEngine;

public class SaveWindowPA : EditorWindow
{
    private static IScenarioHandler _scenarioHandler;
    private static object _scenarioData = null;

    private const int WindowHeight = 100;
    private const int WindowWidth = 400;
    
    string _savedFileName = String.Empty;
    public static void Init()
    {
        SaveWindowPA window = (SaveWindowPA)EditorWindow.GetWindow(typeof(SaveWindowPA));
        _scenarioHandler = new NewtonsoftScenarioHandler();

        int x = (Screen.currentResolution.width - SaveWindowPA.WindowWidth) / 2;
        int y = (Screen.currentResolution.height - SaveWindowPA.WindowHeight) / 2;
        window.position = new Rect(x, y, 
            SaveWindowPA.WindowWidth, 
            SaveWindowPA.WindowHeight);
        window.Show();
        
    }

    public static void SetScenarioData(object scenarioData)
    {
        _scenarioData = scenarioData;
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            _savedFileName = EditorGUILayout.TextField("File Name", _savedFileName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button( "Save" ))
            {
                _scenarioHandler.Save(_savedFileName, _scenarioData);
                Close();
            }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}

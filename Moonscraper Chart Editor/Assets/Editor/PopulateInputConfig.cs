using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputConfigBuilder))]
public class PopulateInputConfig : Editor
{
    SerializedProperty _inputProperties;
    MSChartEditorInputActions _newActionAdded;

    static bool s_shouldReset = false;
    string inputPropertiesPath = "Assets/Database/InputPropertiesConfig.json";

    void OnEnable()
    {
        _inputProperties = serializedObject.FindProperty("inputProperties");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_inputProperties, true);
        serializedObject.ApplyModifiedProperties();

        InputConfigBuilder.InputProperties inputProperties = ((InputConfigBuilder)target).inputProperties;

        if (s_shouldReset)
        {
            inputProperties.shortcutInputs = new ShortcutInputConfig[0];
            s_shouldReset = false;
        }

        if (GUILayout.Button("Load Config From File"))
        {
            string filename = inputPropertiesPath;
            //if (FileExplorer.OpenFilePanel(new ExtensionFilter("Config files", "json"), "json", out filename))
            {
                InputConfig inputConfig = new InputConfig();
                InputConfig.LoadFromFile(filename, inputConfig);
                inputProperties.shortcutInputs = inputConfig.shortcutInputs;
            }
        }
        if (GUILayout.Button("Save Config To File")) 
        {
            string filename = inputPropertiesPath;

            if (inputProperties.shortcutInputs.Length > 0)
            {
                //if (FileExplorer.SaveFilePanel(new ExtensionFilter("Config files", "json"), "InputPropertiesConfig", "json", out filename))
                {
                    InputConfig inputConfig = new InputConfig();
                    inputConfig.shortcutInputs = inputProperties.shortcutInputs;
                    InputConfig.Save(inputConfig, filename);

                    Debug.Log("Input properties config saved successfully");
                }
            }
            else
            {
                Debug.LogError("Trying to save empty input properties. This is not allowed.");
            }
        }      
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void Reset()
    {
        s_shouldReset = true;
    }
}

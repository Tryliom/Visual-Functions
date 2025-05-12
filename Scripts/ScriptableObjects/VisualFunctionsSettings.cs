using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [Serializable]
    public class Settings
    {
        public string PathToGlobalVariables = "Assets/Resources/ScriptableObjects/GlobalVariables";
        public string PathToVariables = "Assets/Resources/ScriptableObjects/Variables";

        public string GlobalValuesPrefix = "_";
    }
    
    [CreateAssetMenu(fileName = "VisualFunctionsSettings", menuName = "VisualFunctions/Settings")]
    public class VisualFunctionsSettings : ScriptableObject
    {
        public Settings Settings = new();
        
        private void OnValidate()
        {
            VisualFunctionsInitializer.LoadSettings();
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Settings))]
    public class SettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var fieldWidth = position.width - 25;
            const int buttonWidth = 20;
            
            var globalPathProperty = property.FindPropertyRelative("PathToGlobalVariables");
            var globalPathRect = new Rect(position.x, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            
            globalPathProperty.stringValue = EditorGUI.TextField(globalPathRect, "Path To Global Variables", globalPathProperty.stringValue);
            
            var globalButtonRect = new Rect(position.x + fieldWidth + 5, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            
            if (GUI.Button(globalButtonRect, "..."))
            {
                ShowFolderPicker(globalPathProperty, "Select Global Variables Folder");
            }
            
            var variablesPathProperty = property.FindPropertyRelative("PathToVariables");
            var variablesPathRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, fieldWidth, EditorGUIUtility.singleLineHeight);
            
            variablesPathProperty.stringValue = EditorGUI.TextField(variablesPathRect, "Path To Variables", variablesPathProperty.stringValue);
            
            var variablesButtonRect = new Rect(position.x + fieldWidth + 5, position.y + EditorGUIUtility.singleLineHeight + 2, buttonWidth, EditorGUIUtility.singleLineHeight);
            
            if (GUI.Button(variablesButtonRect, "..."))
            {
                ShowFolderPicker(variablesPathProperty, "Select Variables Folder");
            }
            
            var prefixProperty = property.FindPropertyRelative("GlobalValuesPrefix");
            var prefixRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, fieldWidth, EditorGUIUtility.singleLineHeight);
            
            prefixProperty.stringValue = EditorGUI.TextField(prefixRect, "Global Values Prefix", prefixProperty.stringValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 4;
        }
        
        private static void ShowFolderPicker(SerializedProperty property, string title)
        {
            var selectedPath = EditorUtility.OpenFolderPanel(title, property.stringValue, "");

            if (string.IsNullOrEmpty(selectedPath)) return;
            
            var index = selectedPath.IndexOf("Assets/Resources/", StringComparison.Ordinal);
                
            if (index >= 0)
            {
                selectedPath = selectedPath[index..];
            }
                
            property.stringValue = selectedPath;
        }
    }
#endif
}
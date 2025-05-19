using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [Serializable]
    public class AssetPath
    {
        public string Path;
        
        public AssetPath(string path)
        {
            Path = path;
        }
        
        public static implicit operator string(AssetPath assetPath)
        {
            return assetPath.Path;
        }
    }
    
    [Serializable]
    public class Settings
    {
        public AssetPath PathToGlobalVariables = new ("Assets/Resources/ScriptableObjects/GlobalVariables");
        public AssetPath PathToVariables = new ("Assets/Resources/ScriptableObjects/Variables");
        public AssetPath PathToGlobalObjects = new ("Assets/Resources/ScriptableObjects/GlobalObjects");

        public string GlobalValuesPrefix = "_";
    }
    
    [CreateAssetMenu(fileName = "VisualFunctionsSettings", menuName = "Visual Functions/Settings")]
    public class VisualFunctionsSettings : ScriptableObject
    {
        public Settings Settings = new();
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            VisualFunctionsInitializer.LoadSettings();
        }
#endif
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AssetPath))]
    public class AssetPathDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var fieldWidth = position.width - 25;
            const int buttonWidth = 20;
            
            var pathProperty = property.FindPropertyRelative("Path");
            var pathRect = new Rect(position.x, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            
            pathProperty.stringValue = EditorGUI.TextField(pathRect, label.text, pathProperty.stringValue);
            
            var buttonRect = new Rect(position.x + fieldWidth + 5, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            
            if (GUI.Button(buttonRect, "..."))
            {
                ShowFolderPicker(pathProperty, "Select Path");
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
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
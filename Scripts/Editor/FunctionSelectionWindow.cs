using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace TryliomFunctions
{
    public class FunctionSelectionWindow : EditorWindow
    {
        private string _searchQuery = "";
        private Action<Function> _onFunctionSelected;
        private Vector2 _scrollPosition;

        public static void ShowWindow(Action<Function> onFunctionSelected)
        {
            var window = GetWindow<FunctionSelectionWindow>("Select Function");
            window._onFunctionSelected = onFunctionSelected;
            window.Show();
        }

        private void OnGUI()
        {
            // Search bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUI.SetNextControlName("SearchTextField");
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            EditorGUI.FocusTextInControl("SearchTextField");
            
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = "";
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Function list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var category in Function.Functions.GroupBy(f => f.Value.Category))
            {
                var centeredStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(category.Key.ToString()), centeredStyle);
                
                // Filter functions by search query
                var categories = category.Where(f =>
                    string.IsNullOrEmpty(_searchQuery) || 
                    f.Key.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Value.Description.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
                );

                foreach (var function in categories)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(function.Key, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(function.Value.Description, EditorStyles.wordWrappedLabel);

                    if (GUILayout.Button("Use", EditorStyles.miniButton))
                    {
                        _onFunctionSelected?.Invoke(Activator.CreateInstance(function.Value.Type) as Function);
                        Close();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
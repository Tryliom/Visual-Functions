using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TryliomFunctions
{
    public class MethodInfos
    {
        public readonly string Description;
        public readonly string Name;
        public readonly string[] Parameters;
        public readonly string ReturnType;

        public MethodInfos(string name, string description, string returnType, string[] parameters)
        {
            Name = name;
            Description = description;
            ReturnType = returnType;
            Parameters = parameters;
        }
    }

    public class PropertyInfos
    {
        public readonly string Description;
        public readonly string Name;
        public readonly string Type;

        public PropertyInfos(string name, string description, string type)
        {
            Name = name;
            Description = description;
            Type = type;
        }
    }

    public class MethodSearchWindow : EditorWindow
    {
        private List<MethodInfos> _methods;
        private List<PropertyInfos> _properties;
        private Vector2 _scrollPosition;
        private string _searchText = "";

        private void OnGUI()
        {
            GUI.SetNextControlName("SearchTextField");
            _searchText = EditorGUILayout.TextField("Search", _searchText);
            EditorGUI.FocusTextInControl("SearchTextField");

            if (_methods == null || _properties == null || (_methods.Count == 0 && _properties.Count == 0))
            {
                EditorGUILayout.LabelField("No methods or properties found.");
                return;
            }

            var filteredMethods = _methods
                .Where(m => m.Name.ToLower().Contains(_searchText.ToLower()) ||
                            m.Description.ToLower().Contains(_searchText.ToLower()))
                .ToList();
            var filteredProperties = _properties
                .Where(p => p.Name.ToLower().Contains(_searchText.ToLower()) ||
                            p.Description.ToLower().Contains(_searchText.ToLower()))
                .ToList();

            if (filteredMethods.Count == 0 && filteredProperties.Count == 0)
            {
                EditorGUILayout.LabelField("No methods or properties found.");
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                foreach (var method in filteredMethods)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField($"{method.Name}({string.Join(", ", method.Parameters)})", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(method.ReturnType, EditorStyles.boldLabel, GUILayout.Width(100));

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard"), GUILayout.Width(60)))
                    {
                        EditorGUIUtility.systemCopyBuffer = method.Name;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (method.Description != string.Empty)
                    {
                        EditorGUILayout.LabelField(method.Description, EditorStyles.miniLabel);
                    }

                    EditorGUILayout.Space();
                }

                foreach (var property in filteredProperties)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(property.Name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(property.Type, EditorStyles.boldLabel, GUILayout.Width(100));

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard"), GUILayout.Width(60)))
                    {
                        EditorGUIUtility.systemCopyBuffer = property.Name;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (property.Description != string.Empty)
                    {
                        EditorGUILayout.LabelField(property.Description, EditorStyles.miniLabel);
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        /**
         * Show methods and properties in the search window from all possible types
         */
        public static void ShowWindow(bool displayVoid)
        {
            var window = GetWindow<MethodSearchWindow>("Search Methods");
            window._methods = new List<MethodInfos>();
            window._properties = new List<PropertyInfos>();

            var constantTypes = ReferenceUtility.GetAllConstantTypes();
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsPublic && !type.IsNested && !type.Name.StartsWith("_") && !type.IsAbstract &&
                               type.IsVisible && !type.Name.Contains("`1") &&
                               (type.IsEnum || constantTypes.Contains(type) || type.Namespace == "UnityEngine"))
                .OrderBy(t => t.IsGenericType)
                .ThenBy(t => t.Namespace == "UnityEngine")
                .ThenBy(t => t.Name)
                .ToList();

            allTypes.ForEach(type => Fill(window, type, displayVoid));

            window.Show();
        }

        /**
         * Show methods and properties in the search window from a specific type
         */
        public static void ShowWindow(Type type, bool displayVoid)
        {
            var window = GetWindow<MethodSearchWindow>("Search Methods");
            window._methods = new List<MethodInfos>();
            window._properties = new List<PropertyInfos>();

            Fill(window, type, displayVoid);

            // Reorder the methods and properties
            window._methods = window._methods
                .OrderBy(m => m.Name)
                .ThenBy(m => m.ReturnType)
                .ToList();
            window._properties = window._properties
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Type)
                .ToList();

            window.Show();
        }

        private static void Fill(MethodSearchWindow self, Type type, bool displayVoid)
        {
            var constantTypes = ReferenceUtility.GetAllConstantTypes();
            var bannedMethods = new List<string>
            {
                "GetType", "GetHashCode", "ToString", "Equals", "Finalize", "MemberwiseClone",
                "GetTypeCode", "Compare", "CompareTo", "GetEnumerator", "GetObjectData", "GetMethod", "GetField"
            };
            var methods = type.GetMethods();

            foreach (var method in methods)
            {
                if (bannedMethods.Contains(method.Name)) continue;
                if (method.Name.Contains("`1")) continue;
                if (method.IsSpecialName) continue;
                if (method.IsPrivate) continue;
                if (!displayVoid && method.ReturnType == typeof(void)) continue;
                if (!constantTypes.Contains(method.ReturnType) && !method.ReturnType.IsEnum) continue;
                if (method.GetParameters()
                    .Any(p => !constantTypes.Contains(p.ParameterType) && !p.ParameterType.IsEnum)) continue;

                var description = method.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? "";
                var name = method.Name;

                if (method.IsStatic)
                    name = $"static {RenameTypes(type.Name)}.{name}";
                else
                    name = $"{RenameTypes(type.Name)} {name}";

                self._methods.Add(new MethodInfos(
                    name,
                    description,
                    RenameTypes(method.ReturnType.Name),
                    method.GetParameters().Select(p => RenameTypes(p.ParameterType.Name) + " " + p.Name).ToArray()
                ));
            }

            var properties = type.GetProperties().ToList();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.Name.Contains("`1")) continue;
                if (propertyInfo.IsSpecialName) continue;
                if (!propertyInfo.CanRead) continue;
                if (!constantTypes.Contains(propertyInfo.PropertyType)) continue;

                var description = propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? "";
                var name = RenameTypes(type.Name) + "." + propertyInfo.Name;

                if (propertyInfo.GetMethod.IsStatic) name = "static " + name;

                self._properties.Add(new PropertyInfos(
                    name,
                    description,
                    RenameTypes(propertyInfo.PropertyType.Name)
                ));
            }

            var fields = type.GetFields().ToList();

            foreach (var info in fields)
            {
                if (info.Name.Contains("`1")) continue;
                if (info.IsSpecialName) continue;
                if (info.IsPrivate) continue;
                if (!constantTypes.Contains(info.FieldType)) continue;

                var description = info.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? "";
                var name = RenameTypes(type.Name) + "." + info.Name;

                if (info.IsStatic) name = "static " + name;

                self._properties.Add(new PropertyInfos(
                    name,
                    description,
                    RenameTypes(info.FieldType.Name)
                ));
            }
        }

        private static string RenameTypes(string str)
        {
            return str
                .Replace("Int32", "Int")
                .Replace("Single", "Float");
        }
    }
}
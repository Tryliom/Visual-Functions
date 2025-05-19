using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;

namespace VisualFunctions
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
        private string _searchText = "";
        private float _searchTimer;
        private ScrollView _resultsContainer;
        
        private void CreateGUI()
        {
            var root = rootVisualElement;
            var searchField = new TextField("Search");
            
            searchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue;
                StartSearchTimer();
            });
            
            root.Add(searchField);
            
            _resultsContainer = new ScrollView()
            {
                style =
                {
                    marginTop = 5
                }
            };
            
            _resultsContainer.Add(new Label("Loading... This can take a while if you have a lot of types.."));
            
            root.Add(_resultsContainer);
        }
        
        private void StartSearchTimer()
        {
            EditorApplication.update -= OnSearchTimerElapsed;
            _searchTimer = Time.realtimeSinceStartup;
            EditorApplication.update += OnSearchTimerElapsed;
        }
            
        private void OnSearchTimerElapsed()
        {
            if (Time.realtimeSinceStartup - _searchTimer < 0.5f) return;
            
            EditorApplication.update -= OnSearchTimerElapsed;
            UpdateResults();
        }

        private void UpdateResults()
        {
            var container = new VisualElement();
            var searchLower = _searchText.ToLower();
            var filteredMethods = _methods;
            var filteredProperties = _properties;
            
            _resultsContainer.Clear();

            if (!string.IsNullOrEmpty(_searchText))
            {
                filteredMethods = _methods
                    .Where(m => m.Name.ToLower().Contains(searchLower) ||
                                m.Description.ToLower().Contains(searchLower))
                    .ToList();
                filteredProperties = _properties
                    .Where(p => p.Name.ToLower().Contains(searchLower) ||
                                p.Description.ToLower().Contains(searchLower))
                    .ToList();
            }

            if (filteredMethods.Count == 0 && filteredProperties.Count == 0)
            {
                container.Add(new Label("No methods or properties found."));
                return;
            }

            foreach (var method in filteredMethods)
            {
                var methodRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                methodRow.Add(new Label($"{method.Name}({string.Join(", ", method.Parameters)})") { style = { flexGrow = 1 } });
                methodRow.Add(new Label(method.ReturnType) { style = { width = 100 } });

                var copyButton = new Button(() => EditorGUIUtility.systemCopyBuffer = method.Name) { text = "Copy" };
                methodRow.Add(copyButton);

                container.Add(methodRow);

                if (!string.IsNullOrEmpty(method.Description))
                {
                    container.Add(new Label(method.Description) { style = { unityFontStyleAndWeight = FontStyle.Italic } });
                }
            }

            foreach (var property in filteredProperties)
            {
                var propertyRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                propertyRow.Add(new Label(property.Name) { style = { flexGrow = 1 } });
                propertyRow.Add(new Label(property.Type) { style = { width = 100 } });

                var copyButton = new Button(() => EditorGUIUtility.systemCopyBuffer = property.Name) { text = "Copy" };
                propertyRow.Add(copyButton);

                container.Add(propertyRow);

                if (!string.IsNullOrEmpty(property.Description))
                {
                    container.Add(new Label(property.Description) { style = { unityFontStyleAndWeight = FontStyle.Italic } });
                }
            }
            
            _resultsContainer.Add(container);
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

            window.UpdateResults();
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
            
            window.UpdateResults();
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
            
            constantTypes.Add(typeof(void));
            constantTypes.Add(typeof(Component));

            foreach (var method in methods)
            {
                if (bannedMethods.Contains(method.Name)) continue;
                if (method.Name.Contains("`1")) continue;
                if (method.IsSpecialName) continue;
                if (method.IsPrivate) continue;
                if (!displayVoid && method.ReturnType == typeof(void)) continue;
                if (!constantTypes.Contains(method.ReturnType) && !method.ReturnType.IsEnum) continue;
                if (method.GetParameters().Any(p => !constantTypes.Contains(p.ParameterType) && !p.ParameterType.IsEnum)) continue;

                var name = method.IsStatic ? $"static {RenameTypes(type.Name)}.{method.Name}" : $"{RenameTypes(type.Name)} {method.Name}";
                var description = method.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? "";

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
            
            var constructors = type.GetConstructors();
            
            foreach (var constructor in constructors)
            {
                if (constructor.IsPrivate) continue;
                if (constructor.IsStatic) continue;
                if (constructor.Name.Contains("`1")) continue;

                var description = constructor.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? "";
                var name = "new " + RenameTypes(type.Name);
                
                self._methods.Add(new MethodInfos(
                    name,
                    description,
                    RenameTypes(type.Name),
                    constructor.GetParameters().Select(p => RenameTypes(p.ParameterType.Name) + " " + p.Name).ToArray()
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
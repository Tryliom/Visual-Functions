#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace VisualFunctions
{
    [InitializeOnLoad]
    public static class FunctionUtility
    {
        private static readonly List<Function> Functions = new();

        [NonSerialized] private static bool _unityInitialized;

        static FunctionUtility()
        {
            if (_unityInitialized)
            {
                InitializeFunctions();
                return;
            }

            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.ExitingPlayMode)
                InitializeFunctions();
        }

        public static void RegisterFunction(Function function)
        {
            if (!Functions.Contains(function)) Functions.Add(function);
        }

        private static void Update()
        {
            InitializeFunctions();
            EditorApplication.update -= Update;
        }

        private static void InitializeFunctions()
        {
            var functionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Function)) && !t.IsAbstract)
                .ToList();

            foreach (var type in functionTypes)
            {
                var nameProperty = type.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                var descriptionProperty = type.GetField("Description", BindingFlags.Public | BindingFlags.Static);
                var categoryProperty = type.GetField("Category", BindingFlags.Public | BindingFlags.Static);

                if (descriptionProperty == null || categoryProperty == null || nameProperty == null)
                {
                    throw new Exception($"The function {type.Name} is missing the Name or Description or Category field");
                }

                var name = (string)nameProperty.GetValue(null);
                var description = (string)descriptionProperty.GetValue(null);
                var category = (FunctionCategory)categoryProperty.GetValue(null);

                if (Function.Functions.ContainsKey(name)) continue;

                Function.Functions.Add(name, new Function.FunctionInfo
                {
                    Type = type, Description = description, Category = category,
                    Instance = Activator.CreateInstance(type)
                });

                Function.Functions[name].Instance.GetType().GetMethod("GenerateFields")
                    ?.Invoke(Function.Functions[name].Instance, null);
            }

            // Reorder the functions by category
            Function.Functions = Function.Functions
                .OrderBy(x => x.Value.Category)
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var function in Functions) function.ValidateFields();

            _unityInitialized = true;
        }
    }
}
#endif
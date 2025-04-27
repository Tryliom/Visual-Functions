#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TryliomFunctions
{
    public static class ReferenceUtility
    {
        public const string PathToGlobalVariables = "Assets/Resources/ScriptableObjects/GlobalVariables";
        public const string PathToVariables = "Assets/Resources/ScriptableObjects/Variables";

        private static readonly Dictionary<Type, Type> VariableTypes = new();
        private static readonly Dictionary<Type, Type> ReferenceTypes = new();
        private static readonly Dictionary<Type, Type> ReferenceToVariable = new();
        private static List<Type> _iValueTypes = new();

        static ReferenceUtility()
        {
            InitializeDictionaries();
        }

        private static void InitializeDictionaries()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var variableTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.Name.EndsWith("Variable") && !t.IsAbstract);
            var iValueTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsGenericType && !t.IsInterface && !t.IsAbstract && 
                            t.GetInterfaces().Contains(typeof(IValue)) && t.GetConstructor(Type.EmptyTypes) != null);

            foreach (var variableType in variableTypes)
            {
                var constantType = variableType.BaseType?.GenericTypeArguments.FirstOrDefault();
                if (constantType != null) VariableTypes[constantType] = variableType;
            }

            foreach (var iValueType in iValueTypes)
            {
                var constantType = iValueType.BaseType?.GenericTypeArguments.FirstOrDefault();

                _iValueTypes.Add(iValueType);

                if (constantType == null) continue;

                ReferenceTypes[constantType] = iValueType;
                ReferenceToVariable[iValueType] = VariableTypes[constantType];
            }

            // Reorder the IValue types by: generic type, UnityEngine type, and then by name
            _iValueTypes = _iValueTypes.OrderBy(x => x.BaseType != null && x.BaseType.GenericTypeArguments.Length > 0 && x.BaseType.GenericTypeArguments[0].IsGenericType)
                .ThenBy(x => x.BaseType == null || x.BaseType.GenericTypeArguments.Length == 0)
                .ThenBy(x => x.BaseType != null && x.BaseType.GenericTypeArguments.Length > 0 && x.BaseType.GenericTypeArguments[0].Namespace == "UnityEngine")
                .ThenBy(x => x.Name)
                .ToList();
        }

        public static Type GetReferenceType(Type type)
        {
            return ReferenceTypes.GetValueOrDefault(type);
        }

        public static List<Type> GetAllIValueTypes()
        {
            return _iValueTypes;
        }

        public static Type GetVariableFromReference(Type type)
        {
            return ReferenceToVariable.GetValueOrDefault(type);
        }

        public static List<Type> GetAllConstantTypes()
        {
            return VariableTypes.Keys.ToList();
        }

        /**
         * Create a new variable asset of the given type that is not a reference or a variable.
         */
        public static ScriptableObject CreateVariableAsset(Type constantType, string startName, string path)
        {
            var variableType = VariableTypes[constantType];
            var asset = ScriptableObject.CreateInstance(variableType);
            var name = FormatNameWithPath(startName, path);

            asset.name = name;

            AssetDatabase.CreateAsset(asset, $"{path}/{name}.asset");

            return asset;
        }

        private static string FormatNameWithPath(string baseName, string path)
        {
            var name = baseName;

            // Check if the name is already taken
            var i = 0;
            while (File.Exists($"{path}/{name}{(i == 0 ? "" : i.ToString())}.asset")) i++;
            name = $"{name}{(i == 0 ? "" : i.ToString())}";

            return name;
        }
    }
#endif
}
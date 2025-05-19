using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualFunctions;
using Object = UnityEngine.Object;

[Serializable]
public class ExportedFunctions
{
    public Object Asset;
    public string FunctionsPropertyPath;
    
    public ExportedFunctions(Object asset, string functionsPropertyPath)
    {
        Asset = asset;
        FunctionsPropertyPath = functionsPropertyPath;
    }
    
#if UNITY_EDITOR
    public Functions GetFunctions()
    {
        var property = new SerializedObject(Asset).FindProperty(FunctionsPropertyPath);
        
        return PropertyDrawerUtility.RetrieveTargetObject(property) as Functions;
    }
#endif
}

[CreateAssetMenu(fileName = "ExportableFields", menuName = "Visual Functions/Exportable Fields")]
[Serializable]
public class ExportableFields : ScriptableObject
{
#if UNITY_EDITOR
    public string DeveloperDescription;
    public List<ExportedFunctions> ExportedOnFunctions = new ();
#endif
    
    public List<Field> Fields = new ();
}
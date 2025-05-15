using System;
using System.Collections.Generic;
using UnityEngine;
using VisualFunctions;

[CreateAssetMenu(fileName = "ExportableFields", menuName = "Visual Functions/Exportable Fields")]
[Serializable]
public class ExportableFields : ScriptableObject
{
#if UNITY_EDITOR
    public string DeveloperDescription;
#endif
    
    public List<Field> Fields = new ();
}
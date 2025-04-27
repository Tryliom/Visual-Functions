using System;
using TryliomFunctions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

[Serializable]
public class Formula : IValue<string>
{
    public string FormulaValue = string.Empty;
    
    object IValue.Value
    {
        get => FormulaValue;
        set => FormulaValue = (string) value;
    }

    public string Value
    {
        get => FormulaValue;
        set => FormulaValue = value;
    }

    public Type Type { get; } = typeof(string);
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Formula))]
public class FormulaPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var helpIcon = EditorGUIUtility.IconContent("_Help");
        helpIcon.tooltip = "Support all logical operations (+,-,/,&&,||,...) and variables.\nAssign result to a variable with: varA = A + B. Execute functions also work.\n" +
                           "Boolean are converted to int when used in +, -, * and / operations\n" +
                           "If ternary (condition ? if true : if false) used inside another need to have ()\n" +
                           "Examples: \n" +
                           "varA = A + B\n" +
                           "A.x *= Random.Range(0, A.y)\n" +
                           "myVar.myMethod()\n" +
                           "A.x += A.y; A.y += Random.Range(-0.5, 2)";
        
        var iconRect = new Rect(position.x + position.width - 20, position.y, 20, position.height);
        EditorGUI.LabelField(iconRect, helpIcon);
        
        position.width -= 25;
        var formulaProperty = property.FindPropertyRelative("FormulaValue");
        EditorGUI.PropertyField(position, formulaProperty, label);
    }
}
#endif
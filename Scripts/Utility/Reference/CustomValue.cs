using System;
using System.Collections.Generic;
using TryliomFunctions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

/**
 * Used to define a custom value that can be evaluated using a formula. Can be any type.
 */
[Serializable]
public class CustomValue : IValue
{
    // The formula to be evaluated to define the value
    public string Formula;
    private string _lastEvaluatedFormula = string.Empty;
    public bool RecalculateOnUpdate = true;

    public object Value { get; set; }
    public Type Type { get; } = typeof(object);
    
    /**
     * Set the value from the formula.
     */
    public void Evaluate(string uid, List<ExpressionVariable> variables)
    {
        if (_lastEvaluatedFormula != Formula)
        {
            _lastEvaluatedFormula = Formula;
            Value = Evaluator.Process(uid, Formula, variables);
            return;
        }

        if (RecalculateOnUpdate)
        {
            Value = Evaluator.Process(uid, Formula, variables);
            return;
        }

        Value ??= Evaluator.Process(uid, Formula, variables);
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CustomValue))]
public class CustomValuePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var formulaProperty = property.FindPropertyRelative("Formula");
        var helpIcon = EditorGUIUtility.IconContent("_Help");
        helpIcon.tooltip = "The formula will define the value of this variable, can be any type";
        
        var iconRect = new Rect(position.x + position.width - 20, position.y, 20, position.height);
        EditorGUI.LabelField(iconRect, helpIcon);
        
        position.width -= 25;
        position.height = EditorStyles.textArea.CalcHeight(new GUIContent(formulaProperty.stringValue), position.width);
        formulaProperty.stringValue = EditorGUI.TextArea(position, formulaProperty.stringValue, EditorStyles.textArea);
        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
        
        var recalculateProperty = property.FindPropertyRelative("RecalculateOnUpdate");
        EditorGUI.PropertyField(position, recalculateProperty, new GUIContent("Recalculate on use", "If true, the formula will be re-evaluated every time the variable is used"));
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var formulaProperty = property.FindPropertyRelative("Formula");
        var textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(formulaProperty.stringValue), EditorGUIUtility.currentViewWidth);
        var recalculateHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        
        return textHeight + recalculateHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
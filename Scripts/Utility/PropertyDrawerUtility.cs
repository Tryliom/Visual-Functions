using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class PropertyDrawerUtility
{
    public static void SaveAndRefresh(SerializedProperty property, GameObject targetObject, Action redrawAction = null)
    {
        // Used for game objects
        if (targetObject && PrefabUtility.IsPartOfPrefabInstance(targetObject))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
        }

        if (property.serializedObject != null)
        {
            // The order is important
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
        
        redrawAction?.Invoke();

        // Used for scriptable objects
        if (!targetObject || !PrefabUtility.IsPartOfPrefabInstance(targetObject))
        {
            AssetDatabase.SaveAssets();
        }
    }

    /**
     * Retrieve the target object from a SerializedProperty.
     */
    public static object RetrieveTargetObject(SerializedProperty property)
    {
        var targetObject = property.serializedObject.targetObject;
        var propertyPath = property.propertyPath.Replace(".Array.data[", "[");
        var pathParts = propertyPath.Split('.');

        object currentObject = targetObject;
        foreach (var part in pathParts)
        {
            if (part.Contains("["))
            {
                var arrayPart = part[..part.IndexOf("[", StringComparison.Ordinal)];
                var indexPart = int.Parse(part.Substring(
                    part.IndexOf("[", StringComparison.Ordinal) + 1,
                    part.IndexOf("]", StringComparison.Ordinal) - part.IndexOf("[", StringComparison.Ordinal) - 1
                ));

                var field = currentObject.GetType().GetField(arrayPart, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    Debug.LogError($"Field '{arrayPart}' not found on object of type '{currentObject.GetType().Name}'");
                    return null;
                }

                if (field.GetValue(currentObject) is not IList array)
                {
                    Debug.LogError($"Field '{arrayPart}' is not an array on object of type '{currentObject.GetType().Name}'");
                    return null;
                }

                currentObject = array[indexPart];
            }
            else
            {
                var field = currentObject.GetType().GetField(part, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    Debug.LogError($"Field '{part}' not found on object of type '{currentObject.GetType().Name}'");
                    return null;
                }

                currentObject = field.GetValue(currentObject);
            }
        }
        
        return currentObject;
    }
}
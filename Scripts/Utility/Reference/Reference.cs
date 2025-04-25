using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TryliomFunctions
{
     [Serializable]
     public class Reference<TType> : IResettable, IValue<TType>
     {
         public bool UseLocal = false;
         public TType LocalValue;
         public Variable<TType> Variable;
         
         public bool DisplayedInInspector = false;
     
         public Reference() {}
     
         public Reference(TType value)
         {
             UseLocal = true;
             LocalValue = value;
         }

         public Reference(bool useLocal)
         {
             UseLocal = useLocal;
         }
         
         public TType Value 
         {
             get => UseLocal ? LocalValue : Variable.Value;
             set
             {
                 if (UseLocal)
                 {
                     LocalValue = value;
                 }
                 else
                 {
                     Variable.Value = value;
                 }
             }
         }
    
         object IValue.Value
         {
             get => Value;
             set
             {
                 if (value is Variable<TType> variable)
                 {
                     Variable = (Variable<TType>) value;
                     UseLocal = false;
                 }
                 else
                 {
                    Value = (TType)value;
                 }
             }
         }
         
         public Type Type => typeof(TType);
     
         public static implicit operator TType(Reference<TType> reference)
         {
             return reference.Value;
         }
         
         public static implicit operator Reference<TType>(Variable<TType> value)
         {
             return new Reference<TType> {Variable = value};
         }
         
         public static implicit operator Reference<TType>(TType value)
         {
             return new Reference<TType>(value);
         }
         
         public void ResetValue()
         {
             Value = default;
         }
     }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Reference<>), useForChildren: true)]
    public class ReferenceDrawer : PropertyDrawer
    {
        private GUIStyle _buttonStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _buttonStyle ??= new GUIStyle
            {
                imagePosition = ImagePosition.ImageOnly,
                fixedWidth = 20
            };

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
        
            EditorGUI.BeginChangeCheck();

            // Get properties
            var useLocal = property.FindPropertyRelative("UseLocal");
            var localValue = property.FindPropertyRelative("LocalValue");
            var variable = property.FindPropertyRelative("Variable");
            var displayedInInspector = property.FindPropertyRelative("DisplayedInInspector");

            // Add a foldout to show the variable properties
            if (!useLocal.boolValue && variable.objectReferenceValue != null)
            {
                bool currentValue = displayedInInspector.boolValue;
                var rect = new Rect(position);

                rect.yMin += _buttonStyle.margin.top - 3;
                rect.width = _buttonStyle.fixedWidth + _buttonStyle.margin.right - 5;

                currentValue = EditorGUI.Foldout(rect, currentValue, "");
                displayedInInspector.boolValue = currentValue;

                if (currentValue)
                {
                    EditorGUI.indentLevel++;
                    var serializedObject = new SerializedObject(variable.objectReferenceValue);
                    var prop = serializedObject.GetIterator();

                    prop.NextVisible(true);

                    while (prop.NextVisible(false))
                    {
                        EditorGUILayout.PropertyField(prop, true);
                    }

                    serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
            }
            
            position.xMin += 15;

            var refreshIcon = new GUIContent(EditorGUIUtility.IconContent("Refresh").image, "Toggle between local and variable");
            var buttonRect = new Rect(position);

            buttonRect.yMin += _buttonStyle.margin.top;
            buttonRect.width = _buttonStyle.fixedWidth + _buttonStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (GUI.Button(buttonRect, refreshIcon, _buttonStyle))
            {
                useLocal.boolValue = !useLocal.boolValue;
            }

            if (useLocal.boolValue && localValue.propertyType == SerializedPropertyType.String)
            {
                float textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(localValue.stringValue), position.width);
                position.height = textHeight;
                localValue.stringValue = EditorGUI.TextArea(position, localValue.stringValue, EditorStyles.textArea);
            }
            else
            {
                EditorGUI.PropertyField(position, useLocal.boolValue ? localValue : variable, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var useLocal = property.FindPropertyRelative("UseLocal");
            var localValue = property.FindPropertyRelative("LocalValue");

            if (useLocal.boolValue && localValue.propertyType == SerializedPropertyType.String)
            {
                float textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(localValue.stringValue), EditorGUIUtility.currentViewWidth);
                return textHeight;
            }
            
            return base.GetPropertyHeight(property, label);
        }
    }
#endif
     
    [Serializable]
    public class FloatReference : Reference<float> {}

    [Serializable]
    public class IntReference : Reference<int> {}

    [Serializable]
    public class BoolReference : Reference<bool> {}

    [Serializable]
    public class StringReference : Reference<string> {}

    [Serializable]
    public class Vector2Reference : Reference<Vector2> {}
    
    [Serializable]
    public class Vector3Reference : Reference<Vector3> {}
    
    [Serializable]
    public class RigidbodyReference : Reference<Rigidbody> {}

    [Serializable]
    public class Rigidbody2DReference : Reference<Rigidbody2D> {}

    [Serializable]
    public class ColliderReference : Reference<Collider> {}
    
    [Serializable]
    public class Collider2DReference : Reference<Collider2D> {}

    [Serializable]
    public class GameObjectReference : Reference<GameObject> {}   

    [Serializable]
    public class TransformReference : Reference<Transform> {}
    
    [Serializable]
    public class ColorReference : Reference<Color> {}
    
    [Serializable]
    public class QuaternionReference : Reference<Quaternion> {}
}
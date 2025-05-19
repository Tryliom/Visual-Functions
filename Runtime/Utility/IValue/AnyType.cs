using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [Serializable]
    public class AnyType<TType> : IValue<TType>
    {
        public TType TypeValue;
        
        object IValue.Value
        {
            get => Value;
            set => Value = (TType) value;
        }

        public TType Value
        {
            get => TypeValue;
            set => TypeValue = value;
        }
        public Type Type => typeof(TType);

        public IValue Clone()
        {
            return new AnyType<TType>
            {
                Value = Value
            };
        }
    }
    
    [Serializable]
    public class AnyType : IRefType, IRefValue
    {
        [SerializeReference] public IValue TypeValue;
        
        public Type Type => TypeValue != null ? TypeValue.Type : typeof(AnyType);

        public object RefValue
        {
            get => TypeValue?.Value;
            set => TypeValue.Value = value;
        }
        
        public void SetValue(Type type)
        {
            var listType = typeof(AnyType<>).MakeGenericType(type);
            var listValue = Activator.CreateInstance(listType);
            TypeValue = (IValue) listValue;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnyType))]
    public class AnyTypeDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        private GameObject _targetObject;
        private AnyType _anyType;
        
        private void Refresh()
        {
            if (_property == null || _targetObject == null) return;

            PropertyDrawerUtility.SaveAndRefresh(_property, _targetObject);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;
            _targetObject = Selection.activeObject as GameObject;
            _anyType = _property.serializedObject.targetObject is AnyTypeVariable comp ? comp.Value : (AnyType) PropertyDrawerUtility.RetrieveTargetObject(_property);
            
            var isDefined = _anyType.TypeValue is not null;
            var buttonText = isDefined ? "..." : "Select Type";
            var buttonWidth = isDefined ? 20 : buttonText.Length * 8f;

            position.height = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(new Rect(position.x, position.y, buttonWidth, position.height), buttonText))
            {
                var menu = new GenericMenu();
                
                AddItemsToMenu(menu, "Popular", ExpressionUtility.PopularTypes);
                AddItemsToMenu(menu, "Unity", ExpressionUtility.UnityTypes);
                AddItemsToMenu(menu, "Other", ExpressionUtility.OtherTypes);

                menu.ShowAsContext();
            }

            if (isDefined)
            {
                position.x += buttonWidth + 5f;
                position.width -= buttonWidth + 5f;
                
                var listValue = _property.FindPropertyRelative("TypeValue");
                EditorGUI.PropertyField(position, listValue, true);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var listValue = property.FindPropertyRelative("TypeValue");

            return EditorGUI.GetPropertyHeight(listValue, true);
        }

        private void AddItemsToMenu(GenericMenu menu, string prefix, List<Type> types)
        {
            if (_anyType.TypeValue == null)
            {
                menu.AddItem(new GUIContent("None"), true, () => {});
            }
            
            foreach (var type in types)
            {
                var name = ObjectNames.NicifyVariableName(ExpressionUtility.GetBetterTypeName(type));
                var startStr = prefix != string.Empty ? $"{prefix}/" : "";
                var namespaceParts = type.FullName?.Split(".") ?? Array.Empty<string>();

                for (var i = 0; i < namespaceParts.Length - 1; i++)
                {
                    startStr += namespaceParts[i] + "/";
                }
                
                startStr += name;
                
                menu.AddItem(new GUIContent(startStr), _anyType.TypeValue?.Type == type, () =>
                {
                    _anyType.SetValue(type);
                    Refresh();
                });
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(AnyType<>))]
    public class AnyTypeGenericDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var value = property.FindPropertyRelative("TypeValue");

            if (value == null)
            {
                GUI.Label(position, "No GUI available for this type");
                return;
            }
            
            var name = ObjectNames.NicifyVariableName(value.type).Replace("P Ptr$", "");
            
            GUI.Label(position, new GUIContent(name));
            
            position.x += name.Length * 8f + 5f;
            position.width -= name.Length * 8f + 5f;

			if (value.propertyType == SerializedPropertyType.String)
            {
                var textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(value.stringValue), position.width);
                position.height = textHeight;
                value.stringValue = EditorGUI.TextArea(position, value.stringValue, EditorStyles.textArea);
            }
            else
            {
                EditorGUI.PropertyField(position, value, new GUIContent(""));
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var value = property.FindPropertyRelative("TypeValue");
            
            if (value == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            
            if (value.propertyType == SerializedPropertyType.String)
            {
                var textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(value.stringValue), EditorGUIUtility.currentViewWidth);
                return textHeight;
            }
            
            return EditorGUI.GetPropertyHeight(value, true);
        }
    }
#endif
}
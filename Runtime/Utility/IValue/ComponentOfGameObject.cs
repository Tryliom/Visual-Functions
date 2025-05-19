using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [Serializable]
    public class ComponentOfGameObject : IRefType, IRefValue
    {
        public GameObject GameObject;
        public Component Component;
        
        public Type Type => Component ? Component.GetType() : typeof(GameObject);
        
        public void SetComponent(Component component)
        {
            Component = component;
        }

        public object RefValue
        {
            get
            { 
                if (!GameObject) return null;
                
                return Component ? Component : GameObject;
            }
            set
            {
                switch (value)
                {
                    case GameObject gameObject:
                        GameObject = gameObject;
                        Component = null;
                        break;
                    case Component component:
                        GameObject = component.gameObject;
                        Component = component;
                        break;
                }
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ComponentOfGameObject))]
    public class ComponentOfGameObjectDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        private GameObject _targetObject;
        
        private void Refresh()
        {
            if (_property == null || _targetObject == null) return;

            PropertyDrawerUtility.SaveAndRefresh(_property, _targetObject);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;
            _targetObject = Selection.activeObject as GameObject;
            
            EditorGUI.BeginProperty(position, label, property);
            
            var gameObjectProperty = property.FindPropertyRelative("GameObject");
            var componentProperty = property.FindPropertyRelative("Component");
            var isComponentedDefined = componentProperty.objectReferenceValue is not null;
            var componentName = isComponentedDefined ? componentProperty.objectReferenceValue.GetType().Name : "Game Object";
            var displayName = ObjectNames.NicifyVariableName(componentName) + " of";
            var displayNameWidth = displayName.Length * 6.2f + 5f;

            position.height = EditorGUIUtility.singleLineHeight;
            
            GUI.Label(new Rect(position.x, position.y, displayNameWidth, position.height), displayName);
            
            EditorGUI.PropertyField(new Rect(position.x + displayNameWidth + 5, position.y, position.width - displayNameWidth - 5 - 20 - 5, position.height), gameObjectProperty, GUIContent.none);

            if (gameObjectProperty.objectReferenceValue is not GameObject gameObject)
            {
                EditorGUI.EndProperty();
                return;
            }
            
            if (GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, position.height), $"..."))
            {
                var menu = new GenericMenu();
                var parent = gameObject.transform.parent;
                var parents = parent ? parent.GetComponents<Component>() : Array.Empty<Component>();
                var actualItem = gameObject.GetComponents<Component>();
                var children = gameObject.GetComponentsInChildren<Component>().Where(x => !actualItem.Contains(x)).ToArray();
                
                AddItemsToMenu(menu, "Parent", parents);
                AddItemsToMenu(menu, "", actualItem);
                AddItemsToMenu(menu, gameObject.name, children);

                menu.ShowAsContext();
            }
            
            EditorGUI.EndProperty();
        }
        
        private void AddItemsToMenu(GenericMenu menu, string prefix, IEnumerable<Component> components)
        {
            var isAddedList = new List<string>();
            var componentOfGameObject = _property.serializedObject.targetObject is ComponentOfGameObjectVariable comp ?
                comp.Value : (ComponentOfGameObject) PropertyDrawerUtility.RetrieveTargetObject(_property);
            
            foreach (var component in components)
            {
                var componentName = component.gameObject.name;
                var startStr = prefix != string.Empty ? $"{prefix}/{componentName}" : componentName;
                
                if (!isAddedList.Contains(componentName))
                {
                    isAddedList.Add(componentName);

                    menu.AddItem(new GUIContent($"{startStr}/Game Object"), !componentOfGameObject.Component, () =>
                    {
                        componentOfGameObject.GameObject = component.gameObject;
                        componentOfGameObject.Component = null;
                        Refresh();
                    });
                }
                
                menu.AddItem(new GUIContent($"{startStr}/{ObjectNames.NicifyVariableName(component.GetType().Name)}"), componentOfGameObject.Component == component, () =>
                {
                    componentOfGameObject.GameObject = component.gameObject;
                    componentOfGameObject.Component = component;
                    Refresh();
                });
            }
        }
    }
#endif
}
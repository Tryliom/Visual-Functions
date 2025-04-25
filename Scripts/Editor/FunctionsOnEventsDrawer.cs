using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TryliomFunctions
{
    [CustomPropertyDrawer(typeof(FunctionsOnEvents))]
    public class FunctionsOnEventsDrawer : PropertyDrawer
    {
        private VisualElement _content;
        private SerializedProperty _property;
        private GameObject _targetObject;

        private void Refresh()
        {
            if (_targetObject && PrefabUtility.IsPartOfPrefabInstance(_targetObject))
                PrefabUtility.RecordPrefabInstancePropertyModifications(_targetObject);

            if (_property.serializedObject != null)
            {
                _property.serializedObject.ApplyModifiedProperties();
                _property.serializedObject.Update();
            }

            CreateGUI(_property, _content);

            if (!_targetObject || !PrefabUtility.IsPartOfPrefabInstance(_targetObject)) AssetDatabase.SaveAssets();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _targetObject = Selection.activeObject as GameObject;
            _property = property;

            var container = new VisualElement();
            _content = new VisualElement();

            CreateGUI(_property, _content);

            container.Add(_content);

            return container;
        }

        private void CreateGUI(SerializedProperty property, VisualElement container)
        {
            container.Clear();

            var listProperty = property.FindPropertyRelative("_functionsOnEvents");

            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var elementProperty = listProperty.GetArrayElementAtIndex(i);

                var box = new VisualElement
                {
                    style =
                    {
                        marginBottom = 5,
                        minHeight = 24
                    }
                };

                var onEventProperty = elementProperty.FindPropertyRelative("OnEvent");
                var functionsProperty = elementProperty.FindPropertyRelative("Functions");

                box.Add(new PropertyField(onEventProperty, "On Event")
                {
                    style =
                    {
                        marginRight = 30
                    }
                });
                box.Add(new PropertyField(functionsProperty, "Functions"));

                var index = i;
                var removeButton = new Button(() =>
                {
                    listProperty.DeleteArrayElementAtIndex(index);
                    Refresh();
                })
                {
                    text = "-",
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        right = 0,
                        width = 20,
                        height = 18
                    }
                };

                box.Add(removeButton);

                if (i != listProperty.arraySize - 1)
                {
                    // Add a line between elements
                    var line = new VisualElement
                    {
                        style =
                        {
                            backgroundColor = new Color(0.5f, 0.5f, 0.5f),
                            height = 1,
                            marginTop = 5,
                            marginBottom = 5
                        }
                    };

                    box.Add(line);
                }

                container.Add(box);
            }

            var addButton = new Button(() =>
            {
                listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);

                newElement.managedReferenceValue = new FunctionsOnEvent();

                Refresh();
            })
            {
                text = "New Event",
                style =
                {
                    marginTop = 5,
                    width = 100,
                    height = 20
                }
            };

            container.Add(addButton);
        }
    }
}
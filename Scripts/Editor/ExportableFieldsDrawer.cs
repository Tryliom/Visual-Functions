using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualFunctions
{
    [CustomEditor(typeof(ExportableFields))]
    public class ExportableFieldsEditor : Editor
    {
        private VisualElement _content;
        private ExportableFields _targetObject;

        private void Refresh()
        {
            if (_targetObject && PrefabUtility.IsPartOfPrefabInstance(_targetObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(_targetObject);
            }

            // The order is important
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            CreateGUI(_content);
            
            if (!_targetObject || !PrefabUtility.IsPartOfPrefabInstance(_targetObject))
            {
                AssetDatabase.SaveAssets();
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            _targetObject = serializedObject.targetObject as ExportableFields;

            var container = new VisualElement()
            {
                style =
                {
                    marginTop = 5
                }
            };
            _content = new VisualElement();

            CreateGUI(_content);

            container.Add(_content);

            return container;
        }

        private void CreateGUI(VisualElement container)
        {
            container.Clear();

            if (!_targetObject)
            {
                Debug.LogError("Failed to cast the resolved object to 'ExportableFields'");
                return;
            }
            
            var descriptionProperty = serializedObject.FindProperty("DeveloperDescription");
            var descriptionField = new TextField("Description")
            {
                value = descriptionProperty.stringValue,
                multiline = true,
                style =
                {
                    marginTop = 5,
                    marginBottom = 5
                }
            };
            
            descriptionField.RegisterValueChangedCallback(evt =>
            {
                descriptionProperty.stringValue = evt.newValue;
            });

            descriptionField.RegisterCallback<FocusOutEvent>(evt =>
            {
                descriptionProperty.stringValue = descriptionField.value;
                Refresh();
            });
            
            container.Add(descriptionField);
            
            var inputsProperty = serializedObject.FindProperty("Fields");
            var inputsContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                }
            };
            
            FunctionsDrawer.CreateFields(
                inputsContainer, "Exported Fields", "",
                _targetObject.Fields, null,
                () =>
                {
                    _targetObject.Fields.Add(new Field("" + (char)('A' + _targetObject.Fields.Count)));
                    FormulaCache.Clear();
                    Refresh();
                },
                () =>
                {
                    _targetObject.Fields.Add(FunctionsDrawer.CopiedField.Clone());
                    FunctionsDrawer.CopiedField = null;
                    FormulaCache.Clear();
                    Refresh();
                },
                (element, field) =>
                {
                    element.Add(
                        FunctionsDrawer.CreateField(
                            inputsProperty.GetArrayElementAtIndex(_targetObject.Fields.IndexOf(field)),
                            field, "Exportable", true, new FunctionSettings().AllowMethods(true),
                            _targetObject.Fields, 0,
                            (previousName, newName) =>
                            {
                                _targetObject.Fields.ForEach(input => input.OnEditField(previousName, newName));
                            },
                            Refresh
                        )
                    );
                },
                Refresh
            );
            
            container.Add(inputsContainer);
        }
    }
}
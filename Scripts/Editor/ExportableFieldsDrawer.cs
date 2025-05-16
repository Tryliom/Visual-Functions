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
        private SerializedObject _serializedObject;

        private void Refresh()
        {
            if (PrefabUtility.IsPartOfPrefabInstance(_targetObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(_targetObject);
            }
            
            EditorUtility.SetDirty(_targetObject);

            // The order is important
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();

            CreateGUI(_content);
            
            AssetDatabase.SaveAssets();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _serializedObject = serializedObject;
            _targetObject = _serializedObject.targetObject as ExportableFields;

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
            
            var descriptionProperty = _serializedObject.FindProperty("DeveloperDescription");
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
            
            var inputsProperty = _serializedObject.FindProperty("Fields");
            var inputsContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5
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
                                _targetObject.ExportedOnFunctions.ForEach(functions =>
                                {
                                    functions.GetFunctions().EditField(previousName, newName);
                                    
                                    EditorUtility.SetDirty(functions.Asset);

                                    // The order is important
                                    /*var serializedFunctions = new SerializedObject(functions.Asset);
                                    serializedFunctions.ApplyModifiedProperties();
                                    serializedFunctions.Update();*/
                                    
                                    AssetDatabase.SaveAssets();
                                });
                                FormulaCache.Clear();
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
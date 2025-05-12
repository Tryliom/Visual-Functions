using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualFunctions
{
    [CustomPropertyDrawer(typeof(CustomFunction))]
    public class CustomFunctionDrawer : PropertyDrawer
    {
        private VisualElement _content;
        private SerializedProperty _property;
        private GameObject _targetObject;
        
        private void Refresh()
        {
            PropertyDrawerUtility.SaveAndRefresh(
                _property,
                _targetObject,
                () => CreateGUI(_property, _content)
            );
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _targetObject = Selection.activeObject as GameObject;
            _property = property;

            var container = new VisualElement()
            {
                style =
                {
                    marginTop = 5
                }
            };
            _content = new VisualElement();

            CreateGUI(_property, _content);

            container.Add(_content);

            return container;
        }

        private void CreateGUI(SerializedProperty property, VisualElement container)
        {
            container.Clear();
            
            if (PropertyDrawerUtility.RetrieveTargetObject(_property) is not CustomFunction customFunction)
            {
                Debug.LogError("Failed to cast the resolved object to 'CustomFunction'");
                return;
            }
            
            // First, display the inputs
            var inputsProperty = property.FindPropertyRelative("Inputs");
            var inputsFoldoutProperty = property.FindPropertyRelative("InputFoldoutOpen");
            var inputsContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                }
            };
            
            FunctionsDrawer.CreateFields(
                inputsContainer, "Inputs", "These are the inputs to the function and will be passed to the function when it is invoked.\nIf it's call with less inputs than defined, it will use the default value of inputs.",
                customFunction.Inputs, inputsFoldoutProperty,
                () =>
                {
                    customFunction.Inputs.Add(new Field("" + (char)('A' + customFunction.Inputs.Count)));
                    customFunction.InputFoldoutOpen = true;
                    FormulaCache.Clear();
                    Refresh();
                },
                () =>
                {
                    customFunction.Inputs.Add(FunctionsDrawer.CopiedField.Clone());
                    FunctionsDrawer.CopiedField = null;
                    FormulaCache.Clear();
                    Refresh();
                },
                (element, field) =>
                {
                    element.Add(
                        FunctionsDrawer.CreateField(
                            inputsProperty.GetArrayElementAtIndex(customFunction.Inputs.IndexOf(field)),
                            field, "CustomFunction", true, new FunctionSettings().AllowMethods(true),
                            customFunction.Inputs, 0,
                            (previousName, newName) =>
                            {
                                customFunction.Function.EditField(previousName, newName);
                                customFunction.Inputs.ForEach(input => input.OnEditField(previousName, newName));
                            },
                            Refresh
                        )
                    );
                },
                Refresh
            );
            
            container.Add(inputsContainer);
            
            // Then, display the outputs
            var outputsProperty = property.FindPropertyRelative("Outputs");
            var outputsFoldoutProperty = property.FindPropertyRelative("OutputFoldoutOpen");
            var outputsContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                }
            };
            
            FunctionsDrawer.CreateFields(
                outputsContainer, "Output", "If not defined, the function is considered a void function. The only output that will be used is the first one.",
                customFunction.Outputs, outputsFoldoutProperty,
                () =>
                {
                    customFunction.Outputs.Add(new Field("" + (char)('a' + customFunction.Outputs.Count)));
                    customFunction.OutputFoldoutOpen = true;
                    FormulaCache.Clear();
                    Refresh();
                },
                () =>
                {
                    customFunction.Outputs.Add(FunctionsDrawer.CopiedField.Clone());
                    FunctionsDrawer.CopiedField = null;
                    FormulaCache.Clear();
                    Refresh();
                },
                (element, field) =>
                {
                    element.Add(
                        FunctionsDrawer.CreateField(
                            outputsProperty.GetArrayElementAtIndex(customFunction.Outputs.IndexOf(field)),
                            field, "CustomFunction", true, new FunctionSettings().AllowMethods(true),
                            customFunction.Outputs, 0,
                            (previousName, newName) =>
                            {
                                customFunction.Function.EditField(previousName, newName);
                                customFunction.Outputs.ForEach(output => output.OnEditField(previousName, newName));
                            },
                            Refresh
                        )
                    );
                },
                Refresh
            );
            
            container.Add(outputsContainer);
            
            // Clear and set the temporary global variables for function testing
            customFunction.Function.TemporaryGlobalVariables.Clear();
            customFunction.Function.TemporaryGlobalVariables.Capacity = customFunction.Inputs.Count + customFunction.Outputs.Count;
            customFunction.Function.TemporaryGlobalVariables.AddRange(customFunction.Inputs);
            customFunction.Function.TemporaryGlobalVariables.AddRange(customFunction.Outputs);
            
            // Finally, display the function
            var functionProperty = property.FindPropertyRelative("Function");
            var function = new PropertyField(functionProperty);
            
            function.BindProperty(property.serializedObject);
            container.Add(function);
        }
    }
}
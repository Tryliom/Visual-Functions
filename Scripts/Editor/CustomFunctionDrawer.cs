using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualFunctions
{
    [CustomPropertyDrawer(typeof(CustomFunction))]
    public class CustomFunctionDrawer : PropertyDrawer
    {
        private static readonly Dictionary<SerializedProperty, Data> PropertyData = new();

        private class Data
        {
            public VisualElement Content;
            public SerializedProperty Property;
            public GameObject TargetObject;
        }
        
        private void Refresh(Data data)
        {
            PropertyDrawerUtility.SaveAndRefresh(
                data.Property,
                data.TargetObject,
                () => CreateGUI(data)
            );
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!PropertyData.TryGetValue(property, out var data))
            {
                data = new Data
                {
                    Property = property,
                    TargetObject = Selection.activeObject as GameObject,
                    Content = new VisualElement()
                };
                PropertyData[property] = data;
            }

            var container = new VisualElement
            {
                style =
                {
                    marginTop = 5
                }
            };

            CreateGUI(data);

            container.Add(data.Content);

            return container;
        }

        private void CreateGUI(Data data)
        {
            data.Content.Clear();
            
            if (PropertyDrawerUtility.RetrieveTargetObject(data.Property) is not CustomFunction customFunction)
            {
                Debug.LogError("Failed to cast the resolved object to 'CustomFunction'");
                return;
            }
            
            // First, display the inputs
            var inputsProperty = data.Property.FindPropertyRelative("Inputs");
            var inputsFoldoutProperty = data.Property.FindPropertyRelative("InputFoldoutOpen");
            var inputsContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                }
            };
            
            FunctionsDrawer.CreateFields(
                inputsContainer, "Inputs", "These are the inputs to the function and will be passed to the function when it is invoked.\n" +
                                           "If it's call with less inputs than defined, it will use the default value of inputs.\n" +
                                           "The inputs cannot be modified in the function.",
                customFunction.Inputs, inputsFoldoutProperty,
                () =>
                {
                    customFunction.Inputs.Add(new Field("" + (char)('A' + customFunction.Inputs.Count)));
                    customFunction.InputFoldoutOpen = true;
                    FormulaCache.Clear();
                    Refresh(data);
                },
                () =>
                {
                    customFunction.Inputs.Add(FunctionsDrawer.CopiedField.Clone());
                    FunctionsDrawer.CopiedField = null;
                    FormulaCache.Clear();
                    Refresh(data);
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
                            () => Refresh(data)
                        )
                    );
                },
                () => Refresh(data),
                true
            );
            
            data.Content.Add(inputsContainer);
            
            // Then, display the outputs
            var outputsProperty = data.Property.FindPropertyRelative("Outputs");
            var outputsFoldoutProperty = data.Property.FindPropertyRelative("OutputFoldoutOpen");
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
                    Refresh(data);
                },
                () =>
                {
                    customFunction.Outputs.Add(FunctionsDrawer.CopiedField.Clone());
                    FunctionsDrawer.CopiedField = null;
                    FormulaCache.Clear();
                    Refresh(data);
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
                            () => Refresh(data)
                        )
                    );
                },
                () => Refresh(data),
                true
            );
            
            data.Content.Add(outputsContainer);
            
            // Clear and set the temporary global variables for function testing
            customFunction.Function.TemporaryGlobalVariables.Clear();
            customFunction.Function.TemporaryGlobalVariables.Capacity = customFunction.Inputs.Count + customFunction.Outputs.Count;
            customFunction.Function.TemporaryGlobalVariables.AddRange(customFunction.Inputs);
            customFunction.Function.TemporaryGlobalVariables.AddRange(customFunction.Outputs);
            
            // Finally, display the function
            var functionProperty = data.Property.FindPropertyRelative("Function");
            var function = new PropertyField(functionProperty);
            
            function.BindProperty(data.Property.serializedObject);
            data.Content.Add(function);
        }
    }
}
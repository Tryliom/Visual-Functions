using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TryliomFunctions
{
    [CustomPropertyDrawer(typeof(Functions))]
    public class FunctionsDrawer : PropertyDrawer
    {
        private VisualElement _content;
        private SerializedProperty _property;
        private string _selectedFunctionIndex;
        private GameObject _targetObject;

        private void Refresh()
        {
            // Used for game objects
            if (_targetObject && PrefabUtility.IsPartOfPrefabInstance(_targetObject))
                PrefabUtility.RecordPrefabInstancePropertyModifications(_targetObject);

            if (_property.serializedObject != null)
            {
                // The order is important
                _property.serializedObject.ApplyModifiedProperties();
                _property.serializedObject.Update();
            }

            CreateGUI(_property, _content);

            // Used for scriptable objects
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

            // Put an alert if play mode is active and on a game object
            if (Application.isPlaying)
                container.Add(new Label("⚠️ [Play Mode] " + (_targetObject
                    ? "The changes will not be saved"
                    : "The changes will be saved in the scriptable object") + " ⚠️")
                {
                    style =
                    {
                        marginTop = 5,
                        marginBottom = 5
                    }
                });

            var foldoutOpen = property.FindPropertyRelative("FoldoutOpen");
            var isFoldoutOpen = foldoutOpen is { boolValue: true };
            var title = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center
                }
            };

            title.Add(new Label(property.displayName));

            if (Function.Functions.Count == 0)
            {
                container.Add(new Label("Code has changed, hit the Reload button to update the functions list")
                {
                    style =
                    {
                        marginTop = 5,
                        marginBottom = 5
                    }
                });

                var reloadButton = new Button(() =>
                {
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    AssetDatabase.SaveAssets();
                    Refresh();
                })
                {
                    text = "Reload",
                    style =
                    {
                        marginTop = 5,
                        marginBottom = 5
                    }
                };

                container.Add(title);
                container.Add(reloadButton);

                return;
            }

            // Add a button to launch the functions using Functions.Invoke()
            var launchButton = new Button(() =>
            {
                var targetObject = property.serializedObject.targetObject;
                var propertyPath = property.propertyPath.Replace(".Array.data[", "[");
                var pathParts = propertyPath.Split('.');

                object currentObject = targetObject;
                foreach (var part in pathParts)
                    if (part.Contains("["))
                    {
                        var arrayPart = part[..part.IndexOf("[", StringComparison.Ordinal)];
                        var indexPart = int.Parse(part.Substring(
                            part.IndexOf("[", StringComparison.Ordinal) + 1,
                            part.IndexOf("]", StringComparison.Ordinal) - part.IndexOf("[", StringComparison.Ordinal) -
                            1
                        ));

                        var field = currentObject.GetType().GetField(arrayPart,
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (field == null)
                        {
                            Debug.LogError(
                                $"Field '{arrayPart}' not found on object of type '{currentObject.GetType().Name}'");
                            return;
                        }

                        var array = field.GetValue(currentObject) as IList;
                        if (array == null)
                        {
                            Debug.LogError(
                                $"Field '{arrayPart}' is not an array on object of type '{currentObject.GetType().Name}'");
                            return;
                        }

                        currentObject = array[indexPart];
                    }
                    else
                    {
                        var field = currentObject.GetType().GetField(part,
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (field == null)
                        {
                            Debug.LogError(
                                $"Field '{part}' not found on object of type '{currentObject.GetType().Name}'");
                            return;
                        }

                        currentObject = field.GetValue(currentObject);
                    }

                if (currentObject is Functions functionsInstance)
                    functionsInstance.Invoke();
                else
                    Debug.LogError("Failed to cast the resolved object to 'Functions'");
            })
            {
                text = "Launch",
                style =
                {
                    width = 80,
                    height = 15,
                    top = 0
                }
            };

            var foldout = new Foldout
            {
                text = "Functions",
                value = isFoldoutOpen
            };

            title.Add(launchButton);
            container.Add(title);

            var currentIndex = container.Children().Count() + 1;

            foldout.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("FoldoutOpen").boolValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();

                for (var i = currentIndex; i < container.Children().Count(); i++)
                    container.Children().ElementAt(i).style.display =
                        evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            container.Add(foldout);

            var functionsProperty = property.FindPropertyRelative("FunctionsList");

            for (var i = 0; i < functionsProperty.arraySize; i++)
            {
                var functionProperty = functionsProperty.GetArrayElementAtIndex(i);
                var isEnabled = functionProperty.FindPropertyRelative("Enabled").boolValue;
                var box = new Box
                {
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5,
                        minHeight = 24,
                        backgroundColor = isEnabled
                            ? new Color(0.35f, 0.35f, 0.35f, 0.2f)
                            : new Color(0.1f, 0.1f, 0.1f, 0.2f)
                    }
                };

                box.Add(GetFunction(functionProperty));

                var index = i;
                var removeButton = new Button(() =>
                {
                    functionsProperty.DeleteArrayElementAtIndex(index);
                    Refresh();
                })
                {
                    text = "-",
                    style =
                    {
                        position = Position.Absolute,
                        width = 20,
                        height = 20,
                        top = box.layout.y,
                        right = 0
                    }
                };

                box.Add(removeButton);

                var rightValue = 20;

                if (index != 0)
                {
                    var upButton = new Button(() =>
                    {
                        functionsProperty.MoveArrayElement(index, index - 1);
                        Refresh();
                    })
                    {
                        text = "↑",
                        style =
                        {
                            position = Position.Absolute,
                            width = 20,
                            height = 20,
                            top = box.layout.y,
                            right = rightValue
                        }
                    };

                    box.Add(upButton);
                    rightValue += 20;
                }

                if (index != functionsProperty.arraySize - 1)
                {
                    var downButton = new Button(() =>
                    {
                        functionsProperty.MoveArrayElement(index, index + 1);
                        Refresh();
                    })
                    {
                        text = "↓",
                        style =
                        {
                            position = Position.Absolute,
                            width = 20,
                            height = 20,
                            top = box.layout.y,
                            right = rightValue
                        }
                    };

                    box.Add(downButton);
                }

                container.Add(box);
            }

            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    marginBottom = 10
                }
            };

            var functionDropdown = new PopupField<string>(Function.Functions.Keys.ToList(), 0)
            {
                label = "Add Function"
            };

            _selectedFunctionIndex = functionDropdown.value;

            functionDropdown.RegisterValueChangedCallback(evt => { _selectedFunctionIndex = evt.newValue; });

            var addButton = new Button(() =>
            {
                functionsProperty.arraySize++;
                var functionProperty = functionsProperty.GetArrayElementAtIndex(functionsProperty.arraySize - 1);
                var function = Activator.CreateInstance(Function.Functions[_selectedFunctionIndex].Type) as Function;

                function?.GenerateFields();

                functionProperty.managedReferenceValue = function;
                Refresh();
            })
            {
                text = "Add"
            };

            buttonContainer.Add(functionDropdown);
            buttonContainer.Add(addButton);

            container.Add(buttonContainer);

            if (!isFoldoutOpen)
                for (var i = currentIndex; i < container.Children().Count(); i++)
                    container.Children().ElementAt(i).style.display = DisplayStyle.None;
        }

        private VisualElement GetFunction(SerializedProperty property)
        {
            var container = new VisualElement();
            var foldoutOpen = property.FindPropertyRelative("FoldoutOpen");
            var isEnabled = property.FindPropertyRelative("Enabled").boolValue;
            var refValue = property.managedReferenceValue;
            var functionName = refValue.GetType().GetField("Name").GetValue(refValue).ToString();

            container.Add(new Label(ObjectNames.NicifyVariableName(functionName))
            {
                style =
                {
                    position = Position.Absolute,
                    marginTop = 4,
                    marginBottom = 5,
                    left = 22 + 19
                }
            });

            // Add checkbox to enable or disable the function
            var toggle = new Toggle
            {
                value = isEnabled,
                style =
                {
                    position = Position.Absolute,
                    width = 20,
                    height = 20,
                    top = 0,
                    left = 0
                }
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("Enabled").boolValue = evt.newValue;
                Refresh();
            });

            container.Add(toggle);

            var parametersContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 15,
                    marginTop = 5
                }
            };

            if (refValue is not Function myFunction) return container;

            var description = myFunction.GetType().GetField("Description").GetValue(myFunction).ToString();
            var descriptionImage = new Image
            {
                tooltip = description,
                style =
                {
                    position = Position.Absolute,
                    width = 15,
                    height = 15,
                    top = 4,
                    left = 22
                },
                image = EditorGUIUtility.IconContent("console.infoicon").image
            };

            container.Add(descriptionImage);

            var foldout = new Foldout
            {
                text = "",
                value = foldoutOpen.boolValue,
                style =
                {
                    marginLeft = -5,
                    width = 10
                }
            };

            var currentIndex = container.Children().Count() + 1;

            foldout.RegisterValueChangedCallback(evt =>
            {
                foldoutOpen.boolValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();

                for (var i = currentIndex; i < container.Children().Count(); i++)
                    container.Children().ElementAt(i).style.display =
                        foldoutOpen.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            container.Add(foldout);

            var fieldsIO = new List<List<Field>> { myFunction.Inputs, myFunction.Outputs };

            foreach (var fields in fieldsIO.Where(field => field.Count != 0 ||
                                                           (myFunction.AllowAddInputs && field == myFunction.Inputs) ||
                                                           (myFunction.AllowAddOutputs && field == myFunction.Outputs)))
            {
                var name = fields == myFunction.Outputs ? "Outputs" : "Inputs";

                parametersContainer.Add(
                    new Label(name)
                    {
                        style =
                        {
                            marginBottom = 5,
                            width = 35
                        }
                    }
                );

                var fieldContainer = new VisualElement
                {
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5
                    }
                };

                foreach (var field in fields)
                    fieldContainer.Add(GetFunctionField(
                            property.FindPropertyRelative(name).GetArrayElementAtIndex(fields.IndexOf(field)),
                            field,
                            myFunction
                        )
                    );

                parametersContainer.Add(fieldContainer);

                if ((fields == myFunction.Inputs && !myFunction.AllowAddInputs) ||
                    (fields == myFunction.Outputs && !myFunction.AllowAddOutputs)) continue;

                var addButton = new Button(() =>
                {
                    fields.Add(myFunction.CreateNewField(fields == myFunction.Inputs));
                    FormulaCache.Clear();

                    Refresh();
                })
                {
                    text = "Add " + (fields == myFunction.Inputs ? "input" : "output"),
                    style =
                    {
                        width = 80,
                        left = 0,
                        marginBottom = 5
                    }
                };

                parametersContainer.Add(addButton);
            }

            foreach (var exposedProperty in myFunction.EditableAttributes)
            {
                var exposedPropertyField = property.FindPropertyRelative(exposedProperty);
                var propertyField = new PropertyField(exposedPropertyField)
                {
                    label = ObjectNames.NicifyVariableName(exposedProperty),
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5
                    }
                };
                propertyField.Bind(property.serializedObject);
                parametersContainer.Add(propertyField);
            }

            container.Add(parametersContainer);

            if (!foldoutOpen.boolValue)
                for (var i = currentIndex; i < container.Children().Count(); i++)
                    container.Children().ElementAt(i).style.display = DisplayStyle.None;

            return container;
        }

        private VisualElement GetFunctionField(SerializedProperty property, Field field, Function function)
        {
            var container = new Box
            {
                style =
                {
                    marginBottom = 3,
                    marginTop = 3,
                    marginRight = 5
                }
            };
            var row1 = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    marginLeft = 5,
                    marginBottom = 5,
                    marginTop = 5
                }
            };
            var row2 = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    marginBottom = 5,
                    marginLeft = 15
                }
            };

            if (field.InEdition)
            {
                var textField = new TextField
                {
                    value = field.EditValue,
                    style =
                    {
                        width = 100,
                        marginRight = 5
                    }
                };

                textField.RegisterCallback<InputEvent>(evt =>
                {
                    var newValue = evt.newData;
                    if (newValue.Any(c => !char.IsLetter(c)))
                        textField.value = new string(textField.value.Where(char.IsLetter).ToArray());
                });

                textField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue.Any(c => !char.IsLetter(c))) return;

                    field.EditValue = textField.value;
                });

                textField.RegisterCallback<KeyUpEvent>(evt =>
                {
                    if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;

                    function.EditField(field.FieldName, field.EditValue);
                    Refresh();
                });

                row1.Add(textField);
            }
            else
            {
                row1.Add(new Label(field.FieldName));
            }

            var isEditable = function.IsFieldEditable(field);

            if (field.InEdition && isEditable)
            {
                var stopButton = new Button(() =>
                {
                    function.EditField(field.FieldName, field.EditValue);
                    Refresh();
                })
                {
                    text = "✓",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                row1.Add(stopButton);
            }
            else if (isEditable)
            {
                var editButton = new Button(() =>
                {
                    field.InEdition = true;
                    field.EditValue = field.FieldName;
                    Refresh();
                })
                {
                    text = "✎",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                row1.Add(editButton);
            }

            var rightValue = 0;
            var settings = function.Inputs.Contains(field)
                ? function.FunctionInputSettings
                : function.FunctionOutputSettings;
            var put = function.Inputs.Contains(field) ? function.Inputs : function.Outputs;

            if ((settings.CanCallMethods && isEditable) || field.AcceptAnyMethod)
            {
                var searchButton = new Button(() =>
                {
                    if (field.AcceptAnyMethod)
                        MethodSearchWindow.ShowWindow(settings.AllowVoidMethods);
                    else
                        MethodSearchWindow.ShowWindow(field.Value.Type, settings.AllowVoidMethods);
                })
                {
                    text = "🔍",
                    tooltip = "Search methods and fields available for this",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                row1.Add(searchButton);
            }

            if (function.IsFieldEditable(field))
            {
                var removeButton = new Button(() =>
                {
                    put.Remove(field);
                    FormulaCache.Clear();
                    Refresh();
                })
                {
                    text = "-",
                    style =
                    {
                        position = Position.Absolute,
                        width = 20,
                        height = 20,
                        right = rightValue
                    }
                };

                row1.Add(removeButton);
                rightValue += 20;

                var index = put.IndexOf(field);

                if (index != function.GetMinEditableFieldIndex(field))
                {
                    var upButton = new Button(() =>
                    {
                        // Move the field up in the list
                        put.Remove(field);
                        put.Insert(index - 1, field);
                        Refresh();
                    })
                    {
                        text = "↑",
                        style =
                        {
                            position = Position.Absolute,
                            width = 20,
                            height = 20,
                            right = rightValue
                        }
                    };

                    row1.Add(upButton);
                    rightValue += 20;
                }

                if (index != put.Count - 1)
                {
                    var downButton = new Button(() =>
                    {
                        // Move the field down in the list
                        put.Remove(field);
                        put.Insert(index + 1, field);
                        Refresh();
                    })
                    {
                        text = "↓",
                        style =
                        {
                            position = Position.Absolute,
                            width = 20,
                            height = 20,
                            right = rightValue
                        }
                    };

                    row1.Add(downButton);
                }
            }

            if (field.Value is not null)
            {
                var type = field.Value.Type;

                if (type == null)
                {
                    row1.Add(new Label("Type not found"));

                    container.Add(row1);
                    container.Add(row2);

                    return container;
                }

                if (field.SupportedTypes.Count == 0) row1.Add(new Label($"({GetBetterTypeName(type)})"));

                var value = property.FindPropertyRelative("Value");
                var propertyField = new PropertyField(value)
                {
                    label = "",
                    style =
                    {
                        flexGrow = 1,
                        marginRight = 5
                    }
                };

                propertyField.RegisterValueChangeCallback(_ =>
                {
                    property.serializedObject.ApplyModifiedProperties();
                });

                propertyField.Bind(property.serializedObject);
                row2.Add(propertyField);

                var parentPath = Regex.Replace(AssetDatabase.GetAssetPath(property.serializedObject.targetObject),
                    "/[^/]*$", "");

                if (parentPath == "") parentPath = ReferenceUtility.PathToVariables;

                var searchPath = parentPath + "/" + property.serializedObject.targetObject.name;

                // Get the variable type of the field (Reference type)
                var variableType = ReferenceUtility.GetVariableFromReference(field.Value.GetType());

                if (variableType == null)
                {
                    row1.Add(new Label("Variable type not found"));

                    container.Add(row1);
                    container.Add(row2);

                    return container;
                }

                // Search all asset files in the parent path and subdirectories with the same type as the field
                var assets = AssetDatabase.FindAssets($"t:{variableType.Name}",
                    new[] { searchPath, ReferenceUtility.PathToGlobalVariables, ReferenceUtility.PathToVariables });

                // Display a dropdown with all the assets found and an option to create a new one (the first option)
                var assetPaths = assets.Select(asset => AssetDatabase.GUIDToAssetPath(asset)
                    .Replace($"{ReferenceUtility.PathToGlobalVariables}/", "")
                    .Replace($"{ReferenceUtility.PathToVariables}/", "")
                    .Replace($"{searchPath}/", "")
                ).ToList();

                assetPaths.Insert(0, "Asset..");

                var buttonCreate = new Button(() =>
                {
                    // Asset creation
                    // Create a folder of the function name if it doesn't exist
                    var folderPath = $"{parentPath}/{property.serializedObject.targetObject.name}";

                    if (!AssetDatabase.IsValidFolder(folderPath))
                        AssetDatabase.CreateFolder(parentPath, property.serializedObject.targetObject.name);

                    var assetName = $"{function.GetType().Name}-{field.FieldName}";

                    field.Value.Value = ReferenceUtility.CreateVariableAsset(type, assetName, folderPath);

                    Refresh();
                })
                {
                    text = "+",
                    tooltip = "Create a new asset",
                    style =
                    {
                        width = 20
                    }
                };

                var popupField = new PopupField<string>(assetPaths, 0)
                {
                    style =
                    {
                        marginRight = 5
                    },
                    tooltip = "Select an asset"
                };

                popupField.RegisterValueChangedCallback(_ =>
                {
                    if (popupField.index == 0) return;

                    field.Value.Value =
                        AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[popupField.index - 1]),
                            variableType);

                    popupField.index = 0;
                });

                row2.Add(buttonCreate);
                row2.Add(popupField);
            }

            if (field.SupportedTypes.Count > 0)
            {
                var baseList = new List<string>();
                var defaultIndex = 0;

                if (field.Value is null)
                    baseList.Add("None");
                else
                    defaultIndex = field.SupportedTypes.IndexOf(field.Value.GetType());

                var supportedTypes = baseList
                    .Concat(field.SupportedTypes.Select(type =>
                        ObjectNames.NicifyVariableName(type.Name).Replace(" Reference", "")))
                    .ToList();
                var popupField = new PopupField<string>(supportedTypes, defaultIndex)
                {
                    style =
                    {
                        marginRight = 5
                    }
                };

                popupField.RegisterValueChangedCallback(evt =>
                {
                    if (field.Value is null && popupField.index == 0) return;

                    var index = popupField.index;

                    if (field.Value is null) index--;

                    var selectedType = field.SupportedTypes[index];
                    field.Value = (IValue)Activator.CreateInstance(selectedType);
                    FormulaCache.Clear();
                    Refresh();
                });

                row1.Add(popupField);
            }

            container.Add(row1);
            if (row2.childCount > 0) container.Add(row2);

            return container;
        }

        private static string GetBetterTypeName(Type type)
        {
            if (type == typeof(float)) return "Float";
            if (type == typeof(int)) return "Integer";
            if (type == typeof(bool)) return "Boolean";
            if (type == typeof(string)) return "String";

            return type.Name;
        }

        private static string RenameTypes(string str)
        {
            return str
                .Replace("Int32", "Integer")
                .Replace("Single", "Float");
        }
    }
}
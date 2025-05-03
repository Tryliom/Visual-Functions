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
        
        private static Field _copiedField;
        private static Function _copiedFunction = null;

        private void Refresh()
        {
            // Used for game objects
            if (_targetObject && PrefabUtility.IsPartOfPrefabInstance(_targetObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(_targetObject);
            }

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

            // Put an alert if play mode is active and on a game object
            if (Application.isPlaying)
            {
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
            }

            var foldoutOpen = property.FindPropertyRelative("FoldoutOpen");
            var isFoldoutOpen = foldoutOpen is { boolValue: true };
            var topRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    marginLeft = 3
                }
            };

            topRow.Add(new Label(property.displayName));

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

                var reloadButton = new Button(Refresh)
                {
                    text = "Reload",
                    style =
                    {
                        marginTop = 5,
                        marginBottom = 5
                    }
                };

                container.Add(topRow);
                container.Add(reloadButton);

                return;
            }
            
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
            }

            if (currentObject is not Functions functionsInstance)
            {
                Debug.LogError("Failed to cast the resolved object to 'Functions'");
                return;
            }
            
            var functionsProperty = property.FindPropertyRelative("FunctionsList");
            var play = EditorGUIUtility.IconContent("d_PlayButton").image;
            var launchButton = new Button(() =>
            {
                functionsInstance.Invoke();
            })
            {
                tooltip = "Run the function",
                style =
                {
                    width = 27,
                    height = 20,
                    position = Position.Absolute,
                    top = 0,
                    right = 0
                }
            };
            
            launchButton.Add(new Image
            {
                image = play,
                style =
                {
                    width = 15,
                    height = 15,
                    marginTop = 1
                }
            });
            
            topRow.Add(launchButton);
            
            var selectFunctionButton = new Button(() =>
            {
                FunctionSelectionWindow.ShowWindow(selectedFunction =>
                {
                    functionsProperty.arraySize++;
                    var functionProperty = functionsProperty.GetArrayElementAtIndex(functionsProperty.arraySize - 1);

                    selectedFunction?.GenerateFields();
                    functionProperty.managedReferenceValue = selectedFunction;

                    Refresh();
                });
            })
            {
                text = "Add Function",
                tooltip = "Open a window to select a function"
            };

            topRow.Add(selectFunctionButton);

            if (_copiedFunction != null)
            {
                var pasteButton = new Button(() =>
                {
                    functionsProperty.arraySize++;
                    var functionProperty = functionsProperty.GetArrayElementAtIndex(functionsProperty.arraySize - 1);
                    
                    functionProperty.managedReferenceValue = _copiedFunction.Clone();
                    
                    _copiedFunction = null;
                    FormulaCache.Clear();
                    Refresh();
                })
                {
                    text = "📋",
                    tooltip = "Paste the function",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };
                
                topRow.Add(pasteButton);
            }

            container.Add(topRow);

            var currentIndex = container.Children().Count() + 1;
            var foldout = new Foldout
            {
                value = isFoldoutOpen,
                style =
                {
                    position = Position.Absolute,
                    top = 3,
                }
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("FoldoutOpen").boolValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();

                for (var i = currentIndex; i < container.Children().Count(); i++)
                {
                    container.Children().ElementAt(i).style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            container.Add(foldout);
            
            if (functionsInstance.AllowGlobalVariables)
            {
                var globalValuesProperty = property.FindPropertyRelative("GlobalVariables");
                var globalValuesContainer = new VisualElement
                {
                    style =
                    {
                        marginTop = 5
                    }
                };
                var globalValuesTopRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center
                    }
                };
                
                globalValuesTopRow.Add(
                    new Label("Global Values")
                    {
                        style =
                        {
                            marginLeft = 17
                        }
                    }
                );
                
                var descriptionImage = new Image
                {
                    tooltip = "Global values are available for all functions, use their name in formulas",
                    style =
                    {
                        position = Position.Absolute,
                        width = 15,
                        height = 15,
                        top = 3,
                        left = 0
                    },
                    image = EditorGUIUtility.IconContent("console.infoicon").image
                };

                globalValuesTopRow.Add(descriptionImage);
                
                var addValueButton = new Button(() =>
                {
                    functionsInstance.GlobalVariables.Add(new Field("" + (char)('a' + functionsInstance.GlobalVariables.Count)));
                    FormulaCache.Clear();
                    Refresh();
                })
                {
                    text = "+",
                    tooltip = "Add a new global value",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                globalValuesTopRow.Add(addValueButton);
                
                if (_copiedField != null)
                {
                    var pasteButton = new Button(() =>
                    {
                        functionsInstance.GlobalVariables.Add(_copiedField.Clone());
                        _copiedField = null;
                        FormulaCache.Clear();
                        Refresh();
                    })
                    {
                        text = "📋",
                        tooltip = "Paste the field",
                        style =
                        {
                            width = 20,
                            height = 20
                        }
                    };
                    
                    globalValuesTopRow.Add(pasteButton);
                }
                
                globalValuesContainer.Add(globalValuesTopRow);
                
                var globalFieldsContainer = new VisualElement
                {
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5,
                        marginTop = 10
                    }
                };

                foreach (var field in functionsInstance.GlobalVariables)
                {
                    globalFieldsContainer.Add(
                        GetFunctionField(
                            globalValuesProperty.GetArrayElementAtIndex(functionsInstance.GlobalVariables.IndexOf(field)),
                            field, functionsInstance.GetType().Name, true, new FunctionSettings().AllowMethods(true),
                            functionsInstance.GlobalVariables, 0,
                            (previousName, newName) => functionsInstance.EditField(previousName, newName)
                        )
                    );
                }
                
                globalValuesContainer.Add(globalFieldsContainer);
                container.Add(globalValuesContainer);
            }

            for (var i = 0; i < functionsProperty.arraySize; i++)
            {
                var functionProperty = functionsProperty.GetArrayElementAtIndex(i);
                var functionVisual = new VisualElement();

                functionVisual.Add(GetFunction(functionProperty));

                var topRightY = functionVisual.layout.y + 8;
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
                        top = topRightY,
                        right = 3
                    }
                };

                functionVisual.Add(removeButton);

                var rightValue = 23 + 3;
                
                var copyButton = new Button(() =>
                {
                    _copiedFunction = (functionProperty.managedReferenceValue as Function)?.Clone();
                    Refresh();
                })
                {
                    text = "📋",
                    tooltip = "Copy the function",
                    style =
                    {
                        position = Position.Absolute,
                        width = 20,
                        height = 20,
                        top = topRightY,
                        right = rightValue
                    }
                };
            
                functionVisual.Add(copyButton);
                
                rightValue += 23;

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
                            top = topRightY,
                            right = rightValue
                        }
                    };

                    functionVisual.Add(upButton);
                    rightValue += 23;
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
                            top = topRightY,
                            right = rightValue
                        }
                    };

                    functionVisual.Add(downButton);
                }

                container.Add(functionVisual);
            }

            if (isFoldoutOpen) return;

            for (var i = currentIndex; i < container.Children().Count(); i++)
            {
                container.Children().ElementAt(i).style.display = DisplayStyle.None;
            }
        }

        private VisualElement GetFunction(SerializedProperty property)
        {
            var container = new VisualElement()
            {
                style =
                {
                    marginLeft = 5 + 10,
                    marginTop = 5
                }
            };
            var foldoutOpen = property.FindPropertyRelative("FoldoutOpen");
            var isEnabled = property.FindPropertyRelative("Enabled").boolValue;
            var refValue = property.managedReferenceValue;
            var functionName = refValue.GetType().GetField("Name").GetValue(refValue).ToString();
            
            if (refValue is not Function myFunction) return container;
            
            const int radius = 6;
            var topLeftRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    height = 30,
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius
                }
            };
            
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
                {
                    container.Children().ElementAt(i).style.display = foldoutOpen.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            topLeftRow.Add(foldout);
            
            // Add checkbox to enable or disable the function
            var toggle = new Toggle
            {
                value = isEnabled,
                style =
                {
                    width = 20,
                    height = 20
                }
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("Enabled").boolValue = evt.newValue;
                Refresh();
            });

            topLeftRow.Add(toggle);

            var description = myFunction.GetType().GetField("Description").GetValue(myFunction).ToString();
            var descriptionImage = new Image
            {
                tooltip = description,
                style =
                {
                    width = 15,
                    height = 15,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                },
                image = EditorGUIUtility.IconContent("console.infoicon").image
            };

            topLeftRow.Add(descriptionImage);
            
            var displayNameField = new PropertyField(property.FindPropertyRelative("DisplayName"))
            {
                label = ""
            };

            displayNameField.RegisterCallback<FocusOutEvent>(evt =>
            {
                Refresh();
            });

            topLeftRow.Add(displayNameField);
            
            if (myFunction.DisplayName != myFunction.GetType().Name)
            {
                topLeftRow.Add(new Label($"({functionName})")
                {
                    style =
                    {
                        marginLeft = 10
                    }
                });
            }
            
            container.Add(topLeftRow);

            var borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            var functionContent = new Box
            {
                style =
                {
                    marginLeft = 5,
                    paddingLeft = 10,
                    marginRight = 5,
                    paddingRight = 10,
                    marginBottom = 5,
                    paddingTop = 5,
                    minHeight = 24,
                    backgroundColor = isEnabled
                        ? new Color(0.25f, 0.25f, 0.25f, 1f)
                        : new Color(0.1f, 0.1f, 0.1f, 0.2f),
                    borderLeftColor = borderColor,
                    borderLeftWidth = 2,
                    borderRightColor = borderColor,
                    borderRightWidth = 2,
                    borderBottomColor = borderColor,
                    borderBottomWidth = 2,
                    borderTopWidth = 3,
                    borderTopColor = new Color(0.18f, 0.18f, 0.18f, 1f),
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius
                }
            };
            var fieldsIO = new List<List<Field>> { myFunction.Inputs, myFunction.Outputs };

            foreach (var fields in fieldsIO.Where(field => field.Count != 0 ||
                                                           (myFunction.AllowAddInputs && field == myFunction.Inputs) ||
                                                           (myFunction.AllowAddOutputs && field == myFunction.Outputs)))
            {
                var name = fields == myFunction.Outputs ? "Outputs" : "Inputs";
                var topRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center,
                        marginBottom = 5
                    }
                };
                
                topRow.Add(
                    new Label(name)
                    {
                        style =
                        {
                            width = 40
                        }
                    }
                );
                
                if ((fields == myFunction.Inputs && myFunction.AllowAddInputs) ||
                    (fields == myFunction.Outputs && myFunction.AllowAddOutputs))
                {
                    var addButton = new Button(() =>
                    {
                        fields.Add(myFunction.CreateNewField(fields == myFunction.Inputs));
                        FormulaCache.Clear();

                        Refresh();
                    })
                    {
                        text = "+",
                        tooltip = "Add a new field",
                        style =
                        {
                            width = 20,
                            height = 20
                        }
                    };

                    topRow.Add(addButton);

                    if (_copiedField != null)
                    {
                        var pasteButton = new Button(() =>
                        {
                            fields.Add(_copiedField.Clone());
                            _copiedField = null;
                            FormulaCache.Clear();
                            Refresh();
                        })
                        {
                            text = "📋",
                            tooltip = "Paste the field",
                            style =
                            {
                                width = 20,
                                height = 20
                            }
                        };

                        topRow.Add(pasteButton);
                    }
                }

                functionContent.Add(topRow);

                var fieldContainer = new VisualElement
                {
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5
                    }
                };

                foreach (var field in fields)
                {
                    fieldContainer.Add(
                        GetFunctionField(
                            property.FindPropertyRelative(name).GetArrayElementAtIndex(fields.IndexOf(field)),
                            field, myFunction.GetType().Name, myFunction.IsFieldEditable(field), 
                            myFunction.Inputs.Contains(field) ? myFunction.FunctionInputSettings : myFunction.FunctionOutputSettings,
                            fields, myFunction.GetMinEditableFieldIndex(field),
                            (fieldName, fieldValue) => myFunction.EditField(fieldName, fieldValue)
                        )
                    );
                }

                functionContent.Add(fieldContainer);
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
                functionContent.Add(propertyField);
            }

            container.Add(functionContent);

            if (!foldoutOpen.boolValue)
            {
                for (var i = currentIndex; i < container.Children().Count(); i++)
                {
                    container.Children().ElementAt(i).style.display = DisplayStyle.None;
                }
            }

            return container;
        }
        
        private VisualElement GetFunctionField(SerializedProperty property, Field field, 
            string functionName, bool isEditable, FunctionSettings settings, List<Field> fields, int minEditableFieldIndex, Action<string, string> editFieldAction)
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
                    {
                        textField.value = new string(textField.value.Where(char.IsLetter).ToArray());
                    }
                });

                textField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue.Any(c => !char.IsLetter(c))) return;

                    field.EditValue = textField.value;
                });

                textField.RegisterCallback<KeyUpEvent>(evt =>
                {
                    if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;
                    
                    if (!ExpressionUtility.IsFieldNameValid(field.EditValue))
                    {
                        Debug.LogError($"Invalid field name: {field.EditValue}");
                        return;
                    }

                    editFieldAction(field.FieldName, field.EditValue);
                    Refresh();
                });

                row1.Add(textField);
            }
            else
            {
                row1.Add(new Label(field.FieldName));
            }

            if (isEditable || field.AllowRename)
            {
                if (field.InEdition)
                {
                    var stopButton = new Button(() =>
                    {
                        if (!ExpressionUtility.IsFieldNameValid(field.EditValue))
                        {
                            Debug.LogError($"Invalid field name: {field.EditValue}");
                            return;
                        }
                        
                        editFieldAction(field.FieldName, field.EditValue);
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
                else
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
            }

            var rightValue = 0;

            if (field.Value != null && field.Value.Type != typeof(object) && 
                ((settings.CanCallMethods && isEditable) || field.AcceptAnyMethod))
            {
                var searchButton = new Button(() =>
                {
                    if (field.AcceptAnyMethod) MethodSearchWindow.ShowWindow(settings.AllowVoidMethods);
                    else MethodSearchWindow.ShowWindow(field.Value.Type, settings.AllowVoidMethods);
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
            
            if (field.Value != null)
            {
                // Add a button to copy the field
                var copyButton = new Button(() =>
                {
                    _copiedField = field.Clone();
                    Refresh();
                })
                {
                    text = "📋",
                    tooltip = "Copy the field name and value",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                row1.Add(copyButton);
            }

            if (isEditable)
            {
                var removeButton = new Button(() =>
                {
                    fields.Remove(field);
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
                rightValue += 23;

                var index = fields.IndexOf(field);

                if (index != minEditableFieldIndex)
                {
                    var upButton = new Button(() =>
                    {
                        // Move the field up in the list
                        fields.Remove(field);
                        fields.Insert(index - 1, field);
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
                    rightValue += 23;
                }

                if (index != fields.Count - 1)
                {
                    var downButton = new Button(() =>
                    {
                        // Move the field down in the list
                        fields.Remove(field);
                        fields.Insert(index + 1, field);
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

                var parentPath = Regex.Replace(AssetDatabase.GetAssetPath(property.serializedObject.targetObject), "/[^/]*$", "");

                if (parentPath == "") parentPath = ReferenceUtility.PathToVariables;

                var searchPath = parentPath + "/" + property.serializedObject.targetObject.name;

                // Get the variable type of the field (Reference type)
                var variableType = ReferenceUtility.GetVariableFromReference(field.Value.GetType());

                if (variableType != null)
                {
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

                        var assetName = $"{functionName}-{field.FieldName}";

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

                        field.Value.Value = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[popupField.index - 1]), variableType);

                        popupField.index = 0;
                    });

                    row2.Add(buttonCreate);
                    row2.Add(popupField);
                }
            }

            if (field.SupportedTypes.Count > 0)
            {
                var baseList = new List<string>();
                var defaultIndex = 0;

                if (field.Value is null)
                {
                    baseList.Add("None");
                }
                else
                {
                    defaultIndex = field.SupportedTypes.IndexOf(field.Value.GetType());
                }

                var supportedTypes = baseList.Concat(field.SupportedTypes.Select(type => ObjectNames.NicifyVariableName(type.Name).Replace(" Reference", ""))).ToList();
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
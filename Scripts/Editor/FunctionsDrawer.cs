using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualFunctions
{
    [CustomPropertyDrawer(typeof(Functions))]
    public class FunctionsDrawer : PropertyDrawer
    {
        private VisualElement _content;
        private SerializedProperty _property;
        private GameObject _targetObject;
        
        public static Field CopiedField;
        public static Function CopiedFunction;

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
                    borderBottomWidth = 2,
                    borderBottomColor = new Color(0.15f, 0.15f, 0.15f, 1f),
                    borderTopWidth = 2,
                    borderTopColor = new Color(0.15f, 0.15f, 0.15f, 1f),
                    marginBottom = 5,
                    paddingBottom = 3,
                    paddingTop = 3
                }
            };
            
            var currentIndex = container.Children().Count() + 1;
            var foldoutButton = new Button(() =>
            {
                foldoutOpen.boolValue = !foldoutOpen.boolValue;
                property.serializedObject.ApplyModifiedProperties();
                Refresh();
            })
            {
                text = "≡",
                style =
                {
                    width = 20,
                    height = 20
                }
            };
            
            topRow.Add(foldoutButton);
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

            if (PropertyDrawerUtility.RetrieveTargetObject(_property) is not Functions functionsInstance)
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
                    top = 3,
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
                    foldoutOpen.boolValue = true;

                    Refresh();
                });
            })
            {
                text = "Add Function",
                tooltip = "Open a window to select a function"
            };

            topRow.Add(selectFunctionButton);

            if (CopiedFunction != null)
            {
                var pasteButton = new Button(() =>
                {
                    functionsProperty.arraySize++;
                    var functionProperty = functionsProperty.GetArrayElementAtIndex(functionsProperty.arraySize - 1);
                    
                    functionProperty.managedReferenceValue = CopiedFunction.Clone();
                    
                    CopiedFunction = null;
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
            
            if (!foldoutOpen.boolValue) return;
            
            if (functionsInstance.AllowImport)
            {
                var borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                const int radius = 6;
                var importedFieldsBox = new VisualElement
                {
                    style =
                    {
                        marginLeft = 5,
                        paddingLeft = 10,
                        marginRight = 5,
                        paddingRight = 10,
                        marginBottom = 5,
                        minHeight = 24,
                        backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f),
                        borderLeftColor = borderColor,
                        borderLeftWidth = 2,
                        borderRightColor = borderColor,
                        borderRightWidth = 2,
                        borderBottomColor = borderColor,
                        borderBottomWidth = 2,
                        borderBottomLeftRadius = radius,
                        borderBottomRightRadius = radius
                    }
                };
                var importTopRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center,
                        backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                        height = 30,
                        paddingLeft = 5,
                        marginBottom = 5,
                        borderTopLeftRadius = radius,
                        borderTopRightRadius = radius,
                        borderBottomLeftRadius = radius,
                        borderBottomRightRadius = radius
                    }
                };

                if (functionsInstance.ImportedFields.Count == 0)
                {
                    functionsInstance.ImportedFieldsFoldoutOpen = false;
                }

                var foldoutImportButton = new Button(() =>
                {
                    functionsInstance.ImportedFieldsFoldoutOpen = !functionsInstance.ImportedFieldsFoldoutOpen;
                    Refresh();
                })
                {
                    text = "≡",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                importTopRow.Add(foldoutImportButton);
                importTopRow.Add(new Label("Imported Fields"));

                var descriptionImage = new Image
                {
                    tooltip = "Imported fields are available for all functions, use their name in formulas",
                    style =
                    {
                        marginTop = 2
                    },
                    image = EditorGUIUtility.IconContent("_Help").image
                };

                importTopRow.Add(descriptionImage);

                var importButton = new Button(() =>
                {
                    ExpressionUtility.DisplayAssetPathMenuForType(
                        typeof(ExportableFields),
                        property.serializedObject.targetObject,
                        asset =>
                        {
                            functionsInstance.ImportedFields.Add(new ImportedFields(asset as ExportableFields));
                            functionsInstance.ImportedFieldsFoldoutOpen = true;
                            Refresh();
                        },
                        functionsInstance.ImportedFields.Select(x => x.Value as Object).ToList()
                    );
                })
                {
                    text = "Import..",
                    tooltip = "Import exportable fields from another object",
                    style =
                    {
                        marginTop = 5,
                        marginBottom = 5
                    }
                };

                importTopRow.Add(importButton);
                container.Add(importTopRow);

                if (functionsInstance.ImportedFieldsFoldoutOpen)
                {
                    foreach (var importedFields in functionsInstance.ImportedFields)
                    {
                        var row = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                justifyContent = Justify.FlexStart,
                                alignItems = Align.Center,
                                backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                                height = 30,
                                paddingLeft = 5,
                                marginTop = 10,
                                marginBottom = 10,
                                borderTopLeftRadius = radius,
                                borderTopRightRadius = radius,
                                borderBottomLeftRadius = radius,
                                borderBottomRightRadius = radius
                            }
                        };

                        var importFoldoutOpenButton = new Button(() =>
                        {
                            importedFields.FoldoutOpen = !importedFields.FoldoutOpen;
                            Refresh();
                        })
                        {
                            text = "≡",
                            style =
                            {
                                width = 20,
                                height = 20
                            }
                        };

                        row.Add(importFoldoutOpenButton);
                        row.Add(new Label(ObjectNames.NicifyVariableName(importedFields.Value.name)));
                        row.Add(new Button(() =>
                        {
                            functionsInstance.ImportedFields.Remove(importedFields);
                            FormulaCache.Clear();
                            Refresh();
                        })
                        {
                            text = "-",
                            style =
                            {
                                width = 20,
                                height = 20
                            }
                        });

                        importedFieldsBox.Add(row);

                        var listContent = new VisualElement
                        {
                            style =
                            {
                                marginLeft = 10,
                                marginTop = 5,
                                marginBottom = 5,
                            }
                        };

                        var fieldsContent = new VisualElement
                        {
                            style =
                            {
                                marginLeft = 10,
                                marginTop = 5,
                                marginBottom = 5,
                            }
                        };

                        foreach (var field in importedFields.Value.Fields)
                        {
                            if (field.Value is CustomFunction customFunction)
                            {
                                fieldsContent.Add(new Label(ExpressionUtility.FormatCustomFunction(field.FieldName, customFunction)));
                            }
                            else
                            {
                                var fieldContent = new VisualElement
                                {
                                    style =
                                    {
                                        flexDirection = FlexDirection.Row,
                                        justifyContent = Justify.FlexStart,
                                        alignItems = Align.Center
                                    }
                                };
                                
                                fieldContent.Add(new Label(field.FieldName + " (" + ExpressionUtility.GetBetterTypeName(field.Value.Type) + ")"));
                                
                                if (field.Value != null && field.Value.Type != typeof(object))
                                {
                                    var searchButton = new Button(() =>
                                    {
                                        if (field.AcceptAnyMethod) MethodSearchWindow.ShowWindow(true);
                                        else MethodSearchWindow.ShowWindow(field.Value.Type, true);
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

                                    fieldContent.Add(searchButton);
                                }
                                
                                fieldsContent.Add(fieldContent);
                            }
                        }

                        if (importedFields.FoldoutOpen)
                        {
                            listContent.Add(new Label(importedFields.Value.DeveloperDescription));
                            listContent.Add(fieldsContent);
                            importedFieldsBox.Add(listContent);
                        }
                    }

                    container.Add(importedFieldsBox);
                }
            }
            
            if (functionsInstance.AllowGlobalVariables)
            {
                functionsInstance.ValidateGlobalVariables();
                
                var globalValuesFoldout = property.FindPropertyRelative("GlobalValuesFoldoutOpen");
                var globalValuesProperty = property.FindPropertyRelative("GlobalVariables");
                
                CreateFields(
                    container, "Global Values", "Global values are available for all functions, use their name in formulas",
                    functionsInstance.GlobalVariables, globalValuesFoldout,
                    () =>
                    {
                        functionsInstance.GlobalVariables.Add(new Field(GlobalSettings.Settings.GlobalValuesPrefix + (char)('a' + functionsInstance.GlobalVariables.Count)));
                        functionsInstance.GlobalValuesFoldoutOpen = true;
                        FormulaCache.Clear();
                        Refresh();
                    },
                    () =>
                    {
                        functionsInstance.GlobalVariables.Add(CopiedField.Clone());
                        CopiedField = null;
                        FormulaCache.Clear();
                        Refresh();
                    },
                    (element, field) =>
                    {
                        element.Add(
                            CreateField(
                                globalValuesProperty.GetArrayElementAtIndex(functionsInstance.GlobalVariables.IndexOf(field)),
                                field, functionsInstance.GetType().Name, true, new FunctionSettings().AllowMethods(true),
                                functionsInstance.GlobalVariables, 0,
                                (previousName, newName) => functionsInstance.EditField(previousName, newName),
                                Refresh
                            )
                        );
                    },
                    Refresh
                );
            }

            for (var i = 0; i < functionsProperty.arraySize; i++)
            {
                var functionProperty = functionsProperty.GetArrayElementAtIndex(i);
                var functionVisual = new VisualElement();

                functionVisual.Add(CreateFunction(functionProperty, Refresh));

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
                    CopiedFunction = (functionProperty.managedReferenceValue as Function)?.Clone();
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

        public static VisualElement CreateFunction(SerializedProperty property, Action refresh)
        {
            var container = new VisualElement
            {
                style =
                {
                    marginTop = 5
                }
            };
            var foldoutOpen = property.FindPropertyRelative("FoldoutOpen");
            var isEnabled = property.FindPropertyRelative("Enabled").boolValue;
            var refValue = property.managedReferenceValue;
            var functionName = refValue.GetType().GetField("Name").GetValue(refValue).ToString();
            
            if (refValue is not Function myFunction) return container;
            
            var hasContent = myFunction.Inputs.Count > 0 || myFunction.Outputs.Count > 0 || myFunction.AllowAddInputs || myFunction.AllowAddOutputs || myFunction.EditableAttributes.Count > 0;
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
                    paddingLeft = 5,
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius
                }
            };
            
            if (hasContent)
            {
                var foldoutButton = new Button(() =>
                {
                    foldoutOpen.boolValue = !foldoutOpen.boolValue;
                    property.serializedObject.ApplyModifiedProperties();
                    refresh();
                })
                {
                    text = "≡",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                topLeftRow.Add(foldoutButton);
            }
            
            // Add checkbox to enable or disable the function
            var toggle = new Toggle
            {
                value = isEnabled,
                style =
                {
                    width = 20,
                    height = 20,
                    marginTop = 2
                }
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("Enabled").boolValue = evt.newValue;
                refresh();
            });

            topLeftRow.Add(toggle);

            var description = myFunction.GetType().GetField("Description").GetValue(myFunction).ToString();
            var descriptionImage = new Image
            {
                tooltip = description,
                style =
                {
                    marginTop = 2
                },
                image = EditorGUIUtility.IconContent("_Help").image
            };

            topLeftRow.Add(new Label(functionName));
            topLeftRow.Add(descriptionImage);
            
            container.Add(topLeftRow);
            
            if (!hasContent || !foldoutOpen.boolValue) return container;

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
                    paddingBottom = 5,
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
                
                CreateFields(
                    functionContent, name, "", fields, null,
                    () =>
                    {
                        fields.Add(myFunction.CreateNewField(fields == myFunction.Inputs));
                        FormulaCache.Clear();
                        refresh();
                    },
                    () =>
                    {
                        fields.Add(CopiedField.Clone());
                        CopiedField = null;
                        FormulaCache.Clear();
                        refresh();
                    },
                    (element, field) =>
                    {
                        element.Add(
                            CreateField(
                                property.FindPropertyRelative(name).GetArrayElementAtIndex(fields.IndexOf(field)),
                                field, myFunction.GetType().Name, myFunction.IsFieldEditable(field), 
                                myFunction.Inputs.Contains(field) ? myFunction.FunctionInputSettings : myFunction.FunctionOutputSettings,
                                fields, myFunction.GetMinEditableFieldIndex(field),
                                (fieldName, fieldValue) => myFunction.EditField(fieldName, fieldValue),
                                refresh
                            )
                        );
                    },
                    refresh
                );
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

            return container;
        }
        
        public static VisualElement CreateField(SerializedProperty property, Field field, 
            string functionName, bool isEditable, FunctionSettings settings, List<Field> fields, int minEditableFieldIndex, Action<string, string> editFieldAction,
            Action refresh)
        {
            const int borderWidth = 2;
            const float grey = 0.4f;
            var borderColor = new Color(grey, grey, grey, 1f);
            var container = new Box
            {
                style =
                {
                    marginBottom = 3,
                    marginTop = 3,
                    marginRight = 5,
                    borderLeftColor = borderColor,
                    borderLeftWidth = borderWidth,
                    borderRightColor = borderColor,
                    borderRightWidth = borderWidth,
                    borderBottomColor = borderColor,
                    borderBottomWidth = borderWidth,
                    borderTopColor = borderColor,
                    borderTopWidth = borderWidth,
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
            
            var foldout = property.FindPropertyRelative("Value.FoldoutOpen");

            if (foldout != null)
            {
                // Add a foldout button for values that can be expanded
                var foldoutButton = new Button(() =>
                {
                    foldout.boolValue = !foldout.boolValue;
                    refresh();
                })
                {
                    text = "≡",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };
                
                row1.Add(foldoutButton);
            }

            if (field.InEdition)
            {
                var textField = new TextField
                {
                    value = field.EditValue,
                    style =
                    {
                        marginRight = 5
                    }
                };

                textField.RegisterCallback<InputEvent>(evt =>
                {
                    if (evt.newData.Any(c => !char.IsLetter(c) && c != '_'))
                    {
                        textField.value = new string(textField.value.Where(char.IsLetter).ToArray());
                    }
                });

                textField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue.Any(c => !char.IsLetter(c) && c != '_')) return;

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
                    refresh();
                });

                row1.Add(textField);
            }
            else if (field.Value is CustomFunction customFunction)
            {
                row1.Add(new Label(ExpressionUtility.FormatCustomFunction(field.FieldName, customFunction)));
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
                        refresh();
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
                        refresh();
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

            if (field.Value != null && field.Value.Type != typeof(object) && field.Value is not CustomFunction &&
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
                    CopiedField = field.Clone();
                    refresh();
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
                    refresh();
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
                        refresh();
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
                        refresh();
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
            
            if (field.SupportedTypes.Count > 0)
            {
                var baseList = new List<string>();
                var defaultIndex = 0;

                if (field.Value is null)
                {
                    baseList.Add("None");
                }
                
                baseList.AddRange(ExpressionUtility.SupportedTypes.Select(type => ObjectNames.NicifyVariableName(type.Name).Replace(" Reference", "")));

                var supportedTypes = baseList.Concat(field.SupportedTypes
                    .Where(t => !ExpressionUtility.SupportedTypes.Contains(t))
                    .Select(type => ObjectNames.NicifyVariableName(type.Name).Replace(" Reference", "")))
                .ToList();
                
                if (field.Value is not null)
                {       
                    defaultIndex = supportedTypes.IndexOf(ObjectNames.NicifyVariableName(field.Value.GetType().Name).Replace(" Reference", ""));
                }
                
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

                    var selectedType = index < ExpressionUtility.SupportedTypes.Count
                        ? ExpressionUtility.SupportedTypes[index]
                        : field.SupportedTypes.Where(t => !ExpressionUtility.SupportedTypes.Contains(t)).ToList()[index - ExpressionUtility.SupportedTypes.Count].SystemType;
                    field.Value = (IValue)Activator.CreateInstance(selectedType);
                    FormulaCache.Clear();
                    refresh();
                });

                row1.Add(popupField);
            }

            container.Add(row1);

            if (field.Value is not null)
            {
                if (foldout is { boolValue: false }) return container;
                
                var type = field.Value.Value is IRefType ? field.Value.Value.GetType() : field.Value.Type;

                if (field.SupportedTypes.Count == 0) row1.Add(new Label($"({ExpressionUtility.GetBetterTypeName(type)})"));

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

                // Get the variable type of the field (Reference type)
                var variableType = ReferenceUtility.GetVariableFromReference(field.Value.GetType());

                if (variableType != null)
                {
                    var buttonCreate = new Button(() =>
                    {
                        var parentPath = Regex.Replace(AssetDatabase.GetAssetPath(property.serializedObject.targetObject), "/[^/]*$", "");

                        if (parentPath == "") parentPath = GlobalSettings.Settings.PathToVariables;
                        
                        // Asset creation
                        // Create a folder of the function name if it doesn't exist
                        var folderPath = $"{parentPath}/{property.serializedObject.targetObject.name}";

                        if (!AssetDatabase.IsValidFolder(folderPath))
                        {
                            AssetDatabase.CreateFolder(parentPath, property.serializedObject.targetObject.name);
                        }

                        var assetName = $"{functionName}-{field.FieldName}";

                        field.Value.Value = ReferenceUtility.CreateVariableAsset(type, assetName, folderPath);

                        refresh();
                    })
                    {
                        text = "+",
                        tooltip = "Create a new asset",
                        style =
                        {
                            width = 20
                        }
                    };

                    var assetButton = new Button(() =>
                    {
                        ExpressionUtility.DisplayAssetPathMenuForType(variableType, property.serializedObject.targetObject, asset =>
                        {
                            field.Value.Value = asset;
                        },
                        new List<Object>{field.Value.Value as Object});
                    })
                    {
                        text = "Asset..",
                        style =
                        {
                            marginRight = 5
                        },
                        tooltip = "Select an asset"
                    };

                    row2.Add(buttonCreate);
                    row2.Add(assetButton);
                }
            }
            
            if (row2.childCount > 0) container.Add(row2);

            return container;
        }
        
        public static void CreateFields(VisualElement root, string header, string explanations, List<Field> fields, SerializedProperty foldout, 
            Action addField, Action addCopiedField, Action<VisualElement, Field> createField, Action refresh)
        {
            var borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            const int radius = 6;
            var container = foldout == null ?
                new VisualElement
                {
                    style =
                    {
                        marginLeft = 15,
                        marginBottom = 5
                    }
                } : 
                new VisualElement
                {
                    style =
                    {
                        marginLeft = 5,
                        paddingLeft = 10,
                        marginRight = 5,
                        paddingRight = 10,
                        marginBottom = 5,
                        minHeight = 24,
                        backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f),
                        borderLeftColor = borderColor,
                        borderLeftWidth = 2,
                        borderRightColor = borderColor,
                        borderRightWidth = 2,
                        borderBottomColor = borderColor,
                        borderBottomWidth = 2,
                        borderBottomLeftRadius = radius,
                        borderBottomRightRadius = radius
                    }
                };
            var topRow = foldout == null ?
                new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center,
                        marginBottom = 5
                    }
                } :  
                new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center,
                        backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                        height = 30,
                        paddingLeft = 5,
                        borderTopLeftRadius = radius,
                        borderTopRightRadius = radius,
                        borderBottomLeftRadius = radius,
                        borderBottomRightRadius = radius
                    }
                };

            if (fields.Count == 0 && foldout != null)
            {
                foldout.boolValue = false;
            }
            
            if (fields.Count > 0 && foldout != null)
            {
                var foldoutButton = new Button(() =>
                {
                    foldout.boolValue = !foldout.boolValue;
                    refresh();
                })
                {
                    text = "≡",
                    style =
                    {
                        width = 20,
                        height = 20
                    }
                };

                topRow.Add(foldoutButton);
            }
                
            topRow.Add(new Label(header));

            if (explanations.Length > 0)
            {
                var descriptionImage = new Image
                {
                    tooltip = explanations,
                    style =
                    {
                        marginTop = 2
                    },
                    image = EditorGUIUtility.IconContent("_Help").image
                };

                topRow.Add(descriptionImage);
            }
            
            var addValueButton = new Button(addField)
            {
                text = "+",
                tooltip = "Add a new field",
                style =
                {
                    width = 20,
                    height = 20
                }
            };

            topRow.Add(addValueButton);
            
            if (CopiedField != null)
            {
                var pasteButton = new Button(addCopiedField)
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
                
            root.Add(topRow);

            if (foldout is { boolValue: false }) return;
            
            var fieldsContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 15,
                    marginBottom = 5,
                    marginTop = 5
                }
            };

            foreach (var field in fields)
            {
                createField(fieldsContainer, field);
            }

            container.Add(fieldsContainer);
            root.Add(container);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisualFunctions
{
    public static class ExpressionUtility
    {
        private static readonly char[] Operators;

        static ExpressionUtility()
        {
            var operators = " ";

            foreach (var key in Evaluator.Operations.Keys) operators += key[0];

            operators += ",.;\n";

            Operators = operators.ToCharArray();
        }
        
        public static readonly List<Type> PopularTypes = new()
        {
            typeof(int), typeof(float), typeof(double), typeof(bool), typeof(string),
            typeof(long), typeof(short), typeof(byte), typeof(char),
            typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(Quaternion), typeof(Color), typeof(Rect),
            typeof(AnimationCurve), typeof(ScriptableObject), typeof(GameObject),
        };

        // List of types displayed first when choosing a reference type
        public static readonly List<Type> SupportedTypes = new()
        {
            typeof(AnyTypeReference), typeof(ListOfReference), typeof(ComponentOfGameObjectReference),
            typeof(CustomValue), typeof(CustomFunction), typeof(Formula), typeof(ScriptableObjectReference)
        };
        
        private static IEnumerable<Type> SortedTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => 
                !t.IsNested && !t.IsGenericType && !t.IsAbstract && !t.IsSpecialName && !t.IsPointer && !t.IsInterface &&
                !t.IsNotPublic && t.GetConstructors().Length > 0 &&
                (t.IsClass || t.IsEnum || t.IsValueType) &&
                (t.GetProperties().Length > 0 || t.GetFields().Length > 0 || t.GetMethods().Length > 0) &&
                !t.IsSubclassOf(typeof(Exception)) && (t.Namespace == null || !t.Namespace.Contains("UnityEditor"))
            )
            .Except(PopularTypes);
        
        public static List<Type> UnityTypes => SortedTypes
            .Where(t => t.IsSubclassOf(typeof(UnityEngine.Object)))
            .ToList();
        public static List<Type> OtherTypes => SortedTypes
            .Where(t => !t.IsSubclassOf(typeof(UnityEngine.Object)))
            .ToList();

        public static string ExtractVariable(string formula, int index)
        {
            var variableSpan = formula.AsSpan(index);
            var variableEnd = variableSpan.IndexOfAny(Operators);
            if (variableEnd == -1) variableEnd = variableSpan.Length;

            return variableSpan[..variableEnd].ToString();
        }

        /**
         * Extracts a string surrounded by a character at the index in the formula. Does not return the character at the beginning and end.
         */
        public static string ExtractSurrounded(string formula, int index)
        {
            var character = formula[index];
            var variableSpan = formula.AsSpan(index + 1);
            var variableEnd = variableSpan.IndexOf(character);

            if (variableEnd == -1) variableEnd = variableSpan.Length;

            return variableSpan[..variableEnd].ToString();
        }

        /**
         * Extracts a string inside surrounders characters.
         * The index is the index of the opening surrounder. Does not return the surrounders.
         */
        public static string ExtractInsideSurrounder(string formula, int index, char startSurrounder = '(',
            char endSurrounder = ')')
        {
            // Get all the things inside surrounders
            var depth = 0;
            var i = index;
            var startIndex = i + 1;

            while (i < formula.Length)
            {
                if (formula[i] == startSurrounder) depth++;
                if (formula[i] == endSurrounder) depth--;
                if (depth == 0) break;
                i++;
            }

            return formula.Substring(startIndex, i - startIndex);
        }

        /**
         * Extracts string parameters inside brackets or other things. The index is the index of the opening bracket. Does not return the brackets.
         */
        public static List<string> ExtractMethodParameters(string formula, int index, char startSeparator = '(',
            char endSeparator = ')')
        {
            var parameters = new List<string>();
            var parameter = "";
            var depth = 0;
            var i = index + 1;

            while (i < formula.Length)
            {
                if (formula[i] == startSeparator) depth++;

                if (depth == 1 && formula[i] == ',')
                {
                    parameters.Add(parameter);
                    parameter = "";
                }
                else if (depth > 1 || (formula[i] != endSeparator && formula[i] != startSeparator))
                {
                    parameter += formula[i];
                }

                if (formula[i] == endSeparator) depth--;

                if (depth == 0)
                {
                    if (parameter != "") parameters.Add(parameter);
                    parameter = "";
                    break;
                }

                i++;
            }

            if (parameter != "") parameters.Add(parameter);

            return parameters;
        }

        /**
         * Extract a type from a string, the type can be a class, struct, enum or a primitive type.
         * If a property (field or method) is searched, it will search for the type that contains the property.
         */
        public static Type ExtractType(string typeStr, string propertySearched = "")
        {
            var type = Type.GetType(typeStr) ?? typeStr switch
            {
                "int" => typeof(int),
                "float" => typeof(float),
                "double" => typeof(double),
                "bool" => typeof(bool),
                "string" => typeof(string),
                "long" => typeof(long),
                "short" => typeof(short),
                "byte" => typeof(byte),
                "char" => typeof(char),
                "decimal" => typeof(decimal),
                // Special cases for some types
                "List" => typeof(List<>),
                _ => null
            };

            if (type != null) return type;

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => (t.Name == typeStr || t.FullName == typeStr) && !t.IsNested)
                .ToList();

            if (propertySearched == string.Empty) return types.FirstOrDefault();

            foreach (var t in types)
            {
                // Check if the type has the method or property
                var methods = t.GetMethods();
                var method = methods.FirstOrDefault(m => m.Name == propertySearched);
                var field = t.GetField(propertySearched);
                var property = t.GetProperty(propertySearched);

                if (method != null || property != null || field != null) return t;

                // Check if the type is an enum and contains the field
                if (t.IsEnum && Enum.IsDefined(t, propertySearched)) return t;
            }

            return types.FirstOrDefault();
        }
        
        /**
         * Convert a value to a type. Treat special cases like Vector2 and Vector3.
         */
        public static object ConvertTo(object value, Type targetType)
        {
            return value switch
            {
                Vector3 vector3 when targetType == typeof(Vector2) => new Vector2(vector3.x, vector3.y),
                Vector2 vector2 when targetType == typeof(Vector3) => new Vector3(vector2.x, vector2.y, 0),
                _ => Convert.ChangeType(value, targetType)
            };
        }

        public static object ExtractValue(object from, string uid, List<IVariable> variables)
        {
            var loops = 0;
            
            while (true)
            {
                loops++;
                
                if (loops > 100)
                {
                    Debug.LogError($"Infinite loop detected in {nameof(ExtractValue)} from {from}");
                    return null;
                }
                
                switch (from)
                {
                    case CustomValue customValue:
                    {
                        customValue.Evaluate(uid, variables);
                        from = customValue.Value;
                        continue;
                    }
                    case IValue value:
                        if (value.Value is IRefValue refValue)
                        {
                            return refValue.RefValue;
                        }
                        return value.Value;
                    case AccessorCaller caller:
                        var extractValue = caller.Result.Value;
                        if (extractValue != null) return extractValue;
                        from = Evaluator.EvaluateAccessor(uid, caller, variables);
                        continue;
                    case not null:
                        return from;
                }

                break;
            }
            
            return null;
        }
        
        public static bool IsReservedWord(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            return name is "true" or "false" or "null";
        }
        
        public static object GetReservedWordValue(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return name switch
            {
                "true" => true,
                "false" => false,
                _ => null
            };
        }
        
        public static bool IsFieldNameValid(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (ExtractType(name) is not null) return false;

            foreach (var c in name)
            {
                if (!char.IsLetter(c) && c != '_') return false;
            }

            return !IsReservedWord(name);
        }

        public static string GetBetterTypeName(Type type)
        {
            if (type == null) return "null";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(string)) return "string";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(char)) return "char";
            if (type == typeof(decimal)) return "decimal";
            
            if (!type.IsGenericType) return type.Name;
            
            var genericType = type.GetGenericTypeDefinition();
                
            if (genericType == typeof(List<>))
            {
                return $"List<{type.Name}>";
            }
                
            if (genericType == typeof(Dictionary<,>))
            {
                return $"Dictionary<{type.Name}>";
            }

            return type.Name;
        }

        public static string FormatCustomFunction(string fieldName, CustomFunction customFunction)
        {
            var name = "";

            if (customFunction.Outputs.Count > 0 && customFunction.Outputs[0].Value != null)
            {
                name += GetBetterTypeName(customFunction.Outputs[0].Value.Type) + " ";
            }
            else
            {
                name += "void ";
            }
                
            name += fieldName;
                
            if (customFunction.Inputs.Count > 0)
            {
                var inputs = new List<string>();
                    
                foreach (var input in customFunction.Inputs)
                {
                    if (input.Value == null) continue;
                        
                    inputs.Add(GetBetterTypeName(input.Value.Type) + " " + input.FieldName);
                }

                name += "(" + string.Join(", ", inputs) + ")";
            }
            else
            {
                name += "()";
            }

            return name;
        }

        /**
         * Creates a menu to select an asset path for a specific type.
         * The callback is called when the user selects an asset.
         */
        public static void DisplayAssetPathMenuForType(Type assetType, Object targetObject, Action<Object> callback, List<Object> ignoredObjects)
        {
            var searchPaths = new List<string>();
            var targetPath = "";

            if (targetObject)
            {
                targetPath = Regex.Replace(AssetDatabase.GetAssetPath(targetObject), "/[^/]*$", "");

                if (!string.IsNullOrEmpty(targetPath))
                {
                    searchPaths.Add(targetPath);
                }
            }
            
            if (assetType.BaseType is { FullName: not null } && assetType.BaseType.FullName.StartsWith(typeof(Variable<>).FullName))
            {
                searchPaths.Add(GlobalSettings.Settings.PathToGlobalVariables);
                searchPaths.Add(GlobalSettings.Settings.PathToVariables);
            }
            else
            {
                searchPaths.Add(GlobalSettings.Settings.PathToGlobalObjects);
            }
            
            var files = AssetDatabase.FindAssets($"t:{assetType.Name}", searchPaths.ToArray())
                .Where(guid => ignoredObjects.All(obj => AssetDatabase.GUIDToAssetPath(guid) != AssetDatabase.GetAssetPath(obj)))
                .ToArray();
            var genericMenu = new GenericMenu();

            foreach (var file in files)
            {
                var path = AssetDatabase.GUIDToAssetPath(file);
                var name = path.Replace(GlobalSettings.Settings.PathToGlobalVariables + "/", "Global Variables/")
                    .Replace(GlobalSettings.Settings.PathToVariables + "/", "Variables/")
                    .Replace(GlobalSettings.Settings.PathToGlobalObjects + "/", "Global Objects/")
                    .Replace(targetPath + "/", "");
                
                genericMenu.AddItem(new GUIContent(name), false, () =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, assetType);
                    
                    if (!asset) return;
                    
                    callback(asset);
                });
            }
            
            if (files.Length == 0)
            {
                genericMenu.AddDisabledItem(new GUIContent("No assets found"));
            }
            
            genericMenu.ShowAsContext();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualFunctions
{
    public enum FunctionCategory
    {
        Executor,
        Getter,
        Debug,
        Logic,
        Math,
        Utility,
        Setter
    }

    [Serializable]
    public class FunctionSettings
    {
        public List<SerializableSystemType> SupportedTypes = new();
        public string PrefixName;
        public bool CanCallMethods;
        public bool AllowVoidMethods;
        public Type SupportedType;

        public FunctionSettings()
        {
        }

        public FunctionSettings(Type supportedType, string prefixName = "")
        {
            SupportedType = supportedType;
            PrefixName = prefixName;
        }

        public FunctionSettings(List<SerializableSystemType> supportedTypes, string prefixName = "")
        {
            SupportedTypes = supportedTypes;
            PrefixName = prefixName;
        }

        public FunctionSettings(string prefixName)
        {
            PrefixName = prefixName;
        }

        public FunctionSettings AllowMethods(bool allowVoidMethods = false)
        {
            CanCallMethods = true;
            AllowVoidMethods = allowVoidMethods;
            return this;
        }
    }

    [Serializable]
    public abstract class Function
    {
        public static Dictionary<string, FunctionInfo> Functions = new();

#if UNITY_EDITOR
        public bool FoldoutOpen = true;
#endif

        public bool Enabled = true;

        // Only use IValue types
        public List<Field> Inputs = new();

        // Only use IValue types
        public List<Field> Outputs = new();

        // Add the name of the attributes that can be edited in the inspector
        public List<string> EditableAttributes = new();

        public bool AllowAddInputs;
        public FunctionSettings FunctionInputSettings;
        public bool AllowAddOutputs;
        public FunctionSettings FunctionOutputSettings;

        [NonSerialized] private bool _fieldsValidated;

        // Used to differentiate the function in cache (formula cache)
        protected string Uid;

        protected Function()
        {
#if UNITY_EDITOR
            FunctionUtility.RegisterFunction(this);
#endif

            Uid = Guid.NewGuid().ToString();
        }

        public bool Invoke(List<IVariable> variables)
        {
            return !Enabled || Process(variables);
        }

#if UNITY_EDITOR
        /**
         * If you override this method, you need to surround it with a #if UNITY_EDITOR and #endif
         */
        public virtual void GenerateFields()
        {
        }

        public Function Clone()
        {
            var clone = (Function)MemberwiseClone();
            clone.Inputs = new List<Field>(Inputs.Select(input => input.Clone()));
            clone.Outputs = new List<Field>(Outputs.Select(output => output.Clone()));
            clone.EditableAttributes = new List<string>(EditableAttributes);
            clone.Uid = Guid.NewGuid().ToString();
            return clone;
        }
#endif

        protected abstract bool Process(List<IVariable> variables);

        public void EditField(string previousName, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return;

            foreach (var input in Inputs)
            {
                input.OnEditField(previousName, newName);
            }
        }

        /**
         * Allow to add more inputs to the function
         * prefixName: The prefix name of the fields
         */
        protected void AllowAddInput(FunctionSettings settings)
        {
            AllowAddInputs = true;
            FunctionInputSettings = settings;
        }

        /**
         * Allow to add more outputs to the function
         * prefixName: The prefix name of the fields
         */
        protected void AllowAddOutput(FunctionSettings settings)
        {
            AllowAddOutputs = true;
            FunctionOutputSettings = settings;
        }

        /**
         * Get the IValue of an input field by its name, used when you don't care about the type
         */
        protected IValue GetInputValue(string fieldName)
        {
            return Inputs.Find(field => field.FieldName == fieldName).Value;
        }

        /**
         * Get the IValue of an output field by its name, used when you don't care about the type
         */
        protected IValue GetOutputValue(string fieldName)
        {
            return Outputs.Find(field => field.FieldName == fieldName).Value;
        }

        /**
         * Get the value of an input field by its name and cast it to the desired IValue type
         */
        protected IValue<TType> GetInput<TType>(string fieldName)
        {
            return (IValue<TType>)Inputs.Find(field => field.FieldName == fieldName).Value;
        }

        /**
         * Get the value of an output field by its name and cast it to the desired IValue type
         */
        protected IValue<TType> GetOutput<TType>(string fieldName)
        {
            return (IValue<TType>)Outputs.Find(field => field.FieldName == fieldName).Value;
        }

        public class FunctionInfo
        {
            public FunctionCategory Category;
            public string Description;
            public Function Instance;
            public Type Type;
        }

#if UNITY_EDITOR
        public bool IsFieldEditable(Field field)
        {
            if (Functions[GetType().GetField("Name").GetValue(this).ToString()].Instance is not { } instance)
            {
                throw new Exception("The function is not instantiated");
            }

            if (Inputs.Contains(field))
            {
                if (!AllowAddInputs) return false;
                if (instance.Inputs.Count - 1 >= Inputs.IndexOf(field)) return false;
            }
            else if (Outputs.Contains(field))
            {
                if (!AllowAddOutputs) return false;
                if (instance.Outputs.Count - 1 >= Outputs.IndexOf(field)) return false;
            }

            return true;
        }

        public int GetMinEditableFieldIndex(Field field)
        {
            if (Functions[GetType().GetField("Name").GetValue(this).ToString()].Instance is not { } instance)
            {
                throw new Exception("The function is not instantiated");
            }

            if (Inputs.Contains(field))
            {
                if (!AllowAddInputs) return -1;

                return instance.Inputs.Count;
            }

            if (Outputs.Contains(field))
            {
                if (!AllowAddOutputs) return -1;

                return instance.Outputs.Count;
            }

            return -1;
        }

        public Field CreateNewField(bool input)
        {
            if (Functions[GetType().GetField("Name").GetValue(this).ToString()].Instance is not Function instance)
            {
                throw new Exception("The function is not instantiated");
            }

            var settings = input ? FunctionInputSettings : FunctionOutputSettings;
            var puts = input ? Inputs : Outputs;
            var instancePuts = input ? instance.Inputs : instance.Outputs;

            if (settings == null) return null;

            if (input && !AllowAddInputs) return null;
            if (!input && !AllowAddOutputs) return null;

            var name = "";

            if (settings.PrefixName != "") name = settings.PrefixName;
            
            name += (char)('A' + puts.Count - instancePuts.Count);

            if (settings.SupportedType != null) return new Field(name, settings.SupportedType);
            if (settings.SupportedTypes.Count > 0) return new Field(name, settings.SupportedTypes.ToArray());

            return new Field(name);
        }

        /**
         * Check if some fields are missing or have the wrong type before processing and when editing the function
         */
        public void ValidateFields()
        {
            if (_fieldsValidated) return;

            // Save inputs and outputs
            var inputs = Inputs;
            var outputs = Outputs;

            // Clear inputs and outputs
            Inputs = new List<Field>();
            Outputs = new List<Field>();

            if (Functions[GetType().GetField("Name").GetValue(this).ToString()].Instance is not { } instance)
            {
                throw new Exception("The function is not instantiated");
            }

            // Check if the fields are valid
            var check = new Action<List<Field>, List<Field>, List<Field>, bool, FunctionSettings>((instanceFields, fields, localFields, allowAdd, settings) =>
            {
                foreach (var instanceField in instanceFields)
                {
                    var savedField = fields.Find(field => field.FieldName == instanceField.FieldName);

                    if (savedField is { Value: not null } &&
                        ((instanceField is { Value: not null } && savedField.Value.Type == instanceField.Value.Type) ||
                         instanceField.SupportedTypes.Contains(savedField.Value.GetType())))
                    {
                        if (savedField.SupportedTypes.Count != instanceField.SupportedTypes.Count)
                        {
                            savedField.SupportedTypes = new List<SerializableSystemType>(instanceField.SupportedTypes);
                        }

                        savedField.AcceptAnyMethod = instanceField.AcceptAnyMethod;
                        savedField.AllowRename = instanceField.AllowRename;
                        localFields.Add(savedField.Clone());
                    }
                    else
                    {
                        localFields.Add(instanceField.Clone());
                    }
                }

                if (instanceFields.Count == 0)
                {
                    Debug.LogError("Problem");
                }

                if (!allowAdd) return;

                // Add the missing fields that have been customized, with allowAdd
                for (var i = instanceFields.Count; i < fields.Count; i++)
                {
                    var field = fields[i].Clone();

                    if (settings.SupportedType == null && settings.SupportedTypes.Count == 0)
                    {
                        field.SupportedTypes.Clear();

                        foreach (var supportedType in ReferenceUtility.GetAllIValueTypes())
                        {
                            field.SupportedTypes.Add(supportedType);
                        }
                    }

                    localFields.Add(field);
                }
            });

            check(instance.Inputs, inputs, Inputs, instance.AllowAddInputs, FunctionInputSettings);
            check(instance.Outputs, outputs, Outputs, instance.AllowAddOutputs, FunctionOutputSettings);
            EditableAttributes = instance.EditableAttributes;
            AllowAddInputs = instance.AllowAddInputs;
            FunctionInputSettings = instance.FunctionInputSettings;
            AllowAddOutputs = instance.AllowAddOutputs;
            FunctionOutputSettings = instance.FunctionOutputSettings;
            _fieldsValidated = true;
        }
#endif
    }
}
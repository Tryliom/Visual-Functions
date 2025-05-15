using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualFunctions
{
    [Serializable]
    public class ImportedFields
    {
        public ExportableFields Value;
        public bool FoldoutOpen = true;

        public ImportedFields(ExportableFields value)
        {
            Value = value;
        }
    }
    
    /**
     * This class is used to store a list of functions that can be invoked.
     */
    [Serializable]
    public class Functions
    {
        [SerializeReference] public List<Function> FunctionsList = new();
        public List<Field> GlobalVariables = new();
        
#if UNITY_EDITOR
        public bool FoldoutOpen = true;
        public bool GlobalValuesFoldoutOpen = true;
        public bool AllowGlobalVariables = true;
        
        // Only used in the editor to use temporary global variables on a func with disabled global variables
        public List<Field> TemporaryGlobalVariables = new();
#endif
        
        public List<ImportedFields> ImportedFields = new();
        public bool ImportedFieldsFoldoutOpen = true;
        
        private List<IVariable> _allVariables = new();

        /**
         * Disable the use of global variables in the functions.
         */
        public Functions DisableGlobalVariables()
        {
            AllowGlobalVariables = false;
            return this;
        }
        
        public void Invoke()
        {
#if UNITY_EDITOR
            if (!AllowGlobalVariables)
            {
                GlobalVariables.Clear();
                GlobalVariables.Capacity = TemporaryGlobalVariables.Count;
                GlobalVariables.AddRange(TemporaryGlobalVariables);
            }
#endif
            
            _allVariables.Clear();
            _allVariables.Capacity = GlobalVariables.Count;
            _allVariables.AddRange(GlobalVariables);
            
            foreach (var importedFields in ImportedFields)
            {
                _allVariables.AddRange(importedFields.Value.Fields);
            }
            
            foreach (var function in FunctionsList)
            {
                if (!function.Invoke(_allVariables)) return;
            }
            
#if UNITY_EDITOR
            if (!AllowGlobalVariables)
            {
                GlobalVariables.Clear();
            }
#endif
        }

        public void Invoke(List<IVariable> variables)
        {
            _allVariables.Clear();
            _allVariables.Capacity = variables.Count + GlobalVariables.Count;
            _allVariables.AddRange(variables);
            _allVariables.AddRange(GlobalVariables);
            
            foreach (var importedFields in ImportedFields)
            {
                _allVariables.AddRange(importedFields.Value.Fields);
            }
            
            foreach (var function in FunctionsList)
            {
                if (!function.Invoke(_allVariables)) return;
            }
        }
        
        public void EditField(string previousName, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return;
            
            foreach (var variable in GlobalVariables)
            {
                variable.OnEditField(previousName, newName);
            }

            foreach (var function in FunctionsList)
            {
                function.EditField(previousName, newName);
            }
        }
        
        public void ValidateGlobalVariables()
        {
            foreach (var variable in GlobalVariables)
            {
                variable.SupportedTypes.Clear();
                
                foreach (var supportedType in ReferenceUtility.GetAllIValueTypes())
                {
                    variable.SupportedTypes.Add(supportedType);
                }
            }
        }
        
        public Functions Clone()
        {
            return new Functions
            {
                FunctionsList = FunctionsList.ConvertAll(function => function.Clone()),
                GlobalVariables = GlobalVariables.ConvertAll(variable => variable.Clone()),
                AllowGlobalVariables = AllowGlobalVariables,
                FoldoutOpen = FoldoutOpen,
                GlobalValuesFoldoutOpen = GlobalValuesFoldoutOpen
            };
        }
    }
}
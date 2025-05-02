using System;
using System.Collections.Generic;
using UnityEngine;

namespace TryliomFunctions
{
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
#endif
        
        private List<Field> _allVariables = new();
        
        public void Invoke()
        {
            foreach (var function in FunctionsList)
            {
                if (!function.Invoke(GlobalVariables)) return;
            }
        }

        public void Invoke(List<Field> variables)
        {
            _allVariables.Clear();
            _allVariables.Capacity = variables.Count + GlobalVariables.Count;
            _allVariables.AddRange(variables);
            _allVariables.AddRange(GlobalVariables);
            
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
    }
}
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
        
        public void Invoke()
        {
            var allVariables = new List<Field>(GlobalVariables);
            
            foreach (var function in FunctionsList)
            {
                if (!function.Invoke(allVariables)) return;
            }
        }

        public void Invoke(List<Field> variables)
        {
            var allVariables = new List<Field>(variables);
            
            allVariables.AddRange(GlobalVariables);
            
            foreach (var function in FunctionsList)
            {
                if (!function.Invoke(allVariables)) return;
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
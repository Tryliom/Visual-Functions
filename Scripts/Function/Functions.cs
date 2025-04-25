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
#if UNITY_EDITOR
        public bool FoldoutOpen = true;
#endif

        public void Invoke()
        {
            foreach (var function in FunctionsList)
                if (!function.Invoke())
                    return;
        }
    }
}
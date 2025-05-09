using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "VisualFunctionsSettings", menuName = "VisualFunctions/Settings")]
    public class VisualFunctionsSettings : ScriptableObject
    {
        public string PathToGlobalVariables = "Assets/Resources/ScriptableObjects/GlobalVariables";
        public string PathToVariables = "Assets/Resources/ScriptableObjects/Variables";

        public string GlobalValuesPrefix = "_";
        
        private void OnValidate()
        {
            VisualFunctionsInitializer.LoadSettings();
        }
    }
}
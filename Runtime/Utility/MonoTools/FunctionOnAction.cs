using UnityEngine;

namespace VisualFunctions
{
    /**
     * Used to create a component that can assign to other callbacks the functions to be called.
     * Example: OnClick in a button, OnPointerEnter in a button/UI element, etc.
     */
    public class FunctionOnAction : MonoBehaviour
    {
        [Header("Events")] 
        [SerializeField] private Functions _onAction;
        
        public void LaunchFunctions()
        {
            if (_onAction.FunctionsList.Count == 0) return;

            _onAction.Invoke();
        }
    }
}
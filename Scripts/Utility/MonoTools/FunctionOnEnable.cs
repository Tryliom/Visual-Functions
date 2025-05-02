using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnEnable : MonoBehaviour
    {
        [Header("Events")] 
        [SerializeField] private Functions _onEnable;

        private void OnEnable()
        {
            if (_onEnable.FunctionsList.Count == 0) return;

            _onEnable.Invoke();
        }
    }
}
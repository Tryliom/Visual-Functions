using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnDisable : MonoBehaviour
    {
        [Header("Events")] [SerializeField] private Functions _onDisable;

        private void OnDisable()
        {
            if (_onDisable.FunctionsList.Count == 0) return;

            _onDisable.Invoke();
        }
    }
}
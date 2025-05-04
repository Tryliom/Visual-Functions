using UnityEngine;

namespace VisualFunctions
{
    public class FunctionOnDestroy : MonoBehaviour
    {
        [Header("Events")] [SerializeField] private Functions _onDestroy;

        private void OnDestroy()
        {
            if (_onDestroy.FunctionsList.Count == 0) return;

            _onDestroy.Invoke();
        }
    }
}
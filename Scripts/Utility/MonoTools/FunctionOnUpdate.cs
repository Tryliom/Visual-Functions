using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnUpdate : MonoBehaviour
    {
        [Header("Events")] [SerializeField] private Functions _onUpdate;

        private void Update()
        {
            if (_onUpdate.FunctionsList.Count == 0) return;

            _onUpdate.Invoke();
        }
    }
}
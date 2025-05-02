using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnStart : MonoBehaviour
    {
        [Header("Events")] 
        [SerializeField] private Functions _onStart;

        private void Start()
        {
            if (_onStart.FunctionsList.Count == 0) return;

            _onStart.Invoke();
        }
    }
}
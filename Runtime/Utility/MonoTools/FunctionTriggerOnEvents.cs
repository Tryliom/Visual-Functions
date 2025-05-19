using UnityEngine;

namespace VisualFunctions
{
    public class FunctionTriggerOnEvents : MonoBehaviour
    {
        [SerializeField] private FunctionsOnEvents _functionsOnEvents;

        private void Awake()
        {
            _functionsOnEvents.SubscribeToEvents();
        }

        private void OnDisable()
        {
            _functionsOnEvents.UnsubscribeFromEvents();
        }
    }
}
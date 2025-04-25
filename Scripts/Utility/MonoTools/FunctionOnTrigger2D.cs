using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnTrigger2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("The variable that will store the trigger GameObject [Optional]")]
        private GameObjectVariable _triggerVariable;

        [Header("Events")] [SerializeField] private Functions _onEnter;

        [SerializeField] private Functions _onExit;
        [SerializeField] private Functions _onStay;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_onEnter.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value = other.gameObject;

            _onEnter.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_onExit.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value = other.gameObject;

            _onExit.Invoke();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_onStay.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value = other.gameObject;

            _onStay.Invoke();
        }
    }
}
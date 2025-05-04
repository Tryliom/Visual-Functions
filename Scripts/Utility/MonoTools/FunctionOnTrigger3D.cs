using UnityEngine;

namespace TryliomFunctions
{
    public class FunctionOnTrigger3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("The variable that will store the trigger GameObject [Optional]")]
        private ComponentOfGameObjectVariable _triggerVariable;

        [Header("Events")] [SerializeField] private Functions _onEnter;

        [SerializeField] private Functions _onExit;
        [SerializeField] private Functions _onStay;

        private void OnTriggerEnter(Collider other)
        {
            if (_onEnter.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value.GameObject = other.gameObject;

            _onEnter.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_onExit.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value.GameObject = other.gameObject;

            _onExit.Invoke();
        }

        private void OnTriggerStay(Collider other)
        {
            if (_onStay.FunctionsList.Count == 0) return;

            if (_triggerVariable) _triggerVariable.Value.GameObject = other.gameObject;

            _onStay.Invoke();
        }
    }
}
using System;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Variable<TType> : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline] public string DeveloperDescription = "";
#endif
        [SerializeField] private TType _value;
        public TType Value 
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke();
            }
        }

        public GameEventData OnValueChanged;
    }
}
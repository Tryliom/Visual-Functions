using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "GameEventData", menuName = "VisualFunctions/GameEventData", order = 0)]
    public class GameEventData : ScriptableObject
    {
        private readonly List<Action> _functions = new ();
    
        public void Add(Action action)
        {
            _functions.Add(action);
        }
    
        public void Remove(Action action)
        {
            _functions.Remove(action);
        }
    
        public void Invoke()
        {
            _functions.ForEach(a => a?.Invoke());
        }
    }
}
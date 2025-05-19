using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualFunctions
{
    [Serializable]
    public class FunctionsOnEvent
    {
        public GameEventData OnEvent;
        public Functions Functions;
    }

    [Serializable]
    public class FunctionsOnEvents
    {
        [SerializeReference] private List<FunctionsOnEvent> _functionsOnEvents = new();

        public void SubscribeToEvents()
        {
            foreach (var onEvent in _functionsOnEvents) onEvent.OnEvent?.Add(() => onEvent.Functions.Invoke());
        }

        public void UnsubscribeFromEvents()
        {
            foreach (var onEvent in _functionsOnEvents) onEvent.OnEvent?.Remove(() => onEvent.Functions.Invoke());
        }
    }
}
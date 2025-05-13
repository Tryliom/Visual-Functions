using System;
using System.Collections.Generic;

namespace VisualFunctions
{
    [Serializable]
    public class CallGameEvent : Function
    {
        public static readonly string Name = "Call Game Event";
        public static readonly string Description = "It will call the specified game event";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public GameEventData GameEventData;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(GameEventData));
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            GameEventData.Invoke();

            return true;
        }
    }
}
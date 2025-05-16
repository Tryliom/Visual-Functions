using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualFunctions
{
    public enum TimeType
    {
        Scaled, Unscaled
    }
    
    [Serializable]
    public class Timer : Function
    {
        public static readonly string Name = "Timer";
        public static readonly string Description = "It will execute the functions inside the timer after a certain amount of time.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public TimeType TimeType = TimeType.Scaled;
        public Functions OnFinish = new Functions().DisableGlobalVariables().DisableImport();
        
        private List<IVariable> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(TimeType));
            EditableAttributes.Add(nameof(OnFinish));
            
            Inputs.Add(new Field("MaxTime", typeof(FloatReference)).AllowAnyMethod().AllowRenameField());
            Inputs.Add(new Field("CurrentTime", typeof(FloatReference)).AllowAnyMethod().AllowRenameField());
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            var maxTime = (FloatReference) Inputs[0].Value;
            var currentTime = (FloatReference) Inputs[1].Value;

            currentTime.Value += Time.deltaTime;
            
            if (currentTime.Value < maxTime.Value) return true;

            currentTime.Value = 0;
            
            return OnFinish.FunctionsList.All(function => function.Invoke(_globalVariables));
        }
    }
}
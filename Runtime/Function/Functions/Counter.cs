using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualFunctions
{
    [Serializable]
    public class Counter : Function
    {
        public static readonly string Name = "Counter";
        public static readonly string Description = "It will execute the functions inside the counter when the count reaches the max number.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;
        
        public Functions OnGoal = new Functions().DisableGlobalVariables().DisableImport();
        
        private List<IVariable> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(TimeType));
            EditableAttributes.Add(nameof(OnGoal));
            
            Inputs.Add(new Field("GoalNumber", typeof(IntReference)).AllowAnyMethod().AllowRenameField());
            Inputs.Add(new Field("Count", typeof(IntReference)).AllowAnyMethod().AllowRenameField());
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            var goalNumber = (IntReference) Inputs[0].Value;
            var count = (IntReference) Inputs[1].Value;

            count.Value++;
            
            if (count.Value < goalNumber.Value) return true;

            count.Value = 0;
            
            return OnGoal.FunctionsList.All(function => function.Invoke(_globalVariables));
        }
    }
}
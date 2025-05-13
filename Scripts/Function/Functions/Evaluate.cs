using System;
using System.Collections.Generic;

namespace VisualFunctions
{
    [Serializable]
    public class Evaluate : Function
    {
        public static readonly string Name = "Evaluator";
        public static readonly string Description = "Perform evaluations on a formula string";
        public static readonly FunctionCategory Category = FunctionCategory.Logic;
        
        private List<IVariable> _variables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(Formula)).AllowAnyMethod());

            AllowAddInput(new FunctionSettings().AllowMethods(true));
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _variables.Clear();
            _variables.Capacity = variables.Count + Inputs.Count;
            _variables.AddRange(variables);
                
            foreach (var field in Inputs)
            {
                _variables.Add(field);
            }
            
            var formula = (string) Inputs[0].Value.Value;

            Evaluator.Process(Uid, formula, _variables);

            return true;
        }
    }
}
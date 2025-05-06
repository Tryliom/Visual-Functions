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
        
        private List<ExpressionVariable> _variables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(Formula)).AllowAnyMethod());

            AllowAddInput(new FunctionSettings().AllowMethods(true));
        }
#endif

        protected override bool Process(List<Field> variables)
        {
            if (_variables.Count != variables.Count + Inputs.Count)
            {
                _variables.Clear();
                _variables.Capacity = variables.Count + Inputs.Count;
                
                foreach (var field in variables)
                {
                    _variables.Add(new ExpressionVariable(field.FieldName, field.Value));
                }
                
                foreach (var field in Inputs)
                {
                    _variables.Add(new ExpressionVariable(field.FieldName, field.Value));
                }
            }
            else
            {
                for (var i = 0; i < variables.Count; i++)
                {
                    _variables[i].Name = variables[i].FieldName;
                    _variables[i].Value = variables[i].Value;
                }

                for (var i = variables.Count; i < Inputs.Count; i++)
                {
                    _variables[i].Name = Inputs[i].FieldName;
                    _variables[i].Value = Inputs[i].Value;
                }
            }
            
            var formula = (string) Inputs[0].Value.Value;

            Evaluator.Process(Uid, formula, _variables);

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TryliomFunctions
{
    [Serializable]
    public class Evaluate : Function
    {
        public static readonly string Name = "Evaluator";
        public static readonly string Description = "Perform evaluations on a formula string";
        public static readonly FunctionCategory Category = FunctionCategory.Logic;
        
        private List<Field> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(Formula)).AllowAnyMethod());

            AllowAddInput(new FunctionSettings().AllowMethods(true));
        }
#endif

        protected override bool Process(List<Field> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            var formula = GetInput<string>("Formula").Value;

            Evaluator.Process(Uid, formula, _globalVariables.Select(x => new ExpressionVariable(x.FieldName, x.Value)).ToList());

            return true;
        }
    }
}
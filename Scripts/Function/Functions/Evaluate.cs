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

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(Formula)).AllowAnyMethod());

            AllowAddInput(new FunctionSettings().AllowMethods(true));
        }
#endif

        protected override bool Process(List<Field> variables)
        {
            var allVariables = new List<Field>(variables);
            
            allVariables.AddRange(Inputs);
            
            var formula = GetInput<string>("Formula").Value;

            Evaluator.Process(Uid, formula, allVariables.Select(x => new ExpressionVariable(x.FieldName, x.Value)).ToList());

            return true;
        }
    }
}
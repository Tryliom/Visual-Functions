using System;
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
        
        protected override void OnEditField(string previousName, string newName)
        {
            var pattern = $@"(?<=\W|^){Regex.Escape(previousName)}(?=\W|$)";
            
            Inputs[0].Value.Value = Regex.Replace((string) Inputs[0].Value.Value, pattern, newName, RegexOptions.Multiline);
        }
#endif

        protected override bool Process()
        {
            var formula = GetInput<string>("Formula").Value;
            var variables = Inputs.Select(x => new ExpressionVariable(x.FieldName, GetInputValue(x.FieldName)))
                .ToList();

            Evaluator.Process(Uid, formula, variables);

            return true;
        }
    }
}
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class If : Function
    {
        public static readonly string Name = "If";
        public static readonly string Description = "If the condition is true, it will execute the if branch, otherwise it will execute the else branch";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public Functions IfBranch;
        public Functions ElseBranch;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(IfBranch));
            EditableAttributes.Add(nameof(ElseBranch));
            Inputs.Add(new Field("Condition", typeof(Formula)));
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
        
        protected override void OnEditField(string previousName, string newName)
        {
            var pattern = $@"(?<=\W|^){Regex.Escape(previousName)}(?=\W|$)";
            
            Inputs[0].Value.Value = Regex.Replace((string) Inputs[0].Value.Value, pattern, newName, RegexOptions.Multiline);
        }
#endif

        protected override bool Process()
        {
            if (CheckCondition())
            {
                if (IfBranch.FunctionsList.Any(function => !function.Invoke())) return false;
            }
            else
            {
                if (ElseBranch.FunctionsList.Any(function => !function.Invoke())) return false;
            }

            return true;
        }

        private bool CheckCondition()
        {
            var formula = GetInput<string>("Condition").Value;
            var variables = Inputs.Select(x => new ExpressionVariable(x.FieldName, GetInputValue(x.FieldName)))
                .ToList();
            var result = Evaluator.Process(Uid, formula, variables) switch
            {
                AccessorCaller methodCaller => methodCaller.Result.Value,
                MethodValue methodValue => methodValue.Value,
                bool booleanValue => booleanValue,
                _ => throw new InvalidOperationException("Invalid type")
            };

            if (result is not bool res)
            {
                Debug.LogError("The result of the operation in If function is not a boolean");
                return false;
            }

            return res;
        }
    }
}
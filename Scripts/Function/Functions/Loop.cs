using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Loop : Function
    {
        public static readonly string Name = "Loop";
        public static readonly string Description = "While the condition is true, it will execute the functions inside the loop";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public Functions FunctionsToLoop;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(FunctionsToLoop));
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
            var loops = 0;

            while (CheckCondition())
            {
                loops++;

                if (loops > 1000)
                {
                    Debug.LogError("Infinite loop detected in Loop function");
                    break;
                }

                if (FunctionsToLoop.FunctionsList.Any(function => !function.Invoke())) break;
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
                Debug.LogError("The result of the operation is not a boolean");
                return false;
            }

            return res;
        }
    }
}
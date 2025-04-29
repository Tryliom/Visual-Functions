using System;
using System.Collections.Generic;
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
        
        private List<Field> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(FunctionsToLoop));
            Inputs.Add(new Field("Condition", typeof(Formula)));
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
#endif

        protected override bool Process(List<Field> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            var loops = 0;

            while (CheckCondition())
            {
                loops++;

                if (loops > 1000)
                {
                    Debug.LogError("Infinite loop detected in Loop function");
                    break;
                }

                if (FunctionsToLoop.FunctionsList.Any(function => !function.Invoke(_globalVariables))) break;
            }

            return true;
        }

        private bool CheckCondition()
        {
            var formula = GetInput<string>("Condition").Value;
            var variables = _globalVariables.Select(x => new ExpressionVariable(x.FieldName, x.Value)).ToList();
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
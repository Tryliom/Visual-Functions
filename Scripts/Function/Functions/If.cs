using System;
using System.Collections.Generic;
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
        
        private List<Field> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(IfBranch));
            EditableAttributes.Add(nameof(ElseBranch));
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
            
            if (CheckCondition())
            {
                _globalVariables.AddRange(IfBranch.GlobalVariables);
                
                if (IfBranch.FunctionsList.Any(function => !function.Invoke(_globalVariables))) return false;
            }
            else
            {
                _globalVariables.AddRange(ElseBranch.GlobalVariables);
                
                if (ElseBranch.FunctionsList.Any(function => !function.Invoke(_globalVariables))) return false;
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
                Debug.LogError("The result of the operation in If function is not a boolean");
                return false;
            }

            return res;
        }
    }
}
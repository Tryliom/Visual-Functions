using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Expect : Function
    {
        public static readonly string Name = "Expect";
        public static readonly string Description = "Check that the condition is true, if not, will throw an exception.\n" +
                                                    "If there is multiple lines (;), it will check the ones that are a boolean.";
        public static readonly FunctionCategory Category = FunctionCategory.Debug;
        
        private List<Field> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Condition", typeof(Formula)).AllowAnyMethod());
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
#endif

        protected override bool Process(List<Field> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);

            if (!CheckCondition())
            {
                throw new Exception($"Condition {GetInput<string>("Condition").Value} failed in {Name} function");
            }

            return true;
        }

        private bool CheckCondition()
        {
            var formula = GetInput<string>("Condition").Value;
            var variables = _globalVariables.Select(x => new ExpressionVariable(x.FieldName, x.Value)).ToList();
            var results = Evaluator.Process(Uid, formula, variables);

            foreach (var result in results)
            {
                if (ExpressionUtility.ExtractValue(result, Uid, variables) is not bool res) continue;
                if (res) continue;
                
                Debug.Log($"Condition failed for formula {formula.Split(";")[results.IndexOf(result)].Replace("\n", "")}");
                return false;
            }
            
            return true;
        }
    }
}
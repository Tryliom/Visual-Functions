using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualFunctions
{
    [Serializable]
    public class Loop : Function
    {
        public static readonly string Name = "Loop";
        public static readonly string Description = "While the condition is true, it will execute the functions inside the loop.\n" +
                                                    "If there is multiple lines (;), it will check the ones that are a boolean.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public Functions FunctionsToLoop = new Functions().DisableGlobalVariables().DisableGlobalVariables();
        
        private List<IVariable> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(FunctionsToLoop));
            Inputs.Add(new Field("Condition", typeof(Formula)).AllowAnyMethod());
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
#endif

        protected override bool Process(List<IVariable> variables)
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
            var results = Evaluator.Process(Uid, formula, _globalVariables);

            foreach (var result in results)
            {
                if (ExpressionUtility.ExtractValue(result, Uid, _globalVariables) is not bool res) continue;
                if (!res) return false;
            }
            
            return true;
        }
    }
}
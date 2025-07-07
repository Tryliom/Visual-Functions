using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualFunctions
{
    [Serializable]
    public class For : Function
    {
        public static readonly string Name = "For";

        public static readonly string Description = "It will execute the functions inside the loop a number of times.\n" +
                                                    "Index is the current loop index, it's reset to 0 at the start of the loop.\n" +
                                                    "Loops is only evaluated once at the start of the loop.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;
        
        public Functions FunctionsToLoop = new Functions().DisableGlobalVariables().DisableImport();
        
        private List<IVariable> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Loops", typeof(Formula)).AllowAnyMethod());
            Inputs.Add(new Field("Index", typeof(IntReference)).AllowRenameField());
            Inputs.Add(new Field("Step", typeof(IntReference)).AllowRenameField());
            EditableAttributes.Add(nameof(FunctionsToLoop));
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            var evaluatedLoops = Evaluator.Process(Uid, GetInput<string>("Loops").Value, variables);
            
            if (evaluatedLoops.Count == 0)
            {
                throw new Exception($"Invalid or missing value for 'Loops' in {Name} function");
            }
            
            var loops = Convert.ToInt32(ExpressionUtility.ExtractValue(evaluatedLoops[0], Uid, _globalVariables));
            var index = (IntReference) Inputs[1].Value;
            var step = (IntReference) Inputs[2].Value;

            if (step == 0)
            {
                throw new Exception($"Step cannot be 0 in {Name} function");
            }

            for (index.Value = 0; index.Value < loops; index.Value += step.Value)
            {
                if (FunctionsToLoop.FunctionsList.Any(function => !function.Invoke(_globalVariables))) break;
            }

            return true;
        }
    }
}
using System;
using System.Linq;

namespace TryliomFunctions
{
    [Serializable]
    public class For : Function
    {
        public static readonly string Name = "For";

        public static readonly string Description =
            "It will execute the functions inside the loop a number of times. Index is the current loop index";

        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public Functions FunctionsToLoop = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Loops", typeof(IntReference)));
            Inputs.Add(new Field("Index", typeof(IntReference)));
            EditableAttributes.Add(nameof(FunctionsToLoop));
        }
#endif

        protected override bool Process()
        {
            var loops = GetInput<int>("Loops");
            var index = GetInput<int>("Index");

            for (index.Value = 0; index.Value < loops.Value; index.Value++)
                if (FunctionsToLoop.FunctionsList.Any(function => !function.Invoke()))
                    break;

            return true;
        }
    }
}
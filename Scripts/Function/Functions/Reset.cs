using System;

namespace TryliomFunctions
{
    [Serializable]
    public class Reset : Function
    {
        public static readonly string Name = "Reset";
        public static readonly string Description = "Reset any input value";
        public static readonly FunctionCategory Category = FunctionCategory.Setter;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            AllowAddInput(new FunctionSettings());
        }
#endif

        protected override bool Process()
        {
            foreach (var input in Inputs)
                if (input.Value is IResettable resettable)
                    resettable.ResetValue();

            return true;
        }
    }
}
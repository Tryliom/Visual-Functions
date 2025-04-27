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
            {
                var resettable = input.Value as IResettable;

                resettable?.ResetValue();
            }

            return true;
        }
    }
}
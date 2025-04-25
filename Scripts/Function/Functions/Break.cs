using System;

namespace TryliomFunctions
{
    [Serializable]
    public class Break : Function
    {
        public static readonly string Name = "Break";
        public static readonly string Description = "Will stop the execution of the function or loop";
        public static readonly FunctionCategory Category = FunctionCategory.Utility;

        protected override bool Process()
        {
            return false;
        }
    }
}
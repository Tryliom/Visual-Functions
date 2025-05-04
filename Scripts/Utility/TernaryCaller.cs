using System.Collections.Generic;

namespace VisualFunctions
{
    public enum TernaryState
    {
        NotSet,
        WaitForIfTrue,
        WaitForIfFalse
    }

    public class TernaryCaller
    {
        public List<object> ConditionList = new();
        public List<object> IfFalse = new();
        public List<object> IfTrue = new();
    }
}
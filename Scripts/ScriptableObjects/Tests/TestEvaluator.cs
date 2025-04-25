using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestEvaluator", menuName = "TryliomFunctions/Test/Tests/TestEvaluator")]
    public class TestEvaluator : Test
    {
        public IntReference Input;

        public override void RunCode()
        {
            Input.Value = Input.Value * 2 + 3 * 5 - Input.Value;
        }
    }
}
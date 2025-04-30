using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestEvaluator", menuName = "TryliomFunctions/Test/PerfTests/TestEvaluator")]
    public class PerformanceTestEvaluator : PerformanceTest
    {
        public IntReference Input;

        public override void RunCode()
        {
            Input.Value = Input.Value * 2 + 3 * 5 - Input.Value;
        }
    }
}
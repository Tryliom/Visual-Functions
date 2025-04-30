using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestMethodEvaluator", menuName = "TryliomFunctions/Test/PerfTests/TestMethodEvaluator")]
    public class PerformanceTestMethodEvaluator : PerformanceTest
    {
        public GameObjectVariable Input;
        public StringVariable Result;

        public override void RunCode()
        {
            Result.Value = Input.Value.CompareTag("Player") ? "A" : "B";
        }
    }
}
using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestMethodEvaluator", menuName = "TryliomFunctions/Test/PerfTests/TestMethodEvaluator")]
    public class PerformanceTestMethodEvaluator : PerformanceTest
    {
        public ComponentOfGameObjectVariable Input;
        public StringVariable Result;

        public override void RunCode()
        {
            Result.Value = Input.Value.GameObject.CompareTag("Player") ? "A" : "B";
        }
    }
}
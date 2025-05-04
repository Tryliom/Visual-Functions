using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestMethodEvaluator", menuName = "VisualFunctions/Test/PerfTests/TestMethodEvaluator")]
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
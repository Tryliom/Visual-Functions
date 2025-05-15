using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestMethodEvaluator", menuName = "Visual Functions/Test/PerfTests/TestMethodEvaluator")]
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
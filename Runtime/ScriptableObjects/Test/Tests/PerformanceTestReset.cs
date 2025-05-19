using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestReset", menuName = "Visual Functions/Test/PerfTests/TestReset")]
    public class PerformanceTestReset : PerformanceTest
    {
        public IntReference input;

        public override void RunCode()
        {
            input.Value = 0;
        }
    }
}
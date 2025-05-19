using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestBreak", menuName = "Visual Functions/Test/PerfTests/TestBreak")]
    public class PerformanceTestBreak : PerformanceTest
    {
        public override void RunCode()
        {
            while (true) break;
        }
    }
}
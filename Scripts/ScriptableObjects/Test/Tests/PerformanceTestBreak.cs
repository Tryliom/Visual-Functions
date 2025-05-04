using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestBreak", menuName = "VisualFunctions/Test/PerfTests/TestBreak")]
    public class PerformanceTestBreak : PerformanceTest
    {
        public override void RunCode()
        {
            while (true) break;
        }
    }
}
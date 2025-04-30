using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestReset", menuName = "TryliomFunctions/Test/PerfTests/TestReset")]
    public class PerformanceTestReset : PerformanceTest
    {
        public IntReference input;

        public override void RunCode()
        {
            input.Value = 0;
        }
    }
}
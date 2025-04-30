using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestBreak", menuName = "TryliomFunctions/Test/PerfTests/TestBreak")]
    public class PerformanceTestBreak : PerformanceTest
    {
        public override void RunCode()
        {
            while (true) break;
        }
    }
}
using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestLoop", menuName = "Visual Functions/Test/PerfTests/TestLoop")]
    public class PerformanceTestLoop : PerformanceTest
    {
        public IntReference Loops;
        public IntReference Iterations;

        public override void RunCode()
        {
            while (Iterations.Value < Loops.Value) Iterations.Value++;
            
            Iterations.Value = 0;
        }
    }
}
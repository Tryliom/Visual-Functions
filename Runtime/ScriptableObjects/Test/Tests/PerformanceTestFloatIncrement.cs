using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestFloatIncrement", menuName = "Visual Functions/Test/PerfTests/TestFloatIncrement")]
    public class PerformanceTestFloatIncrement : PerformanceTest
    {
        public FloatReference Float;
        
        public override void RunCode()
        {
            Float.Value++;
        }
    }
}
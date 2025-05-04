using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestComplexLoop", menuName = "VisualFunctions/Test/PerfTests/TestComplexLoop")]
    public class PerformanceTestComplexLoop : PerformanceTest
    {
        public FloatReference MaxNumberCondition;
        public FloatReference A;
        public FloatReference B;

        public override void RunCode()
        {
            while (A.Value < MaxNumberCondition.Value) A.Value += 3 * (A.Value / B.Value) + B.Value;

            A.Value -= MaxNumberCondition.Value;
        }
    }
}
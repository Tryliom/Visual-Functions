using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestLoop", menuName = "TryliomFunctions/Test/Tests/TestLoop")]
    public class TestLoop : Test
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
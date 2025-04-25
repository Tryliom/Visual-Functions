using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestReset", menuName = "TryliomFunctions/Test/Tests/TestReset")]
    public class TestReset : Test
    {
        public IntReference input;

        public override void RunCode()
        {
            input.Value = 0;
        }
    }
}
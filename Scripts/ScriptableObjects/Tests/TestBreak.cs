using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestBreak", menuName = "TryliomFunctions/Test/Tests/TestBreak")]
    public class TestBreak : Test
    {
        public override void RunCode()
        {
            while (true) break;
        }
    }
}
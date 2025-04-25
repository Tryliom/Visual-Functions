using UnityEngine;

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestMethodEvaluator", menuName = "TryliomFunctions/Test/Tests/TestMethodEvaluator")]
    public class TestMethodEvaluator : Test
    {
        public GameObjectVariable Input;
        public StringVariable Result;

        public override void RunCode()
        {
            Result.Value = Input.Value.CompareTag("Player") ? "This is a player" : "This is something else";
        }
    }
}
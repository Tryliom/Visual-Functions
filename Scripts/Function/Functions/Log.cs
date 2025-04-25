using System;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Log : Function
    {
        public static readonly string Name = "Log";
        public static readonly string Description = "Log any input value";
        public static readonly FunctionCategory Category = FunctionCategory.Debug;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            AllowAddInput(new FunctionSettings("ObjectsToLog"));
        }
#endif

        protected override bool Process()
        {
            var logString = string.Empty;

            foreach (var field in Inputs) logString += GetInputValue(field.FieldName).Value + " ";

            Debug.Log(logString);

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
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

        protected override bool Process(List<Field> variables)
        {
            var logString = string.Empty;

            foreach (var field in Inputs) logString += field.Value.Value + " ";

            Debug.Log(logString);

            return true;
        }
    }
}
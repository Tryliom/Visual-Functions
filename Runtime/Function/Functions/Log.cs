using System;
using System.Collections.Generic;
using UnityEngine;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

namespace VisualFunctions
{
    [Serializable]
    public class Log : Function
    {
        public static readonly string Name = "Log";
        public static readonly string Description = "Log a message to the console using the formula string.\n" +
                                                    "You need to encapsulate the string in double quotes.\n" +
                                                    "Example: \"My variable is \" + myVariable + \" !\"";
        public static readonly FunctionCategory Category = FunctionCategory.Debug;
        
        public LogLevel LogLevel = LogLevel.Info;
        
        private List<IVariable> _variables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(Formula)).AllowAnyMethod());
            EditableAttributes.Add(nameof(LogLevel));
            
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _variables.Clear();
            _variables.Capacity = variables.Count + Inputs.Count;
            _variables.AddRange(variables);
            _variables.AddRange(Inputs);
            
            var formula = (string) Inputs[0].Value.Value;
            var results = Evaluator.Process(Uid, formula, _variables);

            foreach (var result in results)
            {
                if (result is not string str) continue;
                
                switch (LogLevel)
                {
                    case LogLevel.Info:
                        Debug.Log(str);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(str);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(str);
                        break;
                }
            }

            return true;
        }
    }
}
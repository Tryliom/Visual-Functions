using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Evaluate : Function
    {
        public static readonly string Name = "Evaluator";
        public static readonly string Description = "Perform evaluations on a formula string.\n" +
            "Support all logical operations (+,-,/,&&,||,...) and variables.\nAssign result to a variable with: varA = A + B. Execute functions also work.\n" +
            "Boolean are converted to int when used in +, -, * and / operations\n" +
            "If ternary (condition ? if true : if false) used inside another need to have ()\n" +
            "Examples: \n" +
            "varA = A + B\n" +
            "A.x *= Random.Range(0, A.y)\n" +
            "myVar.myMethod()\n" +
            "A.x += A.y; A.y += Random.Range(-0.5, 2)";

        public static readonly FunctionCategory Category = FunctionCategory.Logic;

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Formula", typeof(string)).AllowAnyMethod());

            AllowAddInput(new FunctionSettings().AllowMethods(true));
        }
#endif

        protected override void OnEditField(string previousName, string newName)
        {
            if (Inputs[0].Value is not StringReference stringReference)
            {
                Debug.LogError("The formula must be a string");
                return;
            }

            // Search for the previous name in the formula only if the name is surrounded by spaces, brackets or operators to not replace a similar name
            var pattern = $@"(?<=\W|^){Regex.Escape(previousName)}(?=\W|$)";

            if (!stringReference.UseLocal && stringReference.Variable != null)
                stringReference.Variable.Value =
                    Regex.Replace(stringReference.Variable.Value, pattern, newName, RegexOptions.Multiline);

            Inputs[0].Value = new StringReference
            {
                LocalValue = Regex.Replace(stringReference.LocalValue, pattern, newName, RegexOptions.Multiline),
                UseLocal = stringReference.UseLocal,
                Variable = stringReference.Variable
            };
        }

        protected override bool Process()
        {
            var formula = GetInput<string>("Formula").Value;
            var variables = Inputs.Select(x => new ExpressionVariable(x.FieldName, GetInputValue(x.FieldName)))
                .ToList();

            Evaluator.Process(Uid, formula, variables);

            return true;
        }
    }
}
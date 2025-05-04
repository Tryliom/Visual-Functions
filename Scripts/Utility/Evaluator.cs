using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TryliomFunctions
{
    public enum OperationType
    {
        Add,
        Substract,
        Multiply,
        Divide,
        Modulo,
        OpenBracket,
        CloseBracket,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
        Not,

        // Special operations
        RightShift,
        LeftShift,
        BitwiseAnd,
        BitwiseOr,
        BitwiseNot,
        BitwiseXor,

        // Assignment operations (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=)
        Assignment,
        AssignmentAdd,
        AssignmentSubstract,
        AssignmentMultiply,
        AssignmentDivide,
        AssignmentModulo,
        AssignmentAnd,
        AssignmentOr,
        AssignmentXor,
        AssignmentLeftShift,
        AssignmentRightShift
    }

    public class ExpressionVariable
    {
        public string Name;
        public IValue Value;

        public ExpressionVariable(string name, IValue value)
        {
            Name = name;
            Value = value;
        }
    }

    public static class Evaluator
    {
        public static readonly Dictionary<string, OperationType> Operations = new()
        {
            { "+", OperationType.Add },
            { "-", OperationType.Substract },
            { "*", OperationType.Multiply },
            { "/", OperationType.Divide },
            { "%", OperationType.Modulo },
            { "(", OperationType.OpenBracket },
            { ")", OperationType.CloseBracket },
            { "==", OperationType.Equal },
            { "!=", OperationType.NotEqual },
            { ">", OperationType.GreaterThan },
            { ">=", OperationType.GreaterThanOrEqual },
            { "<", OperationType.LessThan },
            { "<=", OperationType.LessThanOrEqual },
            { "&&", OperationType.And },
            { "||", OperationType.Or },
            { "!", OperationType.Not },
            { ">>", OperationType.RightShift },
            { "<<", OperationType.LeftShift },
            { "&", OperationType.BitwiseAnd },
            { "|", OperationType.BitwiseOr },
            { "~", OperationType.BitwiseNot },
            { "^", OperationType.BitwiseXor },
            { "=", OperationType.Assignment },
            { "+=", OperationType.AssignmentAdd },
            { "-=", OperationType.AssignmentSubstract },
            { "*=", OperationType.AssignmentMultiply },
            { "/=", OperationType.AssignmentDivide },
            { "%=", OperationType.AssignmentModulo },
            { "&=", OperationType.AssignmentAnd },
            { "|=", OperationType.AssignmentOr },
            { "^=", OperationType.AssignmentXor },
            { "<<=", OperationType.AssignmentLeftShift },
            { ">>=", OperationType.AssignmentRightShift }
        };

        public static List<object> Process(string uid, string formula, List<ExpressionVariable> variables)
        {
            // Separate the formula into other formulas if it contains a ;
            var formulas = formula.Split(';');
            var results = new List<object>(formulas.Length);

            foreach (var f in formulas)
            {
                switch (f)
                {
                    case "":
                    case "\n":
                        continue;
                    default:
                    {
                        var result = ProcessFormula(uid, f, variables);
                        results.Add(result);
                        break;
                    }
                }
            }

            return results;
        }

        private static object ProcessFormula(string uid, string formula, List<ExpressionVariable> variables)
        {
            if (FormulaCache.Get(uid, formula) is { } cachedResult)
            {
                return EvaluateExpression(uid, cachedResult, variables);
            }

            var expressions = new List<object>(formula.Length);
            var numberBuilder = new StringBuilder();
            var useAssignment = false;
            var encapsulateNext = false;
            // Ternary operators
            var ternaryCallers = new List<TernaryCaller>();
            var ternaryDepth = -1;
            var bracketDepth = 0;
            var ternaryIndexes = new List<int>();
            var ternaryBracketDepths = new List<int>();

            for (var i = 0; i < formula.Length; i++)
            {
                var currentChar = formula[i];
                var nextChar = i < formula.Length - 1 ? formula[i + 1].ToString() : string.Empty;
                var twoCharComparison = currentChar + nextChar;
                var nextNextChar = i < formula.Length - 2 ? formula[i + 2].ToString() : string.Empty;
                var threeCharComparison = twoCharComparison + nextNextChar;

                if (currentChar == ' ') continue; // Skip spaces
                if (currentChar == '\n') continue; // Skip new lines
                
                var ignoreEncapsulation = !encapsulateNext;

                if (Operations.TryGetValue(threeCharComparison, out var threeCharOperation))
                {
                    HandleOperation(expressions, threeCharOperation, i, ref useAssignment, ref encapsulateNext);
                    i += 2; // Skip the next two characters as they are part of the three-character operation
                }
                else if (Operations.TryGetValue(twoCharComparison, out var operationType))
                {
                    HandleOperation(expressions, operationType, i, ref useAssignment, ref encapsulateNext);
                    i++; // Skip the next character as it is part of the two-character operation
                }
                else if (Operations.TryGetValue(currentChar.ToString(), out var operation))
                {
                    HandleOperation(expressions, operation, i, ref useAssignment, ref encapsulateNext);
                }
                else if (currentChar == '?')
                {
                    // Go back to the expression list to find the last ( or the start of expression
                    ternaryDepth++;
                    ternaryCallers.Add(new TernaryCaller());
                    ternaryBracketDepths.Add(bracketDepth);
                    var index = 0;
                    var depth = 0;

                    for (var j = expressions.Count - 1; j >= 0; j--)
                    {
                        if (expressions[j] is OperationType.CloseBracket)
                        {
                            depth++;
                        }
                        else if (expressions[j] is OperationType.OpenBracket)
                        {
                            depth--;
                            if (depth >= 0) continue;
                        }

                        if (expressions[j] is not OperationType.OpenBracket and not OperationType.Assignment) continue;
                        if (depth > 0) continue;

                        index = j + 1;
                        break;
                    }

                    for (; index < expressions.Count;)
                    {
                        ternaryCallers[ternaryDepth].ConditionList.Add(expressions[index]);
                        expressions.RemoveAt(index);
                    }

                    ternaryIndexes.Add(index);
                }
                else if (currentChar == ':')
                {
                    if (ternaryDepth == -1)
                    {
                        Debug.LogError($"Unexpected ':' in formula: {formula}");
                        return false;
                    }

                    var index = ternaryIndexes[ternaryDepth];

                    for (; index < expressions.Count;)
                    {
                        ternaryCallers[ternaryDepth].IfTrue.Add(expressions[index]);
                        expressions.RemoveAt(index);
                    }
                }
                else if (char.IsDigit(currentChar) || currentChar == '.')
                {
                    numberBuilder.Clear();
                    while (i < formula.Length && (char.IsDigit(formula[i]) || formula[i] == '.'))
                    {
                        numberBuilder.Append(formula[i]);
                        i++;
                    }

                    if (numberBuilder.Length > 0)
                    {
                        var numberStr = numberBuilder.ToString();
                        if (numberStr.Contains('.'))
                            // If the number contains a dot, parse it as a float
                            expressions.Add(float.Parse(numberStr));
                        else
                            // Otherwise, parse it as an int
                            expressions.Add(int.Parse(numberStr));

                        i--;
                    }
                }
                else if (formula[i] == '\"' || formula[i] == '\'')
                {
                    // If the character is a quote, it indicates the start of a string
                    var str = ExpressionUtility.ExtractSurrounded(formula, i);

                    i += str.Length + 1;
                    expressions.Add(str);
                }
                else if (char.IsLetter(currentChar))
                {
                    var variable = ExpressionUtility.ExtractVariable(formula, i);
                    i += variable.Length - 1;

                    if (ExpressionUtility.IsReservedWord(variable))
                    {
                        expressions.Add(ExpressionUtility.GetReservedWordValue(variable));
                        continue;
                    }

                    if (i < formula.Length - 1 && formula[i + 1] == '.')
                    {
                        var propertyName = ExpressionUtility.ExtractVariable(formula, i + 2);
                        i += propertyName.Length + 1;

                        var leftProperties = "";
                        var parameters = new List<string>();
                        var genericTypes = new List<Type>();
                        var methodType = i != formula.Length - 1 && (formula[i + 1] == '(' || formula[i + 1] == '<')
                            ? AccessorType.Method
                            : AccessorType.Property;

                        if (methodType == AccessorType.Method)
                        {
                            if (formula[i + 1] == '<')
                            {
                                var genericType = ExpressionUtility.ExtractMethodParameters(formula, i, '<', '>');
                                i += genericType.Sum(type => type.Length) + 2;

                                if (genericType.Count > 1) i += genericType.Count - 1; // For the commas

                                foreach (var type in genericType)
                                {
                                    var t = ExpressionUtility.ExtractType(type);

                                    if (t == null)
                                    {
                                        Debug.LogError($"Generic type '{type}' not found");
                                        return false;
                                    }

                                    genericTypes.Add(t);
                                }
                            }

                            parameters = ExpressionUtility.ExtractMethodParameters(formula, i);
                            i += parameters.Sum(parameter => parameter.Length) + 2;

                            if (parameters.Count > 1) i += parameters.Count - 1; // For the commas
                        }

                        var loops = 0;

                        while (i != formula.Length - 1 && (formula[i + 1] == '.' || formula[i + 1] == '('))
                        {
                            if (formula[i + 1] == '.')
                            {
                                var property = ExpressionUtility.ExtractVariable(formula, i + 2);
                                i += property.Length + 1;

                                if (leftProperties.Length > 0) leftProperties += ".";

                                leftProperties += property;
                            }
                            else if (formula[i + 1] == '<')
                            {
                                var genericType = ExpressionUtility.ExtractInsideSurrounder(formula, i + 1, '<', '>');
                                i += genericType.Length + 2;

                                leftProperties += "<" + genericType + ">";
                            }
                            else if (formula[i + 1] == '(')
                            {
                                var methodParameters = ExpressionUtility.ExtractInsideSurrounder(formula, i + 1);
                                i += methodParameters.Length + 2;

                                leftProperties += "(" + methodParameters + ")";
                            }

                            loops++;

                            if (loops > 100)
                            {
                                Debug.LogError($"Too many loops in formula: {formula}");
                                return false;
                            }
                        }

                        IValue value;

                        if (variables.Find(v => v.Name == variable) is { } variableValue)
                        {
                            value = variableValue.Value;
                        }
                        else
                        {
                            var type = ExpressionUtility.ExtractType(variable, propertyName);

                            if (type == null)
                            {
                                Debug.LogError($"Type '{variable}' not found");
                                return false;
                            }

                            value = new MethodValue(type);
                        }

                        expressions.Add(methodType == AccessorType.Method
                            ? new AccessorCaller(value, propertyName, parameters, leftProperties, genericTypes)
                            : new AccessorCaller(value, propertyName, leftProperties));
                    }
                    else if (variables.Find(v => v.Name == variable) is { } variableValue)
                    {
                        expressions.Add(variableValue.Value);
                    }
                    else
                    {
                        Debug.LogError($"Variable '{variable}' not found");
                        return false;
                    }
                }
                else
                {
                    Debug.LogError($"Invalid character in formula: {currentChar}");
                    return false;
                }

                if (currentChar == '(')
                {
                    bracketDepth++;
                }
                else if (currentChar == ')')
                {
                    if (ternaryDepth != -1 && ternaryBracketDepths[ternaryDepth] == bracketDepth)
                    {
                        var index = ternaryIndexes[ternaryDepth];
                        var caller = ternaryCallers[ternaryDepth];

                        for (; index < expressions.Count - 1;)
                        {
                            caller.IfFalse.Add(expressions[index]);
                            expressions.RemoveAt(index);
                        }

                        expressions.Insert(expressions.Count - 1, caller);
                        ternaryDepth--;
                    }

                    bracketDepth--;

                    if (bracketDepth >= 0) continue;

                    Debug.LogError($"Unexpected ')' in formula: {formula}");
                    return false;
                }
                
                if (!ignoreEncapsulation && encapsulateNext)
                {
                    expressions.Add(OperationType.CloseBracket);
                    encapsulateNext = false;
                }
            }

            while (ternaryDepth != -1)
            {
                var index = ternaryIndexes[ternaryDepth];
                var caller = ternaryCallers[ternaryDepth];

                for (; index < expressions.Count;)
                {
                    caller.IfFalse.Add(expressions[index]);
                    expressions.RemoveAt(index);
                }

                expressions.Add(caller);
                ternaryDepth--;
            }

            if (useAssignment) expressions.Add(OperationType.CloseBracket);

            FormulaCache.Add(uid, formula, expressions);

            return EvaluateExpression(uid, expressions, variables);
        }

        private static object EvaluateExpression(string uid, List<object> expression, List<ExpressionVariable> variables)
        {
            var stack = new Stack<object>(expression.Count);
            var output = new List<object>(expression.Count);

            foreach (var token in expression)
            {
                switch (token)
                {
                    case OperationType operation and OperationType.OpenBracket:
                        stack.Push(operation);
                        break;
                    case OperationType.CloseBracket:
                    {
                        while (stack.Count > 0 && stack.Peek() is not OperationType.OpenBracket)
                        {
                            output.Add(stack.Pop());
                        }

                        stack.Pop(); // Remove the OpenBracket from the stack
                        break;
                    }
                    case OperationType operation:
                    {
                        while (stack.Count > 0 && stack.Peek() is OperationType topOperation && GetPrecedence(topOperation) >= GetPrecedence(operation))
                        {
                            output.Add(stack.Pop());
                        }

                        stack.Push(operation);
                        break;
                    }
                    case TernaryCaller ternaryCaller:
                    {
                        var condition = ExpressionUtility.ExtractValue(
                            EvaluateExpression(uid, ternaryCaller.ConditionList, variables),
                            uid, variables
                        );

                        if (condition is bool conditionValue)
                        {
                            output.Add(conditionValue
                                ? EvaluateExpression(uid, ternaryCaller.IfTrue, variables)
                                : EvaluateExpression(uid, ternaryCaller.IfFalse, variables));
                        }
                        else
                        {
                            Debug.LogError("Ternary condition is not a boolean");
                            return null;
                        }

                        break;
                    }
                    case AccessorCaller methodCaller:
                        output.Add(EvaluateAccessor(uid, methodCaller, variables));
                        break;
                    default:
                        output.Add(token);
                        break;
                }
            }

            while (stack.Count > 0) output.Add(stack.Pop());

            return EvaluatePostfix(uid, output, variables);
        }

        private static int GetPrecedence(OperationType operation)
        {
            return operation switch
            {
                OperationType.Or => 1,
                OperationType.And => 2,
                OperationType.Equal => 3,
                OperationType.NotEqual => 3,
                OperationType.GreaterThan => 4,
                OperationType.GreaterThanOrEqual => 4,
                OperationType.LessThan => 4,
                OperationType.LessThanOrEqual => 4,

                OperationType.Add => 5,
                OperationType.Substract => 5,
                OperationType.Multiply => 6,
                OperationType.Divide => 6,
                OperationType.Modulo => 6,

                OperationType.RightShift => 7,
                OperationType.LeftShift => 7,
                OperationType.BitwiseOr => 8,
                OperationType.BitwiseAnd => 9,
                OperationType.BitwiseNot => 10,
                OperationType.BitwiseXor => 10,
                _ => 0
            };
        }

        private static object EvaluatePostfix(string uid, List<object> postfix, List<ExpressionVariable> variables)
        {
            var stack = new Stack<object>(postfix.Count);

            foreach (var token in postfix)
            {
                if (token is OperationType operation)
                {
                    var originalRight = stack.Pop();
                    var originalLeft = stack.Pop();

                    dynamic right = ExpressionUtility.ExtractValue(originalRight, uid, variables);
                    dynamic left = ExpressionUtility.ExtractValue(originalLeft, uid, variables);

                    if (operation == OperationType.Assignment)
                    {
                        var convertedValue = originalLeft is CustomValue ? right : ExpressionUtility.ConvertTo(right, left.GetType());

                        switch (originalLeft)
                        {
                            case IValue leftValue:
                                leftValue.Value = convertedValue;
                                break;
                            case AccessorCaller { AccessorType: AccessorType.Property } methodCaller:
                                methodCaller.AssignValue(convertedValue);
                                break;
                        }

                        stack.Push(originalLeft);
                        continue;
                    }

                    if (right is bool && operation is >= OperationType.Add and <= OperationType.Modulo)
                    {
                        right = Convert.ToInt32(right);
                    }

                    if (left is bool && operation is >= OperationType.Add and <= OperationType.Modulo)
                    {
                        left = Convert.ToInt32(left);
                    }

                    stack.Push(operation switch
                    {
                        OperationType.Add => left + right,
                        OperationType.Substract => left - right,
                        OperationType.Multiply => left * right,
                        OperationType.Divide => left / right,
                        OperationType.Modulo => left % right,

                        OperationType.Equal => left == right,
                        OperationType.NotEqual => left != right,
                        OperationType.GreaterThan => left > right,
                        OperationType.GreaterThanOrEqual => left >= right,
                        OperationType.LessThan => left < right,
                        OperationType.LessThanOrEqual => left <= right,
                        OperationType.And => left && right,
                        OperationType.Or => left || right,

                        OperationType.RightShift => left >> right,
                        OperationType.LeftShift => left << right,
                        OperationType.BitwiseAnd => left & right,
                        OperationType.BitwiseOr => left | right,
                        OperationType.BitwiseNot => ~left,
                        OperationType.BitwiseXor => left ^ right,
                        _ => throw new InvalidOperationException("Invalid operation")
                    });
                }
                else
                {
                    stack.Push(token);
                }
            }

            return stack.Pop();
        }

        /**
         * Handles the operation and adds it to the expressions list. Returns true if the operation is an assignment operation.
         */
        private static void HandleOperation(List<object> expressions, OperationType operationType, int i, ref bool useAssignment, ref bool encapsulateNext)
        {
            switch (operationType)
            {
                case > OperationType.Assignment:
                    expressions.Add(OperationType.Assignment);
                    expressions.Add(expressions[0]);
                    expressions.Add(operationType switch
                    {
                        OperationType.AssignmentAdd => OperationType.Add,
                        OperationType.AssignmentSubstract => OperationType.Substract,
                        OperationType.AssignmentMultiply => OperationType.Multiply,
                        OperationType.AssignmentDivide => OperationType.Divide,
                        OperationType.AssignmentModulo => OperationType.Modulo,
                        OperationType.AssignmentAnd => OperationType.BitwiseAnd,
                        OperationType.AssignmentOr => OperationType.BitwiseOr,
                        OperationType.AssignmentXor => OperationType.BitwiseXor,
                        OperationType.AssignmentLeftShift => OperationType.LeftShift,
                        OperationType.AssignmentRightShift => OperationType.RightShift,
                        _ => throw new InvalidOperationException("Invalid assignment operation")
                    });
                    expressions.Add(OperationType.OpenBracket);

                    useAssignment = true;
                    break;
                case OperationType.Substract when i == 0 || expressions.Count == 0 || expressions[^1] is OperationType:
                    if (expressions.Count != 0 && (expressions[^1] is not OperationType || expressions[^1] is OperationType.CloseBracket))
                    {
                        expressions.Add(OperationType.Add);
                    }
                    expressions.Add(OperationType.OpenBracket);
                    expressions.Add(-1);
                    expressions.Add(OperationType.Multiply);
                    encapsulateNext = true;
                    break;
                case OperationType.Not:
                    expressions.Add(false);
                    expressions.Add(OperationType.Equal);
                    break;
                default:
                    expressions.Add(operationType);
                    break;
            }
        }

        public static object EvaluateAccessor(string uid, AccessorCaller caller, List<ExpressionVariable> variables)
        {
            var callerValue = caller.Instance.Type == typeof(Type) ? null : ExpressionUtility.ExtractValue(caller.Instance, uid, variables);
            var callerType = caller.Instance.Type.FullName == "System.RuntimeType"
                ? (Type)caller.Instance.Value
                : callerValue.GetType();

            if (caller.AccessorType is AccessorType.Property)
            {
                var field = callerType.GetField(caller.Property);

                if (field == null)
                {
                    var property = callerType.GetProperty(caller.Property);

                    if (property == null)
                    {
                        Debug.LogError($"Property and field '{caller.Property}' not found in '{callerType}'");
                        return null;
                    }

                    if (property.CanRead)
                    {
                        caller.Result = new MethodValue(property.GetValue(callerValue));
                    }
                }
                else
                {
                    caller.Result = new MethodValue(field.GetValue(callerValue));
                }
            }
            else
            {
                var parameters = caller.Parameters
                    .Select(parameter => Process(uid, parameter, variables).FirstOrDefault())
                    .Select(obj => ExpressionUtility.ExtractValue(obj, uid, variables))
                    .ToList();

                var method = callerType.GetMethod(caller.Property, parameters.Select(p => p.GetType()).ToArray());

                if (method == null)
                {
                    Debug.LogError($"Method '{caller.Property}' not found in '{callerType}' with parameters {string.Join(", ", parameters)}");
                    return null;
                }

                if (method.IsGenericMethodDefinition && caller.GenericTypes.Count > 0)
                {
                    try
                    {
                        method = method.MakeGenericMethod(caller.GenericTypes.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to apply generic types to method '{caller.Property}' in '{callerType}': {ex.Message}");
                        return null;
                    }
                }

                var methodParameters = method.GetParameters();
                for (var i = 0; i < methodParameters.Length; i++)
                {
                    if (methodParameters[i].ParameterType.IsEnum && parameters[i] is string enumValue)
                    {
                        parameters[i] = Enum.Parse(methodParameters[i].ParameterType, enumValue);
                    }
                }
                
                if (method.ReturnType == typeof(void))
                {
                    method.Invoke(callerValue, parameters.ToArray());
                    caller.Result = new MethodValue(callerValue);
                }
                else
                {
                    caller.Result = new MethodValue(method.Invoke(callerValue, parameters.ToArray()));
                }
            }

            if (caller.LeftMethod.Length <= 0) return caller;

            var variableName = variables[^2].Name + variables.Count;
            var newList = new List<ExpressionVariable>(variables) { new(variableName, caller.Result) };

            return Process(uid, variableName + "." + caller.LeftMethod, newList).FirstOrDefault();
        }
    }
}
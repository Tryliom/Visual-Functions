using System;
using System.Collections.Generic;

namespace VisualFunctions
{
    public class MethodValue : IValue
    {
        public MethodValue(object value)
        {
            Value = value;
        }

        public object Value { get; set; }

        public Type Type => Value.GetType();
        
        public IValue Clone()
        {
            return new MethodValue(Value);
        }
    }

    public enum AccessorType
    {
        Property,
        Method
    }

    public class AccessorCaller
    {
        public readonly AccessorType AccessorType;
        public readonly List<Type> GenericTypes;
        public readonly IValue Instance;
        public readonly string LeftMethod;
        public readonly List<string> Parameters;
        public readonly string Property;

        public AccessorCaller(IValue instance, string property, string leftMethod)
        {
            Instance = instance;
            Property = property;
            LeftMethod = leftMethod;
            AccessorType = AccessorType.Property;
            Parameters = new List<string>();
            GenericTypes = new List<Type>();
        }

        public AccessorCaller(IValue instance, string property, List<string> parameters, string leftMethod,
            List<Type> genericTypes)
        {
            Instance = instance;
            Property = property;
            LeftMethod = leftMethod;
            AccessorType = AccessorType.Method;
            Parameters = parameters;
            GenericTypes = genericTypes ?? new List<Type>();
        }

        public IValue Result { get; set; }

        public void AssignValue(object value)
        {
            if (AccessorType == AccessorType.Method) return;

            var instanceType = Instance.Type;
            var propertyInfo = instanceType.GetProperty(Property);
            var fieldInfo = instanceType.GetField(Property);

            if (propertyInfo != null)
            {
                if (!propertyInfo.CanWrite)
                {
                    throw new Exception($"Property '{Property}' is read-only from type '{instanceType.Name}'.");
                }
                
                if (instanceType.IsValueType && !instanceType.IsGenericType)
                {
                    var tempValue = Instance.Value is IRefValue refValue ? refValue.RefValue : Instance.Value;
                    propertyInfo.SetValue(tempValue, value);
                    Instance.Value = tempValue;
                }
                else
                {
                    propertyInfo.SetValue(Instance.Value is AccessorCaller caller ? caller.Result.Value : Instance.Value, value);
                }
            }
            else if (fieldInfo != null)
            {
                if (fieldInfo.IsInitOnly)
                {
                    throw new Exception($"Field '{Property}' is read-only from type '{instanceType.Name}'.");
                }
                
                if (instanceType.IsValueType && !instanceType.IsGenericType)
                {
                    var tempValue = Instance.Value is IRefValue refValue ? refValue.RefValue : Instance.Value;
                    fieldInfo.SetValue(tempValue, value);
                    Instance.Value = tempValue;
                }
                else
                {
                    fieldInfo.SetValue(Instance.Value is AccessorCaller caller ? caller.Result.Value : Instance.Value, value);
                }
            }
            else
            {
                throw new Exception($"Property or field '{Property}' not found in type '{instanceType.Name}'.");
            }
        }
    }
}
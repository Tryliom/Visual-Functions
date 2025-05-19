using System;

namespace VisualFunctions
{
    public interface IValue
    {
        public object Value { get; set; }
        public Type Type { get; }

        public IValue Clone();
    }

    public interface IValue<TType> : IValue
    {
        public new TType Value { get; set; }
    }
    
    public class TempIValue : IValue
    {
        public TempIValue(object value)
        {
            Value = value;
        }

        public object Value { get; set; }

        public Type Type => Value.GetType();
        
        public IValue Clone()
        {
            return new TempIValue(Value);
        }
    }
}
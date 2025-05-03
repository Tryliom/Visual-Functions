using System;

namespace TryliomFunctions
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
}
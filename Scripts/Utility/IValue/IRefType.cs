using System;

namespace VisualFunctions
{
    /**
     * Used to get a different type in a Reference type when processed
     */
    public interface IRefType
    {
        public Type Type { get; }
    }
}
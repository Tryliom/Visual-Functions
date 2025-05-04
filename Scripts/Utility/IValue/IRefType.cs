using System;

namespace TryliomFunctions
{
    /**
     * Used to get a different type in a Reference type when processed
     */
    public interface IRefType
    {
        Type Type { get; }
    }
}
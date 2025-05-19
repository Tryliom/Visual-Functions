using System.Runtime.CompilerServices;
using UnityEngine;

namespace VisualFunctions
{
    public abstract class PerformanceTest : ScriptableObject
    {
        public Functions OnStart;
        public Functions FunctionsToTest;

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public abstract void RunCode();
    }
}
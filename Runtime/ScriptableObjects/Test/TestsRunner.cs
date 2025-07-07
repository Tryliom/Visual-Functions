using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [CreateAssetMenu(fileName = "TestsRunner", menuName = "Visual Functions/Test/TestsRunner")]
    public class TestsRunner : ScriptableObject
    {
        [SerializeField] private int _testCount = 10000;
        public PerformanceTest[] PerformanceTests;
        public UnitTest[] UnitTests;

        public void RunPerformanceTests()
        {
            var totalTimeCode = 0f;
            var totalCounterCode = 0;

            var totalTimeFunction = 0f;
            var totalMemoryFunction = 0f;
            var totalCounterFunction = 0;

            foreach (var test in PerformanceTests)
            {
                var result = Test(test);

                ShowResults(result, test.GetType());

                totalTimeCode += result.CodeTime;
                totalCounterCode++;
                totalTimeFunction += result.FunctionTime;
                totalMemoryFunction += result.FunctionMemory;
                totalCounterFunction++;
            }

            if (totalCounterFunction > 0 && totalCounterCode > 0)
            {
                Debug.Log($"Average Function / Code: {totalTimeFunction / totalCounterFunction / (totalTimeCode / totalCounterCode):F2}x slower and " +
                          $"{totalMemoryFunction} MB used");
            }
        }

        public void RunTest(PerformanceTest performanceTest)
        {
            ShowResults(Test(performanceTest), performanceTest.GetType());
        }

        private TestResult Test(PerformanceTest performanceTest)
        {
            performanceTest.OnStart.Invoke();

            var counterCode = new Counter(performanceTest.RunCode);

            for (var i = 0; i < _testCount; i++) counterCode.Test();

            var resultCode = counterCode.GetTime();
            
            performanceTest.OnStart.Invoke();
            
            var counterFunction = new Counter(performanceTest.FunctionsToTest.Invoke);

            for (var i = 0; i < _testCount; i++) counterFunction.Test();

            var resultFunction = counterFunction.GetTime();
            var resultFunctionMemory = counterFunction.GetAllocatedMemoryMb();

            return new TestResult(resultCode, resultFunction, resultFunctionMemory);
        }
        
        public void RunUnitTests()
        {
            foreach (var test in UnitTests)
            {
                try
                {
                    test.Tests.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{test.GetType().Name} - Failed: {e.Message}");
                }
            }
        }

        private static void ShowResults(TestResult result, Type testType)
        {
            Debug.Log($"{testType.Name} - Function takes {result.FunctionTime} ms and used {result.FunctionMemory:F2}mb ({result.FunctionTime / result.CodeTime:F2}x slower) | Code {result.CodeTime} ms");
        }

        private class Counter
        {
            private readonly Stopwatch _stopwatch;
            private readonly Action _testedAction;
            private Recorder _rec;

            public Counter(Action testedAction)
            {
                _stopwatch = Stopwatch.StartNew();
                _testedAction = testedAction;
                _rec = Recorder.Get("GC.Alloc");
                _rec.enabled = false;
#if !UNITY_WEBGL
                _rec.FilterToCurrentThread();
#endif
                _rec.enabled = true;
            }

            public void Test()
            {
                _testedAction?.Invoke();
            }

            public float GetTime()
            {
                _stopwatch.Stop();
                return _stopwatch.ElapsedTicks;
            }
            
            public float GetAllocatedMemoryMb()
            {
                if (_rec == null) return 0;

                _rec.enabled = false;
#if !UNITY_WEBGL
                _rec.CollectFromAllThreads();
#endif

                var res = _rec.sampleBlockCount;
                
                _rec = null;
                
                return res / (1024f * 1024f);
            }
        }

        private class TestResult
        {
            public readonly float CodeTime;
            public readonly float FunctionTime;
            public readonly float FunctionMemory;
            
            /**
             * Takes time in ticks, converts it to milliseconds with a maximum of precision.
             */
            public TestResult(float codeTime = 0f, float functionTime = 0f, float functionMemory = 0f)
            {
                CodeTime = codeTime / 10000f;
                FunctionTime = functionTime / 10000f;
                FunctionMemory = functionMemory;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TestsRunner))]
    public class TestsRunnerEditor : Editor
    {
        private void OnEnable()
        {
            var testsRunner = (TestsRunner)target;
            testsRunner.PerformanceTests = Resources.LoadAll<PerformanceTest>("").ToArray();
            testsRunner.UnitTests = Resources.LoadAll<UnitTest>("").ToArray();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var testsRunner = (TestsRunner)target;

            if (GUILayout.Button("Run All Performance Tests")) testsRunner.RunPerformanceTests();
            if (GUILayout.Button("Run All Unit Tests")) testsRunner.RunUnitTests();
            
            EditorGUILayout.LabelField("Performance Tests", EditorStyles.boldLabel);

            foreach (var test in testsRunner.PerformanceTests)
            {
                if (!GUILayout.Button($"Run {ObjectNames.NicifyVariableName(test.name)}")) continue;
                    
                testsRunner.RunTest(test);
            }
            
            EditorGUILayout.LabelField("Unit Tests", EditorStyles.boldLabel);
            
            foreach (var test in testsRunner.UnitTests)
            {
                if (!GUILayout.Button($"Run {ObjectNames.NicifyVariableName(test.name)}")) continue;
                
                try
                {
                    test.Tests.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{test.GetType().Name} - Failed: {e.Message}");
                }
            }
        }
    }
#endif
}
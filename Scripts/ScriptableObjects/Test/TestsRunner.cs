using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TryliomFunctions
{
    [CreateAssetMenu(fileName = "TestsRunner", menuName = "TryliomFunctions/Test/TestsRunner")]
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
            var totalCounterFunction = 0;

            foreach (var test in PerformanceTests)
            {
                var result = Test(test);

                ShowResults(result, test.GetType());

                totalTimeCode += result.CodeTime;
                totalCounterCode++;
                totalTimeFunction += result.FunctionTime;
                totalCounterFunction++;
            }

            if (totalCounterFunction > 0 && totalCounterCode > 0)
            {
                Debug.Log($"Average Function / Code: {totalTimeFunction / totalCounterFunction / (totalTimeCode / totalCounterCode):F2}x slower");
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
            
            var counterFunction = new Counter(performanceTest.FunctionsToTest.Invoke);

            for (var i = 0; i < _testCount; i++) counterFunction.Test();

            var resultFunction = counterFunction.GetTime();

            return new TestResult
            {
                CodeTime = resultCode,
                FunctionTime = resultFunction
            };
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
            Debug.Log($"{testType.Name} - Code {result.CodeTime} ticks - Function {result.FunctionTime} ticks ({result.FunctionTime / result.CodeTime:F2}x slower)");
        }

        private class Counter
        {
            private readonly Stopwatch _stopwatch;
            private readonly Action _testedAction;

            public Counter(Action testedAction)
            {
                _stopwatch = Stopwatch.StartNew();
                _testedAction = testedAction;
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
        }

        private class TestResult
        {
            public float CodeTime;
            public float FunctionTime;
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
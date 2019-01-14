using Microsoft.Xunit.Performance;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

namespace FixedMath.Numerics.PerformanceTests
{
    public class UnitTest1
    {
        //[Fact]
        public void Test1()
        {

        }

        //[Benchmark]
        void TestMethod()
        {
            // Any per-test-case setup can go here.
            foreach (var iteration in Benchmark.Iterations)
            {
                // Any per-iteration setup can go here.
                using (iteration.StartMeasurement())
                {
                    // Code to be measured goes here.
                    Fix64 x = 1.2f;
                    Fix64 y = 2.3f;
                    var z = x + y;
                }
                // ...per-iteration cleanup
            }
            // ...per-test-case cleanup
        }

        public static IEnumerable<object[]> InputData()
        {
            var args = new string[] { "foo", "bar", "baz" };
            foreach (var arg in args)
                // Currently, the only limitation of this approach is that the
                // types passed to the [Benchmark]-annotated test must be serializable.
                yield return new object[] { new string[] { arg } };
        }

        // NoInlining prevents aggressive optimizations that
        // could render the benchmark meaningless
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string FormattedString(string a, string b, string c, string d)
        {
            return string.Format("{0}{1}{2}{3}", a, b, c, d);
        }

        // This benchmark will be executed 3 different times,
        // with { "foo" }, { "bar" }, and { "baz" } as args.
        [MeasureGCCounts]
        //[Benchmark(InnerIterationCount = 10)]
        [MemberData(nameof(InputData))]
        public static void TestMultipleStringInputs(string[] args)
        {
            foreach (BenchmarkIteration iter in Benchmark.Iterations)
            {
                using (iter.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        FormattedString(args[0], args[0], args[0], args[0]);
                    }
                }
            }
        }
    }
}

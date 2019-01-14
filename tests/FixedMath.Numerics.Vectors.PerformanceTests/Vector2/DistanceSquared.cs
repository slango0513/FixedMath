// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Xunit.Performance;
using Single = FixedMath.Fix64;
using Vector2 = FixedMath.Numerics.Fix64Vector2;

namespace FixedMath.Numerics.Tests
{
    public static partial class Perf_Vector2
    {
        [Benchmark(InnerIterationCount = VectorTests.DefaultInnerIterationsCount)]
        public static void DistanceSquaredBenchmark()
        {
            Single expectedResult = 8.0f;

            foreach (var iteration in Benchmark.Iterations)
            {
                Single actualResult;

                using (iteration.StartMeasurement())
                {
                    actualResult = DistanceSquaredTest();
                }

                VectorTests.AssertEqual(expectedResult, actualResult);
            }
        }

        public static Single DistanceSquaredTest()
        {
            Single result = 0.0f;

            for (var iteration = 0; iteration < Benchmark.InnerIterationCount; iteration++)
            {
                // The inputs aren't being changed and the output is being reset with each iteration, so a future
                // optimization could potentially throw away everything except for the final call. This would break
                // the perf test. The JitOptimizeCanary code below does modify the inputs and consume each output.
                result = Vector2.DistanceSquared(VectorTests.Vector2Value, VectorTests.Vector2ValueInverted);
            }

            return result;
        }

        [Benchmark(InnerIterationCount = VectorTests.DefaultInnerIterationsCount)]
        public static void DistanceSquaredJitOptimizeCanaryBenchmark()
        {
            Single expectedResult = 134217728.0f;

            foreach (var iteration in Benchmark.Iterations)
            {
                Single actualResult;

                using (iteration.StartMeasurement())
                {
                    actualResult = DistanceSquaredJitOptimizeCanaryTest();
                }

                VectorTests.AssertEqual(expectedResult, actualResult);
            }
        }

        public static Single DistanceSquaredJitOptimizeCanaryTest()
        {
            Single result = 0.0f;
            var value = VectorTests.Vector2Value;

            for (var iteration = 0; iteration < Benchmark.InnerIterationCount; iteration++)
            {
                value += VectorTests.Vector2Delta;
                result += Vector2.DistanceSquared(value, VectorTests.Vector2ValueInverted);
            }

            return result;
        }
    }
}

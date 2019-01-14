using BenchmarkDotNet.Attributes;
using FixedMath;
using FixedMath.Numerics;
using System;
using System.Text;
//using Single = FixedMath.Fix64;
//using Vector2 = FixedMath.Numerics.Fix64Vector2;
//using Vector3 = FixedMath.Numerics.Fix64Vector3;
//using Vector2 = System.Numerics.Vector2;

namespace ConsoleApp1
{
    public class DivVsMulFactor
    {
        public static Random r = new Random();

        // 475 ns //295
        [Benchmark]
        public Fix64 A64()
        {
            var a = new Fix64Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
            var b = new Fix64Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
            return Fix64Vector2.Distance(a, b);
        }

        // 108 ns //68
        [Benchmark]
        public Fix64 A64Sq()
        {
            var a = new Fix64Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
            var b = new Fix64Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
            return Fix64Vector2.DistanceSquared(a, b);
        }

        //// 252 ns //239
        //[Benchmark]
        //public Fix16 B16()
        //{
        //    var a = new Fix16Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
        //    var b = new Fix16Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
        //    return Fix16Vector2.Distance(a, b);
        //}

        //// 101 ns //94
        //[Benchmark]
        //public Fix16 B16Sq()
        //{
        //    var a = new Fix16Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
        //    var b = new Fix16Vector2(r.NextDouble() * 11111, r.NextDouble() * 11111);
        //    return Fix16Vector2.DistanceSquared(a, b);
        //}

    }


    public class ABC2
    {
        public static readonly Fix64 EN2 = Fix64.One / 100;
        [Benchmark]
        public Fix64 A()
        {
            return Fix64.One + EN2 * 28;
        }
        [Benchmark]
        public Fix64 B()
        {
            return Fix64.One + (Fix64)0.28M;
        }
    }

    public class AtanRunner
    {
        public static Random r = new Random();

        // 388 ns
        [Benchmark]
        public Fix64 Atan()
        {
            Fix64 val = r.NextDouble() * 11111;
            return MathFix.Atan(val);
        }

        // 488 ns
        [Benchmark]
        public Fix64 AtanAlt()
        {
            Fix64 val = r.NextDouble() * 11111;
            return MathFix.Atan2(val, 1);
        }
    }

    public class ClampRunner
    {
        public static Random r = new Random();

        [Benchmark]
        public void ClampSinValue()
        {
            for (int j = 0; j < 1000; j++)
            {
                Fix64 angle = r.NextDouble() * 11111;

                Fix64 clamped2Pi = angle;
                for (int i = 0; i < 29; ++i)
                {
                    clamped2Pi %= (7244019458077122842 >> i);
                }
            }
        }

        [Benchmark]
        public void ClampSinValue_Alt()
        {
            for (int j = 0; j < 1000; j++)
            {
                Fix64 angle = r.NextDouble() * 11111;

                Fix64 clamped2Pi_2 = angle % 0x6487ED511; // 改
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var e = Math.E;
            var fixedE = MathFix.E;

            Console.WriteLine($"Math.Log {Math.Log(0.21412f)} {Math.Log(4123)}");
            Console.WriteLine($"Math.Log {MathFix.Log(0.21412f)} {MathFix.Log(4123)}");

            Console.WriteLine($"MathFix.Log(,2) {Math.Log(4, 3)} {Math.Log(16, 0.321f)}");
            //Console.WriteLine($"MathFix.Log(,2) {MathFix.Log2(4)} {MathFix.Log2(16)}");

            //for (int i = -10; i < 20; i++)
            //{
            //    var b = BitConverter.SingleToInt32Bits(i);
            //    //Console.WriteLine($"{b}");
            //}

            //Console.WriteLine(Convert.ToUInt64(3.7f));

            //var info = NumberFormatInfo.InvariantInfo;
            //var info2 = NumberFormatInfo.CurrentInfo;
            //Console.WriteLine($"{info.CurrencyDecimalDigits}");


            //Console.WriteLine(new Fix64Vector2((Fix64)0.123456789012345678901234567890123m, (Fix64)0.123456789012345678901234567890123m));
            //Console.WriteLine(int.MaxValue);
            //Console.WriteLine($"{0x7FFFFFFF} {0x7F800000}");


            //Console.WriteLine($"{Math.Sqrt(-2.3)}");
            //Console.WriteLine($"{MathF.Sqrt(-2.3f)}");

            //Fix64 s = 0.0f;
            //Console.WriteLine($"{MathFix.Sqrt(0)}");
            //Console.WriteLine($"{Fix64Vector2.Normalize(Fix64Vector2.Zero)} {Vector2.Normalize(Vector2.Zero)}");
            //Console.WriteLine($"{0.0f / 0} {1.0f / 0} {-1.0f / 0}");
            //Console.WriteLine($"{Fix64.Zero / 0} {Fix64.One / s} {-Fix64.One / s}");

            //Console.WriteLine($"{Fix64.NaN + 1213124123.15123f} {Fix64.NaN - 1213124123.15123f}");
            //Console.WriteLine($"{float.NaN + 1213124123.15123f} {float.NaN - 1213124123.15123f}");
            //Console.WriteLine($"{Fix64.NegativeInfinity + 1213124123.15123f} {Fix64.NegativeInfinity - 1213124123.15123f}");
            //Console.WriteLine($"{float.NegativeInfinity + 1213124123.15123f} {float.NegativeInfinity - 1213124123.15123f}");
            //Console.WriteLine($"{Fix64.PositiveInfinity + 1213124123.15123f} {Fix64.PositiveInfinity - 1213124123.15123f}");
            //Console.WriteLine($"{float.PositiveInfinity + 1213124123.15123f} {float.PositiveInfinity - 1213124123.15123f}");


            //Console.WriteLine($"{-3.2f * float.NegativeInfinity} {3.2f * float.PositiveInfinity}");
            //Console.WriteLine($"{float.NegativeInfinity * float.PositiveInfinity} {float.NegativeInfinity + float.NegativeInfinity}");


            //Fix64Vector2 a = new Fix64Vector2(Fix64.NaN, 0.0f);
            //Fix64Vector2 actual = -a;
            //Console.WriteLine($"{Fix64.IsNaN(actual.X)} {0.0f.Equals(actual.Y)} {actual.Y.Equals(0.0f)}");

            //Fix64Vector2 a = new Fix64Vector2(); // no parameter, default to 0.0f
            //Console.WriteLine(a.X);
            //Fix64Vector2 actual = Fix64Vector2.Normalize(a);

            //Console.WriteLine($" -  actual: {actual} {actual.X} {actual.Y}");
            //Console.WriteLine($" -     actual.x is nan: {Fix64.IsNaN(actual.X)} actual.y is nan: {Fix64.IsNaN(actual.Y)}");



            // 4294967294
            // 8589934591
            // 2147483647
            //Fix64Vector2 a = new Fix64Vector2(Fix64.MinValue, Fix64.MinValue);
            //Fix64Vector2 b = new Fix64Vector2(Fix64.MaxValue, Fix64.MaxValue);

            //Fix64 actual = Fix64Vector2.Dot(a, b);

            //Console.WriteLine(Fix64.NegativeInfinity);
            //Console.WriteLine(actual);

            //Vector2 v1 = new Vector2(-2.5f, 2.0f);
            //Console.WriteLine($"{Vector2.SquareRoot(v1).X}");
            //Console.WriteLine($"{Single.NaN == Vector2.SquareRoot(v1).X}");
            //Console.WriteLine($"{ Vector2.SquareRoot(v1).X == Single.NaN }");
            //Console.WriteLine($"{Single.NaN.Equals(Vector2.SquareRoot(v1).X)}");
            //Console.WriteLine($"{ Vector2.SquareRoot(v1).X.Equals(Single.NaN)}");


            //Fix64Vector2 a = new Fix64Vector2(2222, 2222);
            //Fix64Vector2 actual = Fix64Vector2.Normalize(a);
            //Fix64Vector2 expected = new Fix64Vector2(0, 0);
            //Console.WriteLine(a);
            //Console.WriteLine(actual);
            //Console.WriteLine(Fix64.MaxValue);

            //Console.WriteLine(Fix64.MinValue);
            //var a = -1.0f;
            //Fix64 b = 0.0f;
            //var res = a / b;
            //Console.WriteLine($"{Fix64.IsNaN(res)} {Fix64.IsNegativeInfinity(res)} {Fix64.IsPositiveInfinity(res)}  {Fix64.IsInfinity(res)} | {(sbyte)res} {(int)res} {(long)res}");


            //var a2 = 1.0f;
            //Fix64 b2 = 0.0f;
            //var res2 = a2 / b2;
            //Console.WriteLine($"{Fix64.IsNaN(res2)} {Fix64.IsNegativeInfinity(res2)} {Fix64.IsPositiveInfinity(res2)}  {Fix64.IsInfinity(res2)} | {(sbyte)res2} {(int)res2} {(long)res2}");

            //var a3 = 0.0f;
            //Fix64 b3 = 0.0f;
            //var res3 = a3 / b3;
            //Console.WriteLine($"{Fix64.IsNaN(res3)} {Fix64.IsNegativeInfinity(res3)} {Fix64.IsPositiveInfinity(res3)}  {Fix64.IsInfinity(res3)} | {(sbyte)res3} {(int)res3} {(long)res3}");

            //var result = VectorTests.Vector2Value;

            //for (int i = 0; i < 46340 + 0; i++)
            //{
            //    result += Fix64Vector2.Normalize(result);
            //}

            //2147341251.6827721973
            //2147433931.5369719965
            //2147483647 after
            //- 2147440682.6087354838
            //Console.WriteLine($"long {long.MaxValue} {long.MinValue}");
            //Console.WriteLine($"int {int.MaxValue} {int.MinValue}");
            //Console.WriteLine($"float {float.MaxValue} {float.MinValue}");
            //Console.WriteLine($"{result}: {result.X.RawValue}, {result.Y.RawValue}");
            //var result = Fix64Vector2.Distance(new Fix64Vector2(-1.0f, 1.0f), new Fix64Vector2(1.0f, -1.0f));
            //Console.WriteLine(result);

            //Console.WriteLine(0x3243F6A88);
            //long x = 1L << 32;
            //Console.WriteLine(x);
            //Console.WriteLine((float)13493037704 / x);

            //BenchmarkRunner.Run<ClampRunner>();
            //Console.WriteLine("Done");

            //var v1 = new System.Numerics.Vector2(0, 0);
            //var nv1 = System.Numerics.Vector2.Normalize(v1);
            //Console.WriteLine(nv1);

            //var v2 = new Fix64Vector2(0, 0);
            //var nv2 = Fix64Vector2.Normalize(v2);

            //Console.WriteLine(nv2);

            //var v1 = Fix64.MaxValue;
            //var v2 = Fix64.MinValue;

            // 2147483647.9999999998  9223372036854775807 
            // -2147483648 -9223372036854775808
            //Console.WriteLine($"v1 {v1} ");
            //Console.WriteLine($"v2 {v2}");
            //Console.WriteLine($"v1R {v1.RawValue}");
            //Console.WriteLine($"v2R {v2.RawValue }");

            //Console.WriteLine((Fix64)0.28M);
            //Console.WriteLine(Fix64.One / 100 * 28);

            //float a = -1.5f;
            //float b = 1.3f;
            //Console.WriteLine(+a);
            //Console.WriteLine(+b);


            //var expectedResult = new Vector2(-1.0f, 1.0f);
            //Vector2 actualResult;
            //actualResult = AddFunctionTest();
            //Console.WriteLine($"expected: {expectedResult} actualResult {actualResult}");
        }

        //public static Vector2 AddFunctionTest()
        //{
        //    var result = new Vector2(-1.0f, 1.0f);

        //    for (var iteration = 0; iteration < VectorTests.DefaultInnerIterationsCount; iteration++)
        //    {
        //        //result = Vector2.Add(result, new Vector2(1.0f / VectorTests.DefaultInnerIterationsCount, -1.0f / VectorTests.DefaultInnerIterationsCount));
        //        result = Vector2.Add(result, VectorTests.Vector2Delta);
        //        if (iteration % 100000 == 0)
        //        {
        //            Console.WriteLine($"{iteration} {result}");
        //        }
        //    }

        //    return result;
        //}




        static string FloatToBinary(float f)
        {
            var builder = new StringBuilder();
            foreach (var @byte in BitConverter.GetBytes(f))
            {
                for (int i = 0; i < 8; i++)
                {
                    builder.Insert(0, ((@byte >> i) & 1) == 1 ? "1" : "0");
                }
            }
            var s = builder.ToString();
            return $"{s.Substring(0, 1)} {s.Substring(1, 8)} {s.Substring(9)}"; //sign exponent mantissa
        }
    }
}

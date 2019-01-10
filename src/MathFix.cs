using System;

namespace FixedMath
{
    public class MathFix
    {
        /// <summary>
        /// Returns a number indicating the sign of a Fix64 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        public static int Sign(Fix64 value)
        {
            return value.RawValue < 0 ? -1 :
                value.RawValue > 0 ? 1 : 0;
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
        /// </summary>
        public static Fix64 Abs(Fix64 value)
        {
            if (value.RawValue == Fix64.MIN_VALUE)
            {
                return Fix64.MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.RawValue >> 63;
            return new Fix64((value.RawValue + mask) ^ mask);
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// FastAbs(Fix64.MinValue) is undefined.
        /// </summary>
        public static Fix64 FastAbs(Fix64 value)
        {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.RawValue >> 63;
            return new Fix64((value.RawValue + mask) ^ mask);
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static Fix64 Floor(Fix64 value)
        {
            // Just zero out the fractional part
            return new Fix64((long)((ulong)value.RawValue & 0xFFFFFFFF00000000));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static Fix64 Ceiling(Fix64 value)
        {
            var hasFractionalPart = (value.RawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + Fix64.One : value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static Fix64 Round(Fix64 value)
        {
            var fractionalPart = value.RawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);
            if (fractionalPart < 0x80000000)
            {
                return integralPart;
            }
            if (fractionalPart > 0x80000000)
            {
                return integralPart + Fix64.One;
            }
            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart.RawValue & Fix64.ONE) == 0 ? integralPart
                       : integralPart + Fix64.One;
        }

        /// <summary>
        /// Returns 2 raised to the specified power.
        /// Provides at least 6 decimals of accuracy.
        /// </summary>
        public static Fix64 Pow2(Fix64 x)
        {
            if (x.RawValue == 0)
            {
                return Fix64.One;
            }

            // Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
            bool neg = x.RawValue < 0;
            if (neg)
            {
                x = -x;
            }

            if (x == Fix64.One)
            {
                return neg ? Fix64.One / (Fix64)2 : (Fix64)2;
            }
            if (x >= Fix64.Log2Max)
            {
                return neg ? Fix64.One / Fix64.MaxValue : Fix64.MaxValue;
            }
            if (x <= Fix64.Log2Min)
            {
                return neg ? Fix64.MaxValue : Fix64.Zero;
            }

            /* The algorithm is based on the power series for exp(x):
             * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
             * 
             * From term n, we get term n+1 by multiplying with x/n.
             * When the sum term drops to zero, we can stop summing.
             */

            int integerPart = (int)Floor(x);
            // Take fractional part of exponent
            x = new Fix64(x.RawValue & 0x00000000FFFFFFFF);

            var result = Fix64.One;
            var term = Fix64.One;
            int i = 1;
            while (term.RawValue != 0)
            {
                term = Fix64.FastMul(Fix64.FastMul(x, term), Fix64.Ln2) / (Fix64)i;
                result += term;
                i++;
            }

            result = Fix64.FromRaw(result.RawValue << integerPart);
            if (neg)
            {
                result = Fix64.One / result;
            }

            return result;
        }

        /// <summary>
        /// Returns the base-2 logarithm of a specified number.
        /// Provides at least 9 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static Fix64 Log2(Fix64 x)
        {
            if (x.RawValue <= 0)
            {
                throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");
            }

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (Fix64.FRACTIONAL_PLACES - 1);
            long y = 0;

            long rawX = x.RawValue;
            while (rawX < Fix64.ONE)
            {
                rawX <<= 1;
                y -= Fix64.ONE;
            }

            while (rawX >= (Fix64.ONE << 1))
            {
                rawX >>= 1;
                y += Fix64.ONE;
            }

            var z = new Fix64(rawX);

            for (int i = 0; i < Fix64.FRACTIONAL_PLACES; i++)
            {
                z = Fix64.FastMul(z, z);
                if (z.RawValue >= (Fix64.ONE << 1))
                {
                    z = new Fix64(z.RawValue >> 1);
                    y += b;
                }
                b >>= 1;
            }

            return new Fix64(y);
        }

        /// <summary>
        /// Returns the natural logarithm of a specified number.
        /// Provides at least 7 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static Fix64 Ln(Fix64 x)
        {
            return Fix64.FastMul(Log2(x), Fix64.Ln2);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// Provides about 5 digits of accuracy for the result.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// The base was zero, with a negative exponent
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The base was negative, with a non-zero exponent
        /// </exception>
        public static Fix64 Pow(Fix64 b, Fix64 exp)
        {
            if (b == Fix64.One)
            {
                return Fix64.One;
            }
            if (exp.RawValue == 0)
            {
                return Fix64.One;
            }
            if (b.RawValue == 0)
            {
                if (exp.RawValue < 0)
                {
                    throw new DivideByZeroException();
                }
                return Fix64.Zero;
            }

            Fix64 log2 = Log2(b);
            return Pow2(exp * log2);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was negative.
        /// </exception>
        public static Fix64 Sqrt(Fix64 x)
        {
            var xl = x.RawValue;
            if (xl < 0)
            {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (Fix64.NUM_BITS - 2);

            while (bit > num)
            {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i)
            {
                // First we get the top 48 bits of the answer.
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }
                    bit >>= 2;
                }

                if (i == 0)
                {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (Fix64.NUM_BITS / 2)) - 1)
                    {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (Fix64.NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (Fix64.NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= (Fix64.NUM_BITS / 2);
                        result <<= (Fix64.NUM_BITS / 2);
                    }

                    bit = 1UL << (Fix64.NUM_BITS / 2 - 2);
                }
            }
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result)
            {
                ++result;
            }
            return new Fix64((long)result);
        }

        /// <summary>
        /// Returns the Sine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static Fix64 Sin(Fix64 x)
        {
            var clampedL = ClampSinValue(x.RawValue, out var flipHorizontal, out var flipVertical);
            var clamped = new Fix64(clampedL);

            // Find the two closest values in the LUT and perform linear interpolation
            // This is what kills the performance of this function on x86 - x64 is fine though
            var rawIndex = Fix64.FastMul(clamped, Fix64.LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = Fix64.FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fix64(Fix64.SinLut[flipHorizontal ?
                Fix64.SinLut.Length - 1 - (int)roundedIndex :
                (int)roundedIndex]);
            var secondNearestValue = new Fix64(Fix64.SinLut[flipHorizontal ?
                Fix64.SinLut.Length - 1 - (int)roundedIndex - Sign(indexError) :
                (int)roundedIndex + Sign(indexError)]);

            var delta = Fix64.FastMul(indexError, FastAbs(Fix64.FastSub(nearestValue, secondNearestValue))).RawValue;
            var interpolatedValue = nearestValue.RawValue + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
            return new Fix64(finalValue);
        }

        /// <summary>
        /// Returns a rough approximation of the Sine of x.
        /// This is at least 3 times faster than Sin() on x86 and slightly faster than Math.Sin(),
        /// however its accuracy is limited to 4-5 decimals, for small enough values of x.
        /// </summary>
        public static Fix64 FastSin(Fix64 x)
        {
            var clampedL = ClampSinValue(x.RawValue, out bool flipHorizontal, out bool flipVertical);

            // Here we use the fact that the SinLut table has a number of entries
            // equal to (PI_OVER_2 >> 15) to use the angle to index directly into it
            var rawIndex = (uint)(clampedL >> 15);
            if (rawIndex >= Fix64.LUT_SIZE)
            {
                rawIndex = Fix64.LUT_SIZE - 1;
            }
            var nearestValue = Fix64.SinLut[flipHorizontal ?
                Fix64.SinLut.Length - 1 - (int)rawIndex :
                (int)rawIndex];
            return new Fix64(flipVertical ? -nearestValue : nearestValue);
        }


        static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
        {
            var largePI = 7244019458077122842;
            // Obtained from ((Fix64)1686629713.065252369824872831112M).m_rawValue
            // This is (2^29)*PI, where 29 is the largest N such that (2^N)*PI < MaxValue.
            // The idea is that this number contains way more precision than PI_TIMES_2,
            // and (((x % (2^29*PI)) % (2^28*PI)) % ... (2^1*PI) = x % (2 * PI)
            // In practice this gives us an error of about 1,25e-9 in the worst case scenario (Sin(MaxValue))
            // Whereas simply doing x % PI_TIMES_2 is the 2e-3 range.

            var clamped2Pi = angle;
            for (int i = 0; i < 29; ++i)
            {
                clamped2Pi %= (largePI >> i);
            }
            if (angle < 0)
            {
                clamped2Pi += Fix64.PI_TIMES_2;
            }

            // The LUT contains values for 0 - PiOver2; every other value must be obtained by
            // vertical or horizontal mirroring
            flipVertical = clamped2Pi >= Fix64.PI;
            // obtain (angle % PI) from (angle % 2PI) - much faster than doing another modulo
            var clampedPi = clamped2Pi;
            while (clampedPi >= Fix64.PI)
            {
                clampedPi -= Fix64.PI;
            }
            flipHorizontal = clampedPi >= Fix64.PI_OVER_2;
            // obtain (angle % PI_OVER_2) from (angle % PI) - much faster than doing another modulo
            var clampedPiOver2 = clampedPi;
            if (clampedPiOver2 >= Fix64.PI_OVER_2)
            {
                clampedPiOver2 -= Fix64.PI_OVER_2;
            }
            return clampedPiOver2;
        }

        /// <summary>
        /// Returns the cosine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static Fix64 Cos(Fix64 x)
        {
            var xl = x.RawValue;
            var rawAngle = xl + (xl > 0 ? -Fix64.PI - Fix64.PI_OVER_2 : Fix64.PI_OVER_2);
            return Sin(new Fix64(rawAngle));
        }

        /// <summary>
        /// Returns a rough approximation of the cosine of x.
        /// See FastSin for more details.
        /// </summary>
        public static Fix64 FastCos(Fix64 x)
        {
            var xl = x.RawValue;
            var rawAngle = xl + (xl > 0 ? -Fix64.PI - Fix64.PI_OVER_2 : Fix64.PI_OVER_2);
            return FastSin(new Fix64(rawAngle));
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        /// <remarks>
        /// This function is not well-tested. It may be wildly inaccurate.
        /// </remarks>
        public static Fix64 Tan(Fix64 x)
        {
            var clampedPi = x.RawValue % Fix64.PI;
            var flip = false;
            if (clampedPi < 0)
            {
                clampedPi = -clampedPi;
                flip = true;
            }
            if (clampedPi > Fix64.PI_OVER_2)
            {
                flip = !flip;
                clampedPi = Fix64.PI_OVER_2 - (clampedPi - Fix64.PI_OVER_2);
            }

            var clamped = new Fix64(clampedPi);

            // Find the two closest values in the LUT and perform linear interpolation
            var rawIndex = Fix64.FastMul(clamped, Fix64.LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = Fix64.FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fix64(Fix64.TanLut[(int)roundedIndex]);
            var secondNearestValue = new Fix64(Fix64.TanLut[(int)roundedIndex + Sign(indexError)]);

            var delta = Fix64.FastMul(indexError, FastAbs(Fix64.FastSub(nearestValue, secondNearestValue))).RawValue;
            var interpolatedValue = nearestValue.RawValue + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            return new Fix64(finalValue);
        }

        /// <summary>
        /// Returns the arccos of of the specified number, calculated using Atan and Sqrt
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fix64 Acos(Fix64 x)
        {
            if (x < -Fix64.One || x > Fix64.One)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (x.RawValue == 0) return Fix64.PiOver2;

            var result = Atan(Sqrt(Fix64.One - x * x) / x);
            return x.RawValue < 0 ? result + Fix64.Pi : result;
        }

        /// <summary>
        /// Returns the arctan of of the specified number, calculated using Euler series
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fix64 Atan(Fix64 z)
        {
            if (z.RawValue == 0) return Fix64.Zero;

            // Force positive values for argument
            // Atan(-z) = -Atan(z).
            var neg = z.RawValue < 0;
            if (neg)
            {
                z = -z;
            }

            Fix64 result;
            var two = (Fix64)2;
            var three = (Fix64)3;

            bool invert = z > Fix64.One;
            if (invert) z = Fix64.One / z;

            result = Fix64.One;
            var term = Fix64.One;

            var zSq = z * z;
            var zSq2 = zSq * two;
            var zSqPlusOne = zSq + Fix64.One;
            var zSq12 = zSqPlusOne * two;
            var dividend = zSq2;
            var divisor = zSqPlusOne * three;

            for (var i = 2; i < 30; ++i)
            {
                term *= dividend / divisor;
                result += term;

                dividend += zSq2;
                divisor += zSq12;

                if (term.RawValue == 0) break;
            }

            result = result * z / zSqPlusOne;

            if (invert)
            {
                result = Fix64.PiOver2 - result;
            }

            if (neg)
            {
                result = -result;
            }
            return result;
        }

        public static Fix64 Atan2(Fix64 y, Fix64 x)
        {
            var yl = y.RawValue;
            var xl = x.RawValue;
            if (xl == 0)
            {
                if (yl > 0)
                {
                    return Fix64.PiOver2;
                }
                if (yl == 0)
                {
                    return Fix64.Zero;
                }
                return -Fix64.PiOver2;
            }
            Fix64 atan;
            var z = y / x;

            // Deal with overflow
            if (Fix64.One + (Fix64)0.28M * z * z == Fix64.MaxValue)
            {
                return y < Fix64.Zero ? -Fix64.PiOver2 : Fix64.PiOver2;
            }

            if (Abs(z) < Fix64.One)
            {
                atan = z / (Fix64.One + (Fix64)0.28M * z * z);
                if (xl < 0)
                {
                    if (yl < 0)
                    {
                        return atan - Fix64.Pi;
                    }
                    return atan + Fix64.Pi;
                }
            }
            else
            {
                atan = Fix64.PiOver2 - z / (z * z + (Fix64)0.28M);
                if (yl < 0)
                {
                    return atan - Fix64.Pi;
                }
            }
            return atan;
        }
    }
}

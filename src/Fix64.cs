using System;
using System.Runtime.CompilerServices;

namespace FixedMath
{
    /// <summary>
    /// Represents a Q31.32 fixed-point number.
    /// </summary>
    public partial struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
    {
        // Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
        public static readonly decimal Precision = (decimal)(new Fix64(1L));//0.00000000023283064365386962890625m;
        public static readonly Fix64 MaxValue = new Fix64(MAX_VALUE);
        public static readonly Fix64 MinValue = new Fix64(MIN_VALUE);
        public static readonly Fix64 One = new Fix64(ONE);
        public static readonly Fix64 Zero = new Fix64();
        /// <summary>
        /// The value of Pi
        /// </summary>
        public static readonly Fix64 Pi = new Fix64(PI);
        public static readonly Fix64 PiOver2 = new Fix64(PI_OVER_2);
        public static readonly Fix64 PiTimes2 = new Fix64(PI_TIMES_2);
        public static readonly Fix64 PiInv = (Fix64)0.3183098861837906715377675267M;
        public static readonly Fix64 PiOver2Inv = (Fix64)0.6366197723675813430755350535M;
        internal static readonly Fix64 Log2Max = new Fix64(LOG2MAX);
        internal static readonly Fix64 Log2Min = new Fix64(LOG2MIN);
        internal static readonly Fix64 Ln2 = new Fix64(LN2);

        internal static readonly Fix64 LutInterval = (Fix64)(LUT_SIZE - 1) / PiOver2;
        const long MAX_VALUE = long.MaxValue;
        internal const long MIN_VALUE = long.MinValue;
        internal const int NUM_BITS = 64;
        internal const int FRACTIONAL_PLACES = 32;
        internal const long ONE = 1L << FRACTIONAL_PLACES;
        internal const long PI_TIMES_2 = 0x6487ED511;
        internal const long PI = 0x3243F6A88;
        internal const long PI_OVER_2 = 0x1921FB544;
        const long LN2 = 0xB17217F7;
        const long LOG2MAX = 0x1F00000000;
        const long LOG2MIN = -0x2000000000;
        internal const int LUT_SIZE = (int)(PI_OVER_2 >> 15);


        /// <summary>
        /// The underlying integer representation
        /// </summary>
        public long RawValue { get; }

        /// <summary>
        /// This is the constructor from raw value; it can only be used interally.
        /// </summary>
        /// <param name="rawValue"></param>
        internal Fix64(long rawValue)
        {
            RawValue = rawValue;
        }

        public Fix64(int value)
        {
            RawValue = value * ONE;
        }

        /// <summary>
        /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator +(Fix64 x, Fix64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
            {
                sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
            }
            return new Fix64(sum);
        }

        /// <summary>
        /// Adds x and y witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static Fix64 FastAdd(Fix64 x, Fix64 y)
        {
            return new Fix64(x.RawValue + y.RawValue);
        }

        /// <summary>
        /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator -(Fix64 x, Fix64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
            {
                diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
            }
            return new Fix64(diff);
        }

        /// <summary>
        /// Subtracts y from x witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static Fix64 FastSub(Fix64 x, Fix64 y)
        {
            return new Fix64(x.RawValue - y.RawValue);
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
            return sum;
        }

        public static Fix64 operator *(Fix64 x, Fix64 y)
        {

            var xl = x.RawValue;
            var yl = y.RawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual)
            {
                if (sum < 0 || (overflow && xl > 0))
                {
                    return MaxValue;
                }
            }
            else
            {
                if (sum > 0)
                {
                    return MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> FRACTIONAL_PLACES;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
            {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (!opSignsEqual)
            {
                long posOp, negOp;
                if (xl > yl)
                {
                    posOp = xl;
                    negOp = yl;
                }
                else
                {
                    posOp = yl;
                    negOp = xl;
                }
                if (sum > negOp && negOp < -ONE && posOp > ONE)
                {
                    return MinValue;
                }
            }

            return new Fix64(sum);
        }

        /// <summary>
        /// Performs multiplication without checking for overflow.
        /// Useful for performance-critical code where the values are guaranteed not to cause overflow
        /// </summary>
        public static Fix64 FastMul(Fix64 x, Fix64 y)
        {

            var xl = x.RawValue;
            var yl = y.RawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            return new Fix64(sum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CountLeadingZeroes(ulong x)
        {
            int result = 0;
            while ((x & 0xF000000000000000) == 0) { result += 4; x <<= 4; }
            while ((x & 0x8000000000000000) == 0) { result += 1; x <<= 1; }
            return result;
        }

        public static Fix64 operator /(Fix64 x, Fix64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;

            if (yl == 0)
            {
                throw new DivideByZeroException();
            }

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = NUM_BITS / 2 + 1;


            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos)
                {
                    shift = bitPos;
                }
                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // Detect overflow
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                {
                    return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (long)(quotient >> 1);
            if (((xl ^ yl) & MIN_VALUE) != 0)
            {
                result = -result;
            }

            return new Fix64(result);
        }

        public static Fix64 operator %(Fix64 x, Fix64 y)
        {
            return new Fix64(
                x.RawValue == MIN_VALUE & y.RawValue == -1 ?
                0 :
                x.RawValue % y.RawValue);
        }

        /// <summary>
        /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
        /// Use the operator (%) for a more reliable but slower modulo.
        /// </summary>
        public static Fix64 FastMod(Fix64 x, Fix64 y)
        {
            return new Fix64(x.RawValue % y.RawValue);
        }

        public static Fix64 operator -(Fix64 x)
        {
            return x.RawValue == MIN_VALUE ? MaxValue : new Fix64(-x.RawValue);
        }

        public static bool operator ==(Fix64 x, Fix64 y)
        {
            return x.RawValue == y.RawValue;
        }

        public static bool operator !=(Fix64 x, Fix64 y)
        {
            return x.RawValue != y.RawValue;
        }

        public static bool operator >(Fix64 x, Fix64 y)
        {
            return x.RawValue > y.RawValue;
        }

        public static bool operator <(Fix64 x, Fix64 y)
        {
            return x.RawValue < y.RawValue;
        }

        public static bool operator >=(Fix64 x, Fix64 y)
        {
            return x.RawValue >= y.RawValue;
        }

        public static bool operator <=(Fix64 x, Fix64 y)
        {
            return x.RawValue <= y.RawValue;
        }



        public static explicit operator Fix64(long value)
        {
            return new Fix64(value * ONE);
        }
        public static explicit operator long(Fix64 value)
        {
            return value.RawValue >> FRACTIONAL_PLACES;
        }
        public static explicit operator Fix64(float value)
        {
            return new Fix64((long)(value * ONE));
        }
        public static explicit operator float(Fix64 value)
        {
            return (float)value.RawValue / ONE;
        }
        public static explicit operator Fix64(double value)
        {
            return new Fix64((long)(value * ONE));
        }
        public static explicit operator double(Fix64 value)
        {
            return (double)value.RawValue / ONE;
        }
        public static explicit operator Fix64(decimal value)
        {
            return new Fix64((long)(value * ONE));
        }
        public static explicit operator decimal(Fix64 value)
        {
            return (decimal)value.RawValue / ONE;
        }

        public override bool Equals(object obj)
        {
            return obj is Fix64 && ((Fix64)obj).RawValue == RawValue;
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public bool Equals(Fix64 other)
        {
            return RawValue == other.RawValue;
        }

        public int CompareTo(Fix64 other)
        {
            return RawValue.CompareTo(other.RawValue);
        }

        public override string ToString()
        {
            // Up to 10 decimal places
            return ((decimal)this).ToString("0.##########");
        }

        public static Fix64 FromRaw(long rawValue)
        {
            return new Fix64(rawValue);
        }
    }
}

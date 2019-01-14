// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MathF = FixedMath.MathFix;
using Matrix4x4 = FixedMath.Numerics.Fix64Matrix4x4;
using Quaternion = FixedMath.Numerics.Fix64Quaternion;
using Single = FixedMath.Fix64;
using Vector3 = FixedMath.Numerics.Fix64Vector3;

namespace FixedMath.Numerics
{
    /// <summary>
    /// A structure encapsulating three single precision floating point values and provides hardware accelerated methods.
    /// </summary>
    public partial struct Fix64Vector3 : IEquatable<Vector3>, IFormattable
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the vector (0,0,0).
        /// </summary>
        public static Vector3 Zero
        {
            [Intrinsic]
            get
            {
                return new Vector3();
            }
        }
        /// <summary>
        /// Returns the vector (1,1,1).
        /// </summary>
        public static Vector3 One
        {
            [Intrinsic]
            get
            {
                return new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
        /// <summary>
        /// Returns the vector (1,0,0).
        /// </summary>
        public static Vector3 UnitX { get { return new Vector3(1.0f, 0.0f, 0.0f); } }
        /// <summary>
        /// Returns the vector (0,1,0).
        /// </summary>
        public static Vector3 UnitY { get { return new Vector3(0.0f, 1.0f, 0.0f); } }
        /// <summary>
        /// Returns the vector (0,0,1).
        /// </summary>
        public static Vector3 UnitZ { get { return new Vector3(0.0f, 0.0f, 1.0f); } }
        #endregion Public Static Properties

        #region Public Instance Methods

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = this.X.GetHashCode();
            hash = HashHelpers.Combine(hash, this.Y.GetHashCode());
            hash = HashHelpers.Combine(hash, this.Z.GetHashCode());
            return hash;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this Vector3 instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this Vector3; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;
            return Equals((Vector3)obj);
        }

        /// <summary>
        /// Returns a String representing this Vector3 instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a String representing this Vector3 instance, using the specified format to format individual elements.
        /// </summary>
        /// <param name="format">The format of individual elements.</param>
        /// <returns>The string representation.</returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a String representing this Vector3 instance, using the specified format to format individual elements 
        /// and the given IFormatProvider.
        /// </summary>
        /// <param name="format">The format of individual elements.</param>
        /// <param name="formatProvider">The format provider to use when formatting elements.</param>
        /// <returns>The string representation.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            sb.Append('<');
            sb.Append(((IFormattable)this.X).ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(((IFormattable)this.Y).ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(((IFormattable)this.Z).ToString(format, formatProvider));
            sb.Append('>');
            return sb.ToString();
        }

        /// <summary>
        /// Returns the length of the vector.
        /// </summary>
        /// <returns>The vector's length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Single Length()
        {
            if (Vector.IsHardwareAccelerated)
            {
                Single ls = Vector3.Dot(this, this);
                return MathF.Sqrt(ls);
            }
            else
            {
                Single ls = X * X + Y * Y + Z * Z;
                return MathF.Sqrt(ls);
            }
        }

        /// <summary>
        /// Returns the length of the vector squared. This operation is cheaper than Length().
        /// </summary>
        /// <returns>The vector's length squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Single LengthSquared()
        {
            if (Vector.IsHardwareAccelerated)
            {
                return Vector3.Dot(this, this);
            }
            else
            {
                return X * X + Y * Y + Z * Z;
            }
        }
        #endregion Public Instance Methods

        #region Public Static Methods
        /// <summary>
        /// Returns the Euclidean distance between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single Distance(Vector3 value1, Vector3 value2)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 difference = value1 - value2;
                Single ls = Vector3.Dot(difference, difference);
                return MathF.Sqrt(ls);
            }
            else
            {
                Single dx = value1.X - value2.X;
                Single dy = value1.Y - value2.Y;
                Single dz = value1.Z - value2.Z;

                Single ls = dx * dx + dy * dy + dz * dz;

                return MathF.Sqrt(ls);
            }
        }

        /// <summary>
        /// Returns the Euclidean distance squared between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single DistanceSquared(Vector3 value1, Vector3 value2)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 difference = value1 - value2;
                return Vector3.Dot(difference, difference);
            }
            else
            {
                Single dx = value1.X - value2.X;
                Single dy = value1.Y - value2.Y;
                Single dz = value1.Z - value2.Z;

                return dx * dx + dy * dy + dz * dz;
            }
        }

        /// <summary>
        /// Returns a vector with the same direction as the given vector, but with a length of 1.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Single length = value.Length();
                return value / length;
            }
            else
            {
                Single ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                Single invLength = Single.One / MathF.Sqrt(ls);
                return new Vector3(value.X * invLength, value.Y * invLength, value.Z * invLength);
            }
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal of the surface being reflected off.</param>
        /// <returns>The reflected vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Single dot = Vector3.Dot(vector, normal);
                Vector3 temp = normal * dot * 2f;
                return vector - temp;
            }
            else
            {
                Single dot = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
                Single tempX = normal.X * dot * 2f;
                Single tempY = normal.Y * dot * 2f;
                Single tempZ = normal.Z * dot * 2f;
                return new Vector3(vector.X - tempX, vector.Y - tempY, vector.Z - tempZ);
            }
        }

        /// <summary>
        /// Restricts a vector between a min and max value.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The restricted vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            // This compare order is very important!!!
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.
            Single x = value1.X;
            x = (min.X > x) ? min.X : x;  // max(x, minx)
            x = (max.X < x) ? max.X : x;  // min(x, maxx)

            Single y = value1.Y;
            y = (min.Y > y) ? min.Y : y;  // max(y, miny)
            y = (max.Y < y) ? max.Y : y;  // min(y, maxy)

            Single z = value1.Z;
            z = (min.Z > z) ? min.Z : z;  // max(z, minz)
            z = (max.Z < z) ? max.Z : z;  // min(z, maxz)

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Linearly interpolates between two vectors based on the given weighting.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
        /// <returns>The interpolated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 value1, Vector3 value2, Single amount)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 firstInfluence = value1 * (1f - amount);
                Vector3 secondInfluence = value2 * amount;
                return firstInfluence + secondInfluence;
            }
            else
            {
                return new Vector3(
                    value1.X + (value2.X - value1.X) * amount,
                    value1.Y + (value2.Y - value1.Y) * amount,
                    value1.Z + (value2.Z - value1.Z) * amount);
            }
        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 position, Matrix4x4 matrix)
        {
            return new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
        }

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        /// <param name="normal">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix)
        {
            return new Vector3(
                normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
                normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
                normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
        }

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        /// <param name="value">The source vector to be rotated.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 value, Quaternion rotation)
        {
            Single x2 = rotation.X + rotation.X;
            Single y2 = rotation.Y + rotation.Y;
            Single z2 = rotation.Z + rotation.Z;

            Single wx2 = rotation.W * x2;
            Single wy2 = rotation.W * y2;
            Single wz2 = rotation.W * z2;
            Single xx2 = rotation.X * x2;
            Single xy2 = rotation.X * y2;
            Single xz2 = rotation.X * z2;
            Single yy2 = rotation.Y * y2;
            Single yz2 = rotation.Y * z2;
            Single zz2 = rotation.Z * z2;

            return new Vector3(
                value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
                value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
                value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2));
        }
        #endregion Public Static Methods

        #region Public operator methods

        // All these methods should be inlined as they are implemented
        // over JIT intrinsics

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(Vector3 left, Vector3 right)
        {
            return left + right;
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Subtract(Vector3 left, Vector3 right)
        {
            return left - right;
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 left, Vector3 right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Vector3 left, Single right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(Single left, Vector3 right)
        {
            return left * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 left, Vector3 right)
        {
            return left / right;
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="divisor">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 left, Single divisor)
        {
            return left / divisor;
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Negate(Vector3 value)
        {
            return -value;
        }
        #endregion Public operator methods
    }
}
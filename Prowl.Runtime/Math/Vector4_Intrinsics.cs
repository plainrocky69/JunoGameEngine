// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Prowl.Runtime
{
    // This file contains the definitions for all of the JIT intrinsic methods and properties that are recognized by the current x64 JIT compiler.
    // The implementation defined here is used in any circumstance where the JIT fails to recognize these members as intrinsic.
    // The JIT recognizes these methods and properties by name and signature: if either is changed, the JIT will no longer recognize the member.
    // Some methods declared here are not strictly intrinsic, but delegate to an intrinsic method. For example, only one overload of CopyTo() 

    public partial struct Vector4
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public Double x;
        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public Double y;
        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public Double z;
        /// <summary>
        /// The W component of the vector.
        /// </summary>
        public Double w;

        #region Constructors

        /// <summary>
        /// Constructs a vector whose elements are all the single specified value.
        /// </summary>
        /// <param name="value">The element to fill the vector with.</param>
        public Vector4(Double value)
            : this(value, value, value, value)
        {
        }
        /// <summary>
        /// Constructs a vector with the given individual elements.
        /// </summary>
        /// <param name="w">W component.</param>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector4(Double x, Double y, Double z, Double w)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Constructs a Vector4 from the given Vector2 and a Z and W component.
        /// </summary>
        /// <param name="value">The vector to use as the X and Y components.</param>
        /// <param name="z">The Z component.</param>
        /// <param name="w">The W component.</param>
        public Vector4(Vector2 value, Double z, Double w)
        {
            x = value.x;
            y = value.y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Constructs a Vector4 from the given Vector3 and a W component.
        /// </summary>
        /// <param name="value">The vector to use as the X, Y, and Z components.</param>
        /// <param name="w">The W component.</param>
        public Vector4(Vector3 value, Double w)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            this.w = w;
        }
        #endregion Constructors

        #region Public Instance Methods
        /// <summary>
        /// Copies the contents of the vector into the given array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Double[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the contents of the vector into the given array, starting from index.
        /// </summary>
        /// <exception cref="ArgumentNullException">If array is null.</exception>
        /// <exception cref="RankException">If array is multidimensional.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If index is greater than end of the array or index is less than zero.</exception>
        /// <exception cref="ArgumentException">If number of elements in source vector is greater than those available in destination array.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Double[] array, int index)
        {
            if (array == null)
            {
                // Match the JIT's exception type here. For perf, a NullReference is thrown instead of an ArgumentNull.
                throw new NullReferenceException();
            }
            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if ((array.Length - index) < 4)
            {
                throw new ArgumentException();
            }
            array[index] = x;
            array[index + 1] = y;
            array[index + 2] = z;
            array[index + 3] = w;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Vector4 is equal to this Vector4 instance.
        /// </summary>
        /// <param name="other">The Vector4 to compare this instance to.</param>
        /// <returns>True if the other Vector4 is equal to this instance; False otherwise.</returns>
        public bool Equals(Vector4 other)
        {
            return this.x == other.x
                && this.y == other.y
                && this.z == other.z
                && this.w == other.w;
        }
        #endregion Public Instance Methods

        #region Public Static Methods
        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector4 vector1, Vector4 vector2)
        {
            return vector1.x * vector2.x +
                   vector1.y * vector2.y +
                   vector1.z * vector2.z +
                   vector1.w * vector2.w;
        }

        /// <summary>
        /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The minimized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                (value1.x < value2.x) ? value1.x : value2.x,
                (value1.y < value2.y) ? value1.y : value2.y,
                (value1.z < value2.z) ? value1.z : value2.z,
                (value1.w < value2.w) ? value1.w : value2.w);
        }

        /// <summary>
        /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <returns>The maximized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
                (value1.x > value2.x) ? value1.x : value2.x,
                (value1.y > value2.y) ? value1.y : value2.y,
                (value1.z > value2.z) ? value1.z : value2.z,
                (value1.w > value2.w) ? value1.w : value2.w);
        }

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The absolute value vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Abs(Vector4 value)
        {
            return new Vector4(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z), Math.Abs(value.w));
        }

        /// <summary>
        /// Returns a vector whose elements are the square root of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The square root vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SquareRoot(Vector4 value)
        {
            return new Vector4((Double)Math.Sqrt(value.x), (Double)Math.Sqrt(value.y), (Double)Math.Sqrt(value.z), (Double)Math.Sqrt(value.w));
        }
        #endregion Public Static Methods

        #region Public static operators
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            return new Vector4(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            return new Vector4(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            return new Vector4(left.x * right.x, left.y * right.y, left.z * right.z, left.w * right.w);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Double right)
        {
            return left * new Vector4(right);
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Double left, Vector4 right)
        {
            return new Vector4(left) * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 left, Vector4 right)
        {
            return new Vector4(left.x / right.x, left.y / right.y, left.z / right.z, left.w / right.w);
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="value2">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(Vector4 value1, double value2)
        {
            double invDiv = 1.0 / value2;

            return new Vector4(
                value1.x * invDiv,
                value1.y * invDiv,
                value1.z * invDiv,
                value1.w * invDiv);
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(Vector4 value)
        {
            return Zero - value;
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are equal; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are not equal.
        /// </summary>
        /// <param name="left">The first vector to compare.</param>
        /// <param name="right">The second vector to compare.</param>
        /// <returns>True if the vectors are not equal; False if they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator System.Numerics.Vector4(Vector4 value)
        {
            return new System.Numerics.Vector4((float)value.x, (float)value.y, (float)value.z, (float)value.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector4(System.Numerics.Vector4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }
        #endregion Public static operators
    }
}
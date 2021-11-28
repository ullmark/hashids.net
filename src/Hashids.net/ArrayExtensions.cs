using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HashidsNet
{
    internal static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] array, int index)
        {
            return SubArray(array, index, array.Length - index);
        }

        public static T[] SubArray<T>(this T[] array, int index, int length)
        {
            if (index == 0 && length == array.Length) return array;
            if (length == 0) return Array.Empty<T>();

            var subarray = new T[length];
            Array.Copy(array, index, subarray, 0, length);
            return subarray;
        }

        public static T[] Append<T>(this T[] array, T[] appendArray, int index, int length)
        {
            if (length == 0) return array;

            int newLength = array.Length + length - index;
            if (newLength == 0) return Array.Empty<T>();

            var newArray = new T[newLength];
            Array.Copy(array, 0, newArray, 0, array.Length);
            Array.Copy(appendArray, index, newArray, array.Length, length - index);
            return newArray;
        }

        public static T[] CopyPooled<T>(this T[] array)
        {
            return SubArrayPooled(array, 0, array.Length);
        }

        public static T[] SubArrayPooled<T>(this T[] array, int index, int length)
        {
            var subarray = ArrayPool<T>.Shared.Rent(length);
            Array.Copy(array, index, subarray, 0, length);
            return subarray;
        }

        public static void ReturnToPool<T>(this T[] array)
        {
            if (array == null)
                return;

            ArrayPool<T>.Shared.Return(array);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <remarks>This method exists because <see cref="ReadOnlySpan{T}"/> does not implement <see cref="IEnumerable{T}"/> and using <c>.AsEnumerable()</c> will cause boxing.</remarks>
        public static bool Any<T>(this ReadOnlySpan<T> span, Func<T,bool> predicate)
        {
            for(int i = 0; i < span.Length; i++)
            {
                if(predicate(span[i])) return true;
            }

            return false;
        }
#endif
    }
}
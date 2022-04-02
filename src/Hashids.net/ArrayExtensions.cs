using System;

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
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace HashidsNet
{
#if !NETCOREAPP3_1_OR_GREATER
    internal readonly struct ReadOnlySpan<T> : IReadOnlyList<T>
    {
        public static implicit operator ReadOnlySpan<T>(T[] array)
        {
            return new ReadOnlySpan<T>(array);
        }

        public static implicit operator ReadOnlySpan<T>(ArraySegment<T> segment)
        {
            return new ReadOnlySpan<T>(segment.Array, startIndex: segment.Offset, count: segment.Count);
        }

        public static implicit operator ArraySegment<T>(ReadOnlySpan<T> self)
        {
            return self.AsArraySegment();
        }

        private readonly T[] array;
        private readonly int startIndex;
        private readonly int count;

        public ReadOnlySpan(T[] array)
            : this(array, startIndex: 0, count: array?.Length ?? 0)
        {
        }

        public ReadOnlySpan(T[] array, int startIndex, int count)
        {
            this.array      = array ?? Array.Empty<T>();
            this.startIndex = startIndex;
            this.count      = count;

            if (this.array.Length == 0)
            {
                if (startIndex is not (0 or -1)       ) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(startIndex), actualValue: startIndex, message: "Value must be zero or -1 for empty arrays.");
                if (count             != 0            ) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(count)     , actualValue: count     , message: "Value must be zero for empty arrays.");
            }
            else
            {
                if (count              < 0            ) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(count)     , actualValue: count     , message: "Value must be non-negative.");
                if (startIndex         < 0            ) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(startIndex), actualValue: startIndex, message: "Value must be non-negative.");

                if (startIndex         >= array.Length) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(startIndex), actualValue: startIndex, message: "Value exceeds the length of the provided array.");
                if (startIndex + count >  array.Length) ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(count)     , actualValue: count     , message: "Value (plus " + nameof(startIndex) + ") exceeds the length of the provided array.");
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(paramName: nameof(index), actualValue: index, message: "Value must be between 0 and " + nameof(this.Length) + " (exclusive).");
                    return default;
                }
                else
                {
                    int sourceIndex = this.startIndex + index;
                    return this.array[sourceIndex];
                }
            }
        }

        public int Length => this.count;
        public int Count  => this.count; // Specifying both Length and Count for source-level compatibility with both IReadOnlyList<T>.Count and Array<T>.Length (and of course, ReadOnlySpan<T>.Length)

        private int EndIndex => ( this.startIndex + this.count ) - 1;

        public int IndexOf(T value)
        {
            int sourceIndex = Array.IndexOf(this.array, value, startIndex: this.startIndex, count: this.count);
            if (sourceIndex < 0) return -1;
            return this.startIndex + sourceIndex;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.count == 0)
            {
                return ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator();
            }

            return this.AsEnumerable().GetEnumerator();
        }

        private IEnumerable<T> AsEnumerable()
        {
            int endIdx = this.EndIndex;
            for (int i = this.startIndex; i <= endIdx; i++)
            {
                yield return this.array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public ArraySegment<T> AsArraySegment() => new (this.array, offset: this.startIndex, count: this.count);

        /// <remarks>This method exists because using Linq's extensions over <see cref="IEnumerable{T}"/> or <see cref="IReadOnlyList{T}"/> are a lot slower than doing it directly.</remarks>
        public bool Any(Func<T,bool> predicate)
        {
            int endIdx = this.EndIndex;
            for (int i = this.startIndex; i <= endIdx; i++)
            {
               if (predicate(this.array[i])) return true;
            }

            return false;
        }
    }
#endif
}
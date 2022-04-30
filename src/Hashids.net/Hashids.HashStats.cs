using System;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct HashStats
        {
            private HashStats(Hashids parent)
            {
                AlphabetLength = parent._alphabetProvider.Default.Length;
                PayloadHash = 0L;
                PayloadIndex = 0;
                PayloadLength = 0;
                IdleLength = 0;
                DataLength = parent._minHashLength;
                Lottery = 0;
                Count = 0;
                Valid = true;
            }

            public static HashStats Create(Hashids parent, ReadOnlySpan<int> numbers)
            {
                HashStats stats = new HashStats(parent);

                for (int i = 0; i < numbers.Length; i++)
                    stats.Register(numbers[i]);

                stats.Done();

                return stats;
            }

            public static HashStats Create(Hashids parent, ReadOnlySpan<long> numbers)
            {
                HashStats stats = new HashStats(parent);

                for (int i = 0; i < numbers.Length; i++)
                    stats.Register(numbers[i]);

                stats.Done();

                return stats;
            }

            public static HashStats Create(Hashids parent, long number)
            {
                HashStats stats = new HashStats(parent);

                stats.Register(number);
                stats.Done();

                return stats;
            }

            public readonly int GetPayloadLength(long number)
            {
                if (number < AlphabetLength)
                    return 1;

                return (int)Math.Log(number, AlphabetLength) + 1;
            }

            private void Register(long number)
            {
                PayloadHash += number % (Count + 100);
                PayloadLength += GetPayloadLength(number) + 1;
                Count++;
                Valid = Valid && number >= 0L;
            }

            private void Done()
            {
                DataLength = Math.Max(DataLength, PayloadLength);
                IdleLength = DataLength - PayloadLength;
                PayloadIndex = IdleLength - IdleLength / 2;
                Lottery = (int)(PayloadHash % AlphabetLength);
            }

            public int AlphabetLength { get; }
            public long PayloadHash { get; private set; }
            public int PayloadIndex { get; private set; }
            public int PayloadLength { get; private set; }
            public int IdleLength { get; private set; }
            public int DataLength { get; private set; }
            public int Lottery { get; private set; }
            public int Count { get; private set; }
            public bool Valid { get; private set; }
        }
    }
}

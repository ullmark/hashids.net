using System;

namespace HashidsNet
{
    public partial class Hashids
    {
        private static class HashEncoder
        {
            public static string Encode(Hashids parent, ReadOnlySpan<int> numbers)
            {
                if (numbers.Length == 0)
                    return string.Empty;

                if (numbers.Length == 1)
                    return Encode(parent, numbers[0]);

                HashStats stats = HashStats.Create(parent, numbers);

                if (!stats.Valid)
                    return string.Empty;

                Span<char> buffer = stats.DataLength <= 512 ? stackalloc char[stats.DataLength] : new char[stats.DataLength];
                EncodingContext context = new EncodingContext(parent, stats, buffer);

                PayloadWriter.Write(ref context, numbers);
                IdleWriter.Write(ref context);

                context.Dispose();

                return buffer.ToString();
            }

            public static string Encode(Hashids parent, ReadOnlySpan<long> numbers)
            {
                if (numbers.Length == 0)
                    return string.Empty;

                if (numbers.Length == 1)
                    return Encode(parent, numbers[0]);

                HashStats stats = HashStats.Create(parent, numbers);

                if (!stats.Valid)
                    return string.Empty;

                Span<char> buffer = stats.DataLength <= 512 ? stackalloc char[stats.DataLength] : new char[stats.DataLength];
                EncodingContext context = new EncodingContext(parent, stats, buffer);

                PayloadWriter.Write(ref context, numbers);
                IdleWriter.Write(ref context);

                context.Dispose();

                return buffer.ToString();
            }

            public static string Encode(Hashids parent, long number)
            {
                if (number < 0)
                    return string.Empty;

                HashStats stats = HashStats.Create(parent, number);
                Span<char> buffer = stackalloc char[stats.DataLength];
                EncodingContext context = new EncodingContext(parent, stats, buffer);

                PayloadWriter.Write(ref context, number);
                IdleWriter.Write(ref context);

                context.Dispose();

                return buffer.ToString();
            }
        }
    }
}

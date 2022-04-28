using HashidsNet.Alphabets;
using System;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct IdleWriter
        {
            private readonly Hashids _parent;
            private readonly HashStats _stats;
            private readonly Span<char> _buffer;

            private IAlphabet _alphabet;

            public IdleWriter(EncodingContext context)
            {
                _parent = context.Parent;
                _stats = context.Stats;
                _buffer = context.Buffer;
                _alphabet = context.Alphabet;
            }

            public static void Write(ref EncodingContext context)
            {
                if (context.Stats.DataLength == context.Stats.PayloadLength)
                    return;

                IdleWriter writer = new IdleWriter(context);

                writer.Write();
                writer.Update(ref context);
            }

            private void Write()
            {
                int length = _stats.DataLength - _stats.PayloadLength;
                int payloadIndex = length - length / 2;
                int leftIndex = payloadIndex - 1;
                int rightIndex = payloadIndex + _stats.PayloadLength;

                if (length == 0)
                    return;

                WriteGuard(leftIndex, _buffer[payloadIndex]);

                if (length <= 1)
                    return;

                WriteGuard(rightIndex, _buffer[payloadIndex + 1]);

                if (length <= 2)
                    return;

                rightIndex++;
                length -= 2;

                while (length > 0)
                {
                    int len = Math.Min(length, _alphabet.Length);
                    int leftLen = len - len / 2;
                    int rightLen = len - leftLen;

                    leftIndex -= leftLen;

                    _alphabet = _alphabet.NextShuffle();

                    Span<char> leftSpan = _buffer.Slice(leftIndex, leftLen);
                    Span<char> rightSpan = _buffer.Slice(rightIndex, rightLen);

                    _alphabet.CopyTo(leftSpan, _alphabet.Length - leftLen);
                    _alphabet.CopyTo(rightSpan, 0);

                    rightIndex += rightLen;

                    length -= len;
                }
            }

            private void WriteGuard(int position, char salt)
            {
                var guards = _parent._guards;
                long index = (_stats.PayloadHash + salt) % guards.Length;

                _buffer[position] = guards[index];
            }

            private void Update(ref EncodingContext context)
            {
                context.Alphabet = _alphabet;
                context.Index = _stats.DataLength;
            }
        }
    }
}

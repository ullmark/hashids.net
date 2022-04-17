﻿using System;

namespace HashidsNet.Alphabets.Salts
{
    public static class ShuffleHelper
    {
        public static void Shuffle(this ISalt salt, char[] chars)
        {
            Shuffle(salt, chars, chars.Length);
        }

        public static void Shuffle(this ISalt salt, char[] chars, int length)
        {
            if (salt.Length == 0)
                return;

            if (chars.Length <= 1)
                return;

            Span<int> saltValue = stackalloc int[length];

            CalculateSaltValue(salt, saltValue);

            Mods mods = FastMod.Create(length);

            for (int i = length - 1; i > 0; i--)
            {
                int value = saltValue[length - i - 1];
                int j = mods.Mod(value, i);

                char temp = chars[j];
                chars[j] = chars[i];
                chars[i] = temp;
            }
        }

        private static void CalculateSaltValue(ISalt salt, Span<int> buffer)
        {
            int saltSum = 0;

            if (salt.Length > buffer.Length)
            {
                salt.Calculate(buffer, 0, ref saltSum);
                return;
            }

            int index = 0;

            do
            {
                int spanLength = Math.Min(salt.Length, buffer.Length - index);
                Span<int> span = buffer.Slice(index, spanLength);

                salt.Calculate(span, 0, ref saltSum);

                index += spanLength;
            } while (index < buffer.Length);
        }
    }
}

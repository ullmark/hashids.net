using System;

namespace HashidsNet.Alphabets.Salts
{
    public static class ShuffleHelper
    {
        [ThreadStatic]
        private static int[] _saltValueBuffer;

        public static void Shuffle(this ISalt salt, char[] chars)
        {
            if (salt.Length == 0)
                return;

            if (chars.Length <= 1)
                return;

            int[] saltValue = GetSaltValue(salt, chars.Length);

            int len = chars.Length;
            Mods mods = FastMod.Create(len);

            for (int i = len - 1; i > 0; i--)
            {
                int value = saltValue[len - i - 1];
                int j = mods.Mod(value, i);

                char temp = chars[j];
                chars[j] = chars[i];
                chars[i] = temp;
            }
        }

        private static int[] GetSaltValue(ISalt salt, int length)
        {
            int[] buffer = _saltValueBuffer;

            if (buffer == null || buffer.Length < length)
            {
                buffer = new int[length];
                _saltValueBuffer = buffer;
            }

            int index = 0;
            int sum = 0;

            while (index < length)
            {
                int saltLength = Math.Min(salt.Length, length - index);

                salt.Calculate(buffer, index, 0, saltLength, ref sum);

                index += saltLength;
            }

            return buffer;
        }
    }
}

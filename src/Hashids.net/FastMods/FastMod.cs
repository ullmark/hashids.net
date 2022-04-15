using System.Threading;

namespace HashidsNet
{
    public static class FastMod
    {
        private static Mod[] _mods;

        public static Mods Create(int maxDivisor)
        {
            Mod[] mods = _mods;

            if (mods == null || mods.Length < maxDivisor)
            {
                Mod[] newMods = Calc(maxDivisor);

                Interlocked.CompareExchange(ref _mods, newMods, mods);

                mods = newMods;
            }

            return new Mods(mods);
        }

        private static Mod[] Calc(int length)
        {
            Mod[] factors = new Mod[length];

            for (uint i = 1; i < length; i++)
                factors[i] = new Mod(0xFFFFu / i + 1u, i);

            return factors;
        }
    }
}

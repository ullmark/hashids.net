namespace HashidsNet
{
    public struct Mods
    {
        private Mod[] _mods;

        public Mods(Mod[] mods)
        {
            _mods = mods;
        }

        public Mod Get(int divisor)
        {
            return _mods[divisor];
        }

        public int Mod(int value, int divisor)
        {
            return value % _mods[divisor];
        }
    }
}
namespace HashidsNet
{
    public struct Mod
    {
        private uint _factor;
        private uint _divisor;

        public Mod(uint factor, uint divisor)
        {
            _factor = factor;
            _divisor = divisor;
        }

        public static int operator %(int value, Mod mod)
        {
            uint lowBits = ((uint)value * mod._factor) & 0xFFFFu;

            return (int)(lowBits * mod._divisor) >> 16;
        }
    }
}
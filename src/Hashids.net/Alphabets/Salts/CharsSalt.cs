namespace HashidsNet.Alphabets.Salts
{
    public sealed class CharsSalt : BaseCharsSalt
    {
        public CharsSalt(char[] chars)
        {
            Chars = chars;
        }

        public override int Length => Chars.Length;

        protected override char[] Chars { get; }
    }
}

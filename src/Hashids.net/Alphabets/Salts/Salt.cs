namespace HashidsNet.Alphabets.Salts
{
    public static class Salt
    {
        public static ISalt Create(char @char)
        {
            return new CharSalt(@char);
        }

        public static ISalt Create(char[] chars)
        {
            return new CharsSalt(chars);
        }

        public static ISalt Create(int[] value, int sum)
        {
            return new ValueSalt(value, sum);
        }
    }
}

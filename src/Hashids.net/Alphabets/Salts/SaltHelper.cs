namespace HashidsNet.Alphabets.Salts
{
    public static class SaltHelper
    {
        public static ISalt TakeSnapshot(this ISalt salt)
        {
            if (salt.Length == 0)
                return EmptySalt.Instance;

            if (salt is CharSalt)
                return salt;

            int[] buffer = new int[salt.Length];
            int saltSum = 0;

            salt.Calculate(buffer, 0, 0, buffer.Length, ref saltSum);

            return Salt.Create(buffer, saltSum);
        }

        public static ISalt Concat(this ISalt salt, ISalt other)
        {
            if (salt.Length == 0)
                return other;

            if (other.Length == 0)
                return salt;

            return new ConcatSalt(salt, other);
        }

        public static ISalt Concat(this ISalt salt, char[] other)
        {
            return Concat(salt, Salt.Create(other));
        }
    }
}

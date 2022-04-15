namespace HashidsNet.Alphabets
{
    public class CacheAlphabetProvider : AlphabetProvider
    {
        private readonly Item[] _items;

        public CacheAlphabetProvider(char[] chars, char[] salt)
            : base(chars, salt)
        {
            _items = new Item[chars.Length];

            for (int i = 0; i < _items.Length; i++)
                _items[i] = new Item();
        }

        public IAlphabet GetAlphabet(int index)
        {
            if (index < 0 || index >= _items.Length)
                return null;

            Item item = _items[index];
            IAlphabet alphabet = item.Alphabet;

            if (alphabet == null)
            {
                alphabet = base.GetAlphabet(index);

                item.Alphabet = alphabet;
            }

            return alphabet;
        }

        private class Item
        {
            public IAlphabet Alphabet;
        }
    }
}

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

        public override IAlphabet GetAlphabet(int index)
        {
            Item item = _items[index];
            IAlphabet alphabet = item.Alphabet;

            if (alphabet == null)
            {
                alphabet = new StepsAlphabet(Default, Default.GetChar(index), Salt, 2);

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

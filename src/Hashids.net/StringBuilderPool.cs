using System.Collections.Concurrent;
using System.Text;

namespace HashidsNet
{
    internal class StringBuilderPool
    {
        private readonly ConcurrentBag<StringBuilder> _builders = new();

        public StringBuilder Get() => _builders.TryTake(out StringBuilder sb) ? sb : new();

        public void Return(StringBuilder sb)
        {
            sb.Clear();
            _builders.Add(sb);
        }
    }
}
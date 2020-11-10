using System.Collections.Generic;
using System.Linq;

namespace Hashids.net
{
    internal static class CastExtensions
    {
        public static IEnumerable<long> UnboxedCast(this IEnumerable<byte> nums) => nums.Select(x => (long)x);

        public static IEnumerable<long> UnboxedCast(this IEnumerable<short> nums) => nums.Select(x => (long)x);

        public static IEnumerable<long> UnboxedCast(this IEnumerable<int> nums) => nums.Select(x => (long)x);
    }
}

using System;
using System.Collections.Generic;

namespace HashidsNet
{
    public partial class Hashids : IHashids
    {
        public object GetFormat(Type formatType) =>
            formatType == typeof(ICustomFormatter) ? this : null;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == "H" || format == "h")
            {
                if (arg is int intValue) return Encode(intValue);

                if (arg is int[] intArray) return Encode(intArray);

                if (arg is IEnumerable<int> intValues) return Encode(intValues);

                if (arg is long longValue) return EncodeLong(longValue);

                if (arg is long[] longArray) return EncodeLong(longArray);

                if (arg is IEnumerable<long> longValues) return EncodeLong(longValues);

                if (arg is string hexValue) return EncodeHex(hexValue);
            }

            if (arg is IFormattable formattableValue) return formattableValue.ToString(format, formatProvider);

            return arg?.ToString();
        }
    }
}

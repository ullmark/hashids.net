using System;
#if NETCOREAPP3_1_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace HashidsNet
{
    internal static class ThrowHelper
    {
#if NETCOREAPP3_1_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

#if NETCOREAPP3_1_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowArgumentOutOfRangeException(string paramName, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

#if NETCOREAPP3_1_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowArgumentException(string message, string paramName)
        {
            throw new ArgumentException(message, paramName);
        }

#if NETCOREAPP3_1_OR_GREATER
        [DoesNotReturn]
#endif
        public static void ThrowArgumentOutOfRangeException(string paramName, object actualValue, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }
    }
}
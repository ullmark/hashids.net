using System;
using System.ComponentModel;

namespace HashidsNet
{
    public partial class Hashids : IHashids
    {
        /// <summary>
        /// Encodes the provided numbers into a hash.
        /// </summary>
        [Obsolete("Use 'Encode' instead. The method was renamed to better explain what it actually does.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string Encrypt(params int[] numbers)
        {
            return Encode(numbers);
        }

        /// <summary>
        /// Encrypts the provided hex-string to a hash.
        /// </summary>
        [Obsolete("Use 'EncodeHex' instead. The method was renamed to better explain what it actually does.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string EncryptHex(string hex)
        {
            return EncodeHex(hex);
        }

        /// <summary>
        /// Decodes the provided hash into an array of numbers.
        /// </summary>
        [Obsolete("Use 'Decode' instead. Method was renamed to better explain what it actually does.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual int[] Decrypt(string hash)
        {
            return Decode(hash);
        }

        /// <summary>
        /// Decodes the provided hash to a hex-string.
        /// </summary>
        [Obsolete("Use 'DecodeHex' instead. The method was renamed to better explain what it actually does.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual string DecryptHex(string hash)
        {
            return DecodeHex(hash);
        }
    }
}
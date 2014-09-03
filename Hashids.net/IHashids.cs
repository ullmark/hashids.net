namespace HashidsNet
{
    /// <summary>
    ///     Generate YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids
    ///     to the user.
    /// </summary>
    public interface IHashids
    {
        /// <summary>
        ///     Encrypts the provided numbers into a hash.
        /// </summary>
        /// <param name="numbers">the numbers</param>
        /// <returns>the hash</returns>
        string Encrypt(params int[] numbers);

        /// <summary>
        ///     Encrypts the provided hex string to a hashids hash.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        string EncryptHex(string hex);

        /// <summary>
        ///     Decrypts the provided numbers into a array of numbers
        /// </summary>
        /// <param name="hash">hash</param>
        /// <returns>array of numbers.</returns>
        int[] Decrypt(string hash);

        /// <summary>
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        string DecryptHex(string hash);
    }
}
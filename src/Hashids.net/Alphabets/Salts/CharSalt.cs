using System;

namespace HashidsNet.Alphabets.Salts
{
    public class CharSalt : BaseCharSalt
    {
        public CharSalt(char @char)
        {
            Char = @char;
        }

        protected override char Char { get; }
    }
}
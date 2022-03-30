using System;

namespace HashidsNet
{
    public class MultipleResultsException : Exception
    {
        public MultipleResultsException() { }

        public MultipleResultsException(string message) : base(message) { }
    }
}

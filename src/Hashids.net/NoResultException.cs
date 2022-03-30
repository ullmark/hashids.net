using System;

namespace HashidsNet
{
    public class NoResultException : Exception
    {
        public NoResultException() { }

        public NoResultException(string message) : base(message) { }
    }
}

using System;

namespace AQOONHUB.Utilities
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
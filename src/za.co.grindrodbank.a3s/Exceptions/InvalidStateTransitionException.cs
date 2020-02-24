using System;
using System.Runtime.Serialization;

namespace za.co.grindrodbank.a3s.Exceptions
{
    [Serializable]
    public sealed class InvalidStateTransitionException : Exception
    {
        private const string defaultMessage = "Ivalid state transition for object.";

        public InvalidStateTransitionException() : base(defaultMessage)
        {
        }

        public InvalidStateTransitionException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public InvalidStateTransitionException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private InvalidStateTransitionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Runtime.Serialization;

namespace za.co.grindrodbank.a3sidentityserver.Exceptions
{
    [Serializable]
    public sealed class KeyMaterialException : Exception
    {
        private const string defaultMessage = "No key material certificate found.";

        public KeyMaterialException() : base(defaultMessage)
        {
        }

        public KeyMaterialException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public KeyMaterialException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private KeyMaterialException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

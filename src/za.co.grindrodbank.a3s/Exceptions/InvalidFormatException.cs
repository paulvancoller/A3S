/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Runtime.Serialization;

namespace za.co.grindrodbank.a3s.Exceptions
{
    [Serializable]
    public sealed class InvalidFormatException : Exception
    {
        private const string defaultMessage = "Item is not correctly formatted.";

        public InvalidFormatException() : base(defaultMessage)
        {
        }

        public InvalidFormatException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public InvalidFormatException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private InvalidFormatException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

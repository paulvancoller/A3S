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
    public sealed class OperationFailedException : Exception
    {
        private const string defaultMessage = "Operation failed.";

        public OperationFailedException() : base(defaultMessage)
        {
        }

        public OperationFailedException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public OperationFailedException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private OperationFailedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

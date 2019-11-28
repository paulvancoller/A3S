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
    public sealed class SecurityContractDryRunException : Exception
    {
        private const string defaultMessage = "Item not processable.";

        public SecurityContractDryRunException() : base(defaultMessage)
        {
        }

        public SecurityContractDryRunException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public SecurityContractDryRunException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private SecurityContractDryRunException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

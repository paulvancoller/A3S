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
    public sealed class ItemNotFoundException : Exception
    {
        private const string defaultMessage = "Item not found.";

        public ItemNotFoundException() : base(defaultMessage)
        {
        }

        public ItemNotFoundException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public ItemNotFoundException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private ItemNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

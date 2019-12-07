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
    public sealed class ArchiveException : Exception
    {
        private const string defaultMessage = "Archive processing error.";

        public ArchiveException() : base(defaultMessage)
        {
        }

        public ArchiveException(string message) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage)
        {
        }

        public ArchiveException(string message, Exception innerException) : base(!string.IsNullOrEmpty(message) ? message : defaultMessage, innerException)
        {
        }

        private ArchiveException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}

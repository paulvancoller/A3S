/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace za.co.grindrodbank.a3s.Exceptions
{
    [Serializable]
    public sealed class SecurityContractDryRunException : Exception
    {
        private const string defaultMessage = "Security Contract Dry Run Exceptions Detected.";
        public List<string> ValidationErrors { get; set; }
        public List<string> ValidationWarnings { get; set; }


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

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Models
{
    public class SecurityContractDryRunResult
    {
        public List<string> ValidationErrors { get; set; }
        public List<string> ValidationWarnings { get; set; }

        public SecurityContractDryRunResult()
        {
            ValidationErrors = new List<string>();
            ValidationWarnings = new List<string>();
        }
    }
}

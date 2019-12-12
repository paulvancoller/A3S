/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    /// <summary>
    /// A service for handling the idempotent application of the 'defaultConfigurations' section of an A3S consolidated security contract definition YAML.
    /// </summary>
    public interface ISecurityContractDefaultConfigurationService : ITransactableService
    {
        Task ApplyDefaultConfigurationDefinitionAsync(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult);
        Task<SecurityContractDefaultConfiguration> GetDefaultConfigurationDefinitionAsync();
    }
}

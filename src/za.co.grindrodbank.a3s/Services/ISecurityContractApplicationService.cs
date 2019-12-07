/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.A3SApiResources;
using System;

namespace za.co.grindrodbank.a3s.Services
{
    public interface ISecurityContractApplicationService : ITransactableService
    {
        Task<ApplicationModel> ApplyResourceServerDefinitionAsync(SecurityContractApplication applicationSecurityContractDefinition, Guid updatedById, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult);
        Task<List<SecurityContractApplication>> GetResourceServerDefinitionsAsync();
    }
}

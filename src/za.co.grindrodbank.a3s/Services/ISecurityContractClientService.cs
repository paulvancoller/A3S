/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    public interface ISecurityContractClientService : ITransactableService
    {
        Task<Oauth2Client> ApplyClientDefinitionAsync(Oauth2ClientSubmit oauth2ClientSubmit, bool dryRun, SecurityContractDryRunResult securityContractDryRunResult);
        Task<List<Oauth2ClientSubmit>> GetClientDefinitionsAsync();
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class ConsentOfServiceRepository : IConsentOfServiceRepository
    {
        private readonly A3SContext a3SContext;

        public ConsentOfServiceRepository(A3SContext a3SContext)
        {
            this.a3SContext = a3SContext;
        }

        public async Task<ConsentOfServiceModel> GetCurrentConsentAsync()
        {
            return await a3SContext.ConsentOfService.FirstOrDefaultAsync();
        }

        public async Task<ConsentOfServiceModel> UpdateCurrentConsentAsync(ConsentOfServiceModel consentOfService)
        {
            var currentConsent = await a3SContext.ConsentOfService.FirstOrDefaultAsync();
            if (currentConsent == null)
            {
                var addResult = await a3SContext.ConsentOfService.AddAsync(consentOfService);
                currentConsent = addResult.Entity;
            }
            else
            {
                currentConsent.ConsentFile = consentOfService.ConsentFile;
                currentConsent.ChangedBy = consentOfService.ChangedBy;
                a3SContext.Entry(currentConsent).State = EntityState.Modified;
            }

            var affected = await a3SContext.SaveChangesAsync();
            return affected != 1 ? null : currentConsent;
        }
    }
}
/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Extensions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class ConsentOfServiceRepository : IConsentOfServiceRepository
    {
        private readonly A3SContext a3SContext;
        private readonly IArchiveHelper archiveHelper;

        public ConsentOfServiceRepository(A3SContext a3SContext, IArchiveHelper archiveHelper)
        {
            this.a3SContext = a3SContext;
            this.archiveHelper = archiveHelper;
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
                var count = await a3SContext.ConsentOfService.CountAsync();
                count++;
                var addResult = await a3SContext.ConsentOfService.AddAsync(consentOfService);
                currentConsent = addResult.Entity;
            }
            else
            {
                currentConsent.ConsentFile = consentOfService.ConsentFile;
                a3SContext.Entry(currentConsent).State = EntityState.Modified;
            }
            
            await a3SContext.SaveChangesAsync();
            return currentConsent;
        }
    }
}

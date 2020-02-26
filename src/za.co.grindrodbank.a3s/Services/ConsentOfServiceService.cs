/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class ConsentOfServiceService : IConsentOfServiceService
    {
        private readonly IConsentOfServiceRepository consentOfServiceRepository;
        private readonly IMapper mapper;

        public ConsentOfServiceService(IConsentOfServiceRepository consentOfServiceRepository, IMapper mapper)
        {
            this.consentOfServiceRepository = consentOfServiceRepository;
            this.mapper = mapper;
        }

        public async Task<ConsentOfService> GetCurrentConsentAsync()
        {
            var currentConsent = await consentOfServiceRepository.GetCurrentConsentAsync();
            return mapper.Map<ConsentOfService>(currentConsent);
        }

        public async Task<bool> UpdateCurrentConsentAsync(ConsentOfService consentOfService, Guid changedById)
        {
            var consentOfServiceModel = mapper.Map<ConsentOfServiceModel>(consentOfService);
            consentOfServiceModel.ChangedBy = changedById;
            var databaseObj = await consentOfServiceRepository.UpdateCurrentConsentAsync(consentOfServiceModel);
            return databaseObj != null;
        }

        public Task<List<Permission>> GetListOfPermissionsToConsentAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
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
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public ConsentOfServiceService(IConsentOfServiceRepository consentOfServiceRepository, IUserRepository userRepository, IMapper mapper)
        {
            this.consentOfServiceRepository = consentOfServiceRepository;
            this.userRepository = userRepository;
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

        public async Task<List<Permission>> GetListOfPermissionsToConsentAsync(Guid userId)
        {
            var permissions = await consentOfServiceRepository.GetListOfPermissionsToConsentAsync(userId.ToString());
            return mapper.Map<List<Permission>>(permissions);
        }
    }
}
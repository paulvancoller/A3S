/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Linq;

namespace za.co.grindrodbank.a3s.Services
{
    public class LdapAuthenticationModeService : ILdapAuthenticationModeService
    {
        private readonly ILdapAuthenticationModeRepository ldapAuthenticationModeRepository;
        private readonly IMapper mapper;
        private readonly ILdapConnectionService ldapConnectionService;

        public LdapAuthenticationModeService(ILdapAuthenticationModeRepository ldapAuthenticationModeRepository, IMapper mapper, ILdapConnectionService ldapConnectionService)
        {
            this.ldapAuthenticationModeRepository = ldapAuthenticationModeRepository;
            this.ldapConnectionService = ldapConnectionService;
            this.mapper = mapper;
        }

        public async Task<LdapAuthenticationMode> CreateAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit, Guid createdById)
        {
            LdapAuthenticationModeModel existingAuthenticationMode = await ldapAuthenticationModeRepository.GetByNameAsync(ldapAuthenticationModeSubmit.Name, includePassword: false);
            if (existingAuthenticationMode != null)
                throw new ItemNotProcessableException($"LDAP Authentication Mode with Name '{ldapAuthenticationModeSubmit.Name}' already exist.");

            var LdapAuthenticationModeModel = mapper.Map<LdapAuthenticationModeModel>(ldapAuthenticationModeSubmit);
            LdapAuthenticationModeModel.ChangedBy = createdById;

            return mapper.Map<LdapAuthenticationMode>(await ldapAuthenticationModeRepository.CreateAsync(LdapAuthenticationModeModel));
        }

        public async Task<LdapAuthenticationMode> GetByIdAsync(Guid ldapAuthenticationModeId)
        {
            return mapper.Map<LdapAuthenticationMode>(await ldapAuthenticationModeRepository.GetByIdAsync(ldapAuthenticationModeId, includePassword: false));
        }

        public async Task<List<LdapAuthenticationMode>> GetListAsync()
        {
            return mapper.Map<List<LdapAuthenticationMode>>(await ldapAuthenticationModeRepository.GetListAsync(includePassword: false));
        }

        public async Task<LdapAuthenticationMode> UpdateAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit, Guid updatedById)
        {
            LdapAuthenticationModeModel existingAuthenticationMode = await ldapAuthenticationModeRepository.GetByIdAsync(ldapAuthenticationModeSubmit.Uuid);

            if (existingAuthenticationMode == null)
                throw new ItemNotFoundException($"AuthenticationMode with ID '{ldapAuthenticationModeSubmit.Uuid}' not found when attempting to update a authenticationMode using this ID!");

            if (existingAuthenticationMode.Name != ldapAuthenticationModeSubmit.Name)
            {
                // Confirm the new name is available
                LdapAuthenticationModeModel checkExistingNameModel = await ldapAuthenticationModeRepository.GetByNameAsync(ldapAuthenticationModeSubmit.Name, false);
                if (checkExistingNameModel != null)
                    throw new ItemNotProcessableException($"AuthenticationMode with name '{ldapAuthenticationModeSubmit.Name}' already exists.");
            }

            existingAuthenticationMode.Name = ldapAuthenticationModeSubmit.Name;
            existingAuthenticationMode.Account = ldapAuthenticationModeSubmit.Account;
            existingAuthenticationMode.BaseDn = ldapAuthenticationModeSubmit.BaseDn;
            existingAuthenticationMode.HostName = ldapAuthenticationModeSubmit.HostName;
            existingAuthenticationMode.IsLdaps = ldapAuthenticationModeSubmit.IsLdaps;
            existingAuthenticationMode.Password = ldapAuthenticationModeSubmit.Password;
            existingAuthenticationMode.Port = ldapAuthenticationModeSubmit.Port;
            existingAuthenticationMode.ChangedBy = updatedById;

            existingAuthenticationMode.LdapAttributes = mapper.Map<List<LdapAuthenticationModeLdapAttributeModel>>(ldapAuthenticationModeSubmit.LdapAttributes);

            return mapper.Map<LdapAuthenticationMode>(await ldapAuthenticationModeRepository.UpdateAsync(existingAuthenticationMode));
        }

        public Task<ValidationResultResponse> TestAsync(LdapAuthenticationModeSubmit ldapAuthenticationModeSubmit)
        {
            var testResult = new ValidationResultResponse();
            var resultMessages = new List<string>();

            var ldapModel = mapper.Map<LdapAuthenticationModeModel>(ldapAuthenticationModeSubmit);
            testResult.Success = ldapConnectionService.TestLdapSettings(ldapModel, ref resultMessages);
            testResult.Messages = resultMessages;

            return Task.FromResult(testResult);
        }

        public async Task DeleteAsync(Guid ldapAuthenticationModeId)
        {
            var authenticationMode = await ldapAuthenticationModeRepository.GetByIdAsync(ldapAuthenticationModeId, false, true);

            if (authenticationMode == null)
            {
                throw new ItemNotFoundException($"Authentication Mode with GUID '{ldapAuthenticationModeId}' not found.");
            }

            if (authenticationMode.Users != null && authenticationMode.Users.Any())
            {
                throw new ItemNotProcessableException($"Authentication Mode has users still assigned to it. Only Authentication modes without users assigned can be deleted. Not deleting.");
            }

            await ldapAuthenticationModeRepository.DeleteAsync(authenticationMode);
        }
    }
}

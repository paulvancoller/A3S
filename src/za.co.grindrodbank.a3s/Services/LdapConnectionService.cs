/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NLog;
using Novell.Directory.Ldap;
using za.co.grindrodbank.a3s.ConnectionClients;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class LdapConnectionService : ILdapConnectionService
    {
        private readonly ILdapAuthenticationModeRepository ldapAuthenticationModeRepository; 
        private readonly IUserRepository userRepository;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly ILdapConnectionClient ldapConnectionClient;
        private readonly ILdapConnectionClient ldapTestConnectionClient;

        public LdapConnectionService(ILdapAuthenticationModeRepository ldapAuthenticationModeRepository, IUserRepository userRepository,
            ILdapConnectionClient ldapConnectionClient, ILdapConnectionClient ldapTestConnectionClient)
        {
            this.ldapAuthenticationModeRepository = ldapAuthenticationModeRepository;
            this.userRepository = userRepository;
            this.ldapConnectionClient = ldapConnectionClient;
            this.ldapTestConnectionClient = ldapTestConnectionClient;
        }

        public async Task<SignInResult> Login(UserModel appUser, string password)
        {
            if (await FindUser(appUser.UserName, appUser.LdapAuthenticationMode, password, true, true))
                return SignInResult.Success;
            
            return SignInResult.Failed;
        }

        public async Task<bool> CheckIfUserExist(string userName, Guid ldapAuthenticationModeId)
        {
            var ldapAuthMode = await ldapAuthenticationModeRepository.GetByIdAsync(ldapAuthenticationModeId, true);
            if (ldapAuthMode == null)
                throw new ItemNotFoundException($"LDAP Authentication Mode with Id {ldapAuthenticationModeId} could not be found.");

            return await FindUser(userName, ldapAuthMode, string.Empty, false, false);
        }

        public bool TestLdapSettings(LdapAuthenticationModeModel ldapAuthenticationModeModel, ref List<string> returnMessages)
        {

            try
            {
                logger.Info($"Testing LDAP connection.");

                var distinguishedName = $"cn={ldapAuthenticationModeModel.Account},{ldapAuthenticationModeModel.BaseDn}";

                logger.Info($"Attempting to connect to LDAP connection. Hostname '{ldapAuthenticationModeModel.HostName}', Port '{ldapAuthenticationModeModel.Port}'");
                ldapConnectionClient.Connect(ldapAuthenticationModeModel.HostName, ldapAuthenticationModeModel.Port);
                logger.Info($"Attempting to bind with DN '{distinguishedName}'");
                ldapConnectionClient.Bind(distinguishedName, ldapAuthenticationModeModel.Password);

                if (ldapAuthenticationModeModel.LdapAttributes != null && ldapAuthenticationModeModel.LdapAttributes.Count > 0)
                    returnMessages.AddRange(TestLdapAttributes(ldapAuthenticationModeModel));

                return true;
            }
            catch (LdapException lEx)
            {
                logger.Error(lEx.Message, lEx);
                returnMessages.Add(lEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                string message = "Generic error during LDAP connection test.";
                logger.Error(ex, message);
                returnMessages.Add(message);
                return false;
            }
        }

        private async Task<bool> FindUser(string userName, LdapAuthenticationModeModel ldapAuthenticationMode, string password, bool testLogin, bool syncLdapAttributes)
        {
            try
            {
                var adminDistinguishedName = $"cn={ldapAuthenticationMode.Account},{ldapAuthenticationMode.BaseDn}";

                // Search for user in directory
                var searchBase = $"{ldapAuthenticationMode.BaseDn}";
                var searchFilter = $"(cn = {userName})";

                try
                {
                    logger.Info($"Attempting to connect to LDAP connection. Hostname '{ldapAuthenticationMode.HostName}', Port '{ldapAuthenticationMode.Port}'");
                    ldapConnectionClient.Connect(ldapAuthenticationMode.HostName, ldapAuthenticationMode.Port);
                    logger.Info($"Attempting to bind with DN '{adminDistinguishedName}'");
                    ldapConnectionClient.Bind(adminDistinguishedName, ldapAuthenticationMode.Password);
                }
                catch (Exception ex)
                {
                    // User not authenticated
                    logger.Error(ex, "LDAP admin account user not authenticated.");
                    return false;
                }

                LdapSearchResults lsc = ldapConnectionClient.Search(searchBase, LdapConnection.SCOPE_SUB, searchFilter, null, false);

                while (lsc.hasMore())
                {
                    LdapEntry ldapEntry;

                    try
                    {
                        ldapEntry = lsc.next();
                    }
                    catch (LdapException ex)
                    {
                        logger.Warn("Warning on LDAP search: " + ex.LdapErrorMessage);
                        continue;
                    }

                    // User found, try to log in
                    if (testLogin)
                    {
                        try
                        {
                            logger.Info($"Attempting to connect to LDAP connection. Hostname '{ldapAuthenticationMode.HostName}', Port '{ldapAuthenticationMode.Port}'");
                            ldapTestConnectionClient.Connect(ldapAuthenticationMode.HostName, ldapAuthenticationMode.Port);
                            logger.Info($"Attempting to bind with DN '{ldapEntry.DN}'");
                            ldapTestConnectionClient.Bind(ldapEntry.DN, password);
                        }
                        catch (Exception ex)
                        {
                            // User not authenticated
                            logger.Error(ex, "LDAP user not authenticated.");
                            return false;
                        }
                    }

                    // Login successful. Sync attributes
                    if (syncLdapAttributes)
                    {
                        LdapAttributeSet attributeSet = ldapEntry.getAttributeSet();
                        System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                        var attributeValues = new Dictionary<string, string>();

                        while (ienum.MoveNext())
                        {
                            var attribute = (LdapAttribute)ienum.Current;
                            attributeValues.Add(attribute.Name, attribute.StringValue);
                        }

                        await SyncLdapAttributeToUserField(userName, ldapAuthenticationMode.LdapAttributes, attributeValues);
                    }

                    return true;
                }

                // If we reached this point, we were not able authenticate the user
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "General error during LDAP authentication process.");
                return false;
            }
        }

        private async Task SyncLdapAttributeToUserField(string userName, List<LdapAuthenticationModeLdapAttributeModel> ldapAttributes, Dictionary<string, string> attributeList)
        {
            var appUser = await userRepository.GetByUsernameAsync(userName, false);
            if (appUser == null)
                throw new ItemNotFoundException($"User with username {userName} could not be found.");

            bool userHasChanged = false;
            foreach (LdapAuthenticationModeLdapAttributeModel attributeMapping in ldapAttributes)
            {
                string ldapValue = string.Empty;

                if (attributeList.TryGetValue(attributeMapping.LdapField, out ldapValue))
                    UpdateUserField(ref appUser, ref userHasChanged, attributeMapping.UserField, ldapValue);
            }

            if (userHasChanged)
                await userRepository.UpdateAsync(appUser);
        }

        private void UpdateUserField(ref UserModel appUser, ref bool userHasChanged, string userField, string value)
        {
            switch (userField.ToLower())
            {
                case "username":
                    appUser.UserName = value;
                    userHasChanged = true;
                    break;
                case "firstname":
                    appUser.FirstName = value;
                    userHasChanged = true;
                    break;
                case "surname":
                    appUser.Surname = value;
                    userHasChanged = true;
                    break;
                case "email":
                    appUser.Email = value;
                    userHasChanged = true;
                    break;
                case "avatar":
                    var binaryFormatter = new BinaryFormatter();

                    using (var memoryStream = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStream, value);
                        appUser.Avatar = memoryStream.ToArray();
                    }

                    userHasChanged = true;
                    break;
            }
        }

        private List<string> TestLdapAttributes(LdapAuthenticationModeModel ldapAuthenticationModeModel)
        {
            var resultMessages = new List<string>();

            try
            {
                string searchBase = $"{ldapAuthenticationModeModel.BaseDn}";
                string searchFilter = $"(cn = {ldapAuthenticationModeModel.Account})";

                var ldapSearchResults = ldapConnectionClient.Search(searchBase, LdapConnection.SCOPE_SUB, searchFilter, null, false);

                while (ldapSearchResults.hasMore())
                {
                    try
                    {
                        var ldapEntry = ldapSearchResults.next();
                        ProcessLdapSearchResult(ldapEntry, ldapAuthenticationModeModel, resultMessages);
                    }
                    catch (LdapException ex)
                    {
                        logger.Warn("Warning on LDAP search: " + ex.LdapErrorMessage);
                    }
                }
            }
            catch
            {
                throw new LdapException("Error verifying LDAP attributes.", LdapException.NO_SUCH_ATTRIBUTE, string.Empty);
            }

            return resultMessages;
        }

        private void ProcessLdapSearchResult(LdapEntry ldapEntry, LdapAuthenticationModeModel ldapAuthenticationModeModel, List<string> resultMessages)
        {
            var ldapAttributeSet = ldapEntry.getAttributeSet();
            var ienum = ldapAttributeSet.GetEnumerator();
            var attributeValues = new Dictionary<string, string>();

            while (ienum.MoveNext())
            {
                var attribute = (LdapAttribute)ienum.Current;
                attributeValues.Add(attribute.Name, attribute.StringValue);
            }

            foreach (LdapAuthenticationModeLdapAttributeModel attributeMapping in ldapAuthenticationModeModel.LdapAttributes)
            {
                var ldapValue = string.Empty;

                if (!attributeValues.TryGetValue(attributeMapping.LdapField, out ldapValue))
                {
                    var message = $"LDAP attribute {attributeMapping.LdapField} not found.";
                    logger.Info(message);
                    resultMessages.Add(message);
                }
            }
        }
    }
}

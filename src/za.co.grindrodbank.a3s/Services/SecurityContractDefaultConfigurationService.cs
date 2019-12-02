/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.Services
{
    public class SecurityContractDefaultConfigurationService : ISecurityContractDefaultConfigurationService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IFunctionRepository functionRepository;
        private readonly IUserRepository userRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IApplicationRepository applicationRepository;
        private readonly IApplicationDataPolicyRepository applicationDataPolicyRepository;
        private readonly ILdapAuthenticationModeRepository ldapAuthenticationModeRepository;

        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public SecurityContractDefaultConfigurationService(IRoleRepository roleRepository, IUserRepository userRepository, IFunctionRepository functionRepository, ITeamRepository teamRepository, IApplicationRepository applicationRepository, IApplicationDataPolicyRepository applicationDataPolicyRepository, ILdapAuthenticationModeRepository ldapAuthenticationModeRepository)
        {
            this.roleRepository = roleRepository;
            this.userRepository = userRepository;
            this.functionRepository = functionRepository;
            this.teamRepository = teamRepository;
            this.applicationRepository = applicationRepository;
            this.applicationDataPolicyRepository = applicationDataPolicyRepository;
            this.ldapAuthenticationModeRepository = ldapAuthenticationModeRepository;
        }

        public async Task ApplyDefaultConfigurationDefinitionAsync(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            logger.Debug($"[defautlConfigurations.name: '{securityContractDefaultConfiguration.Name}']: Applying default configuration: '{securityContractDefaultConfiguration.Name}'");
            await ApplyAllDefaultApplications(securityContractDefaultConfiguration, updatedById, dryRun, validationErrors);
            await ApplyAllDefaultRoles(securityContractDefaultConfiguration, updatedById, dryRun, validationErrors);
            await ApplyAllDefaultLdapAuthModes(securityContractDefaultConfiguration, updatedById, dryRun, validationErrors);
            await ApplyAllDefaultUsers(securityContractDefaultConfiguration, updatedById, dryRun, validationErrors);
            await ApplyAllDefaultTeams(securityContractDefaultConfiguration, updatedById);
        }

        /// <summary>
        /// Applies all the default configurations for defined applications. This is essentially creating 'business' functions comprised of permissions for
        /// all applications that are defined within the applications section of the default configuration component of the security contract.
        /// </summary>
        /// <param name="securityContractDefaultConfiguration"></param>
        /// <returns></returns>
        private async Task ApplyAllDefaultApplications(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            if (securityContractDefaultConfiguration.Applications == null || securityContractDefaultConfiguration.Applications.Count == 0)
            {
                logger.Debug($"[defautlConfigurations.name: '{securityContractDefaultConfiguration.Name}'].[applications]: Null or empty applications section. Skipping!");
                return;
            }


            foreach (var defaultApplication in securityContractDefaultConfiguration.Applications)
                await ApplyIndividualDefaultApplication(defaultApplication, updatedById, securityContractDefaultConfiguration.Name, dryRun, validationErrors);
        }

        private async Task ApplyIndividualDefaultApplication(SecurityContractDefaultConfigurationApplication defaultApplication, Guid updatedById, string defaultConfigurationName, bool dryRun, List<string> validationErrors)
        {
            // Ensure that the application actually exists. Obtain all the application function relations too, as they will be used for validation.
            var application = await applicationRepository.GetByNameAsync(defaultApplication.Name);

            if (application == null)
            {
                var errorMessage = $"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions]: Application '{defaultApplication.Name}' does not exist within A3S. Cannot assign functions to it!";
                if (dryRun)
                {
                    // add to the validation errors and attempt to continue
                    validationErrors.Add(errorMessage);
                }
                else
                {
                    throw new ItemNotFoundException(errorMessage);
                }
            }

            if (defaultApplication.Functions == null || defaultApplication.Functions.Count == 0)
            {
                logger.Warn($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions]: Default application '{defaultApplication.Name}' has no 'functions' defined within it. Ignoring default configuration for this application.");
                return;
            }

            // Use the currrent state of the assigned application functions to determine which ones are potentially no longer in the YAML.
            await DetectFunctionsRemovedFromSecurityContractDefaultsAndRemoveFromApplication(application, defaultApplication.Functions, dryRun, validationErrors, defaultApplication.Name, defaultConfigurationName);

            // Reset the state of the application functions, as the security contract declares the desired state and we have already used to the historic state
            // to clear any functions that don't appear within the security contract.
            application.Functions = new List<FunctionModel>();
            application.ChangedBy = updatedById;

            foreach (var defaultFunction in defaultApplication.Functions)
            {
                logger.Debug($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{defaultFunction.Name}']: Preparing to add function '{defaultFunction.Name}' to application '{defaultApplication.Name}'.");
                if (defaultFunction.Permissions == null || defaultFunction.Permissions.Count == 0)
                {
                    logger.Warn($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{defaultFunction.Name}']: No permissions defined for function '{defaultFunction.Name}'. Not assiging it to application '{defaultApplication.Name}' ");

                    continue;
                }

                var functionModelToAdd = new FunctionModel();
                // check to see if there is an existing function.
                var existingFunction = await functionRepository.GetByNameAsync(defaultFunction.Name);

                if (existingFunction != null)
                    functionModelToAdd = existingFunction;

                functionModelToAdd.Application = application;
                functionModelToAdd.Description = defaultFunction.Description;
                functionModelToAdd.Name = defaultFunction.Name;
                functionModelToAdd.ChangedBy = updatedById;

                // Clear the current permissions assigned to the function as they are to be re-created.
                functionModelToAdd.FunctionPermissions = new List<FunctionPermissionModel>();

                AddPermissionsToFunctionsEnsuringTheyExistsAndAreAssigedToTheApplication(application, defaultFunction, defaultApplication, functionModelToAdd, updatedById, defaultConfigurationName, dryRun, validationErrors);
            }

            // Update the application with its new application function state.
            await applicationRepository.Update(application);
        }

        /// <summary>
        /// Adds permissions to an application function. However, before this is done, checks are run to ensure that the permissions is defined for the application by being present
        /// within at least one of the application functions (created by the micro-service).
        /// </summary>
        /// <param name="application"></param>
        /// <param name="defaultFunction"></param>
        /// <param name="defaultApplication"></param>
        /// <param name="functionModelToAdd"></param>
        private void AddPermissionsToFunctionsEnsuringTheyExistsAndAreAssigedToTheApplication(ApplicationModel application, SecurityContractDefaultConfigurationFunction defaultFunction,
            SecurityContractDefaultConfigurationApplication defaultApplication, FunctionModel functionModelToAdd, Guid updatedById, string defaultConfigurationName, bool dryRun, List<string> validationErrors)
        {
            foreach (var permissionToAddToFunction in defaultFunction.Permissions)
            {
                bool permissionIsApplicationPermission = false;
                // Ensure the permission actually exists in at least one of the application functions. Only add the permission to the function if it does.
                if (application.ApplicationFunctions == null || application.ApplicationFunctions.Count == 0)
                {
                    logger.Warn($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{defaultFunction.Name}']: ");
                    break;
                }

                foreach (var applicationFunction in application.ApplicationFunctions)
                {
                    var permission = applicationFunction.ApplicationFunctionPermissions.Find(afp => afp.Permission.Name == permissionToAddToFunction);
                    if (permission != null)
                    {
                        logger.Debug($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{functionModelToAdd.Name}'].[permission.name: '{permissionToAddToFunction}']: Assigning permission '{permissionToAddToFunction}' to function: '{functionModelToAdd.Name}'.");
                        permission.ChangedBy = updatedById;
                        permissionIsApplicationPermission = true;
                        functionModelToAdd.FunctionPermissions.Add(new FunctionPermissionModel
                        {
                            Function = functionModelToAdd,
                            Permission = permission.Permission,
                            ChangedBy = updatedById
                        });
                    }
                }

                if (!permissionIsApplicationPermission)
                {
                    var errorMessage = $"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{functionModelToAdd.Name}'].[permission.name: '{permissionToAddToFunction}']: Permission '{permissionToAddToFunction}' is not an existing permission within application '{application.Name}'.";
                    if (dryRun)
                    {
                        validationErrors.Add(errorMessage);
                    }
                    else
                    {
                        throw new ItemNotFoundException(errorMessage);
                    }
                }
            }

            // Persist the application function, but only if there is at least a single permission associated with it.
            if (functionModelToAdd.FunctionPermissions.Count > 0)
            {
                logger.Debug($"[defautlConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{defaultApplication.Name}'].[functions.name: '{functionModelToAdd.Name}']: Assigning function '{functionModelToAdd.Name}' to application '{application.Name}'");
                application.Functions.Add(functionModelToAdd);
            }
        }

        /// <summary>
        /// Detects functions that are currently assigned to an application, but do not appear within the security contract definition and removes them.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="securityContractDefaultConfigurationFunctions"></param>
        /// <returns></returns>
        private async Task<ApplicationModel> DetectFunctionsRemovedFromSecurityContractDefaultsAndRemoveFromApplication(ApplicationModel application, List<SecurityContractDefaultConfigurationFunction> securityContractDefaultConfigurationFunctions, bool dryRun, List<string> validationErrors, string applicationName, string defaultConfigurationName)
        {
            if (application.Functions.Count > 0)
            {
                //logger.Debug($"[defaultConfiguration.name: '{defaultConfigurationName}'].[applications.name: '{applicationName}']: {application.Functions.Count}");
                for (int i = application.Functions.Count - 1; i >= 0; i--)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{applicationName}'].[functions.name: '{application.Functions[i].Name}']: Checking whether function: '{application.Functions[i].Name}' should be removed from application '{application.Name}'.");
                    if (!securityContractDefaultConfigurationFunctions.Exists(f => f.Name == application.Functions[i].Name))
                    {
                        logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[applications.name: '{applicationName}'].[functions.name: '{application.Functions[i].Name}']: Function: '{application.Functions[i].Name}' is being removed from application '{application.Name}' as it is currently assigned to the application within A3S, but is no longer defined within the security contract being processed!");
                        // Note: This only removes the function permissions association. The permission will still exist. We cannot remove the permission here, as it may be assigned to other functions.
                        await functionRepository.DeleteAsync(application.Functions[i]);
                    }
                }
            }

            return application;
        }

        /// <summary>
        /// Processes the 'roles' component of the 'defaultConfigurations' section of a security contract. Creates or updates all the roles in this list. Overwrites the
        /// functions that are assigned to the role with the list of functions defined in the security contract.
        /// </summary>
        /// <param name="securityContractDefaultConfiguration"></param>
        /// <returns></returns>
        private async Task ApplyAllDefaultRoles(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            logger.Debug($"[defaultConfigurations.name: '{securityContractDefaultConfiguration.Name}'][roles]: Processing roles for default config: '{securityContractDefaultConfiguration.Name}'");
            // Ensure all default roles are created first.
            // Recall, this is an optional component of the default configuration so ensure that it is persent.
            if (securityContractDefaultConfiguration.Roles == null || securityContractDefaultConfiguration.Roles.Count == 0)
                return;

            foreach (var simpleRole in securityContractDefaultConfiguration.Roles)
            {
                await ApplyDefaultRole(simpleRole, updatedById, dryRun, validationErrors, securityContractDefaultConfiguration.Name);
            }
        }

        private async Task ApplyDefaultRole(SecurityContractDefaultConfigurationRole defaultRole, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            var defaultRoleToApply = new RoleModel();
            bool roleIsNew = false;

            logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Processing role: '{defaultRole.Name}'");
            var existingRole = await roleRepository.GetByNameAsync(defaultRole.Name);

            if (existingRole != null)
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}' already exist. Updating it.");
                defaultRoleToApply = existingRole;
            }
            else
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}' does not exist. Creating it.");
                roleIsNew = true;
            }

            // Overwrite any assigned functions with the new list. If the list is empty, we still want to clear any assigned functions.
            defaultRoleToApply.Name = defaultRole.Name;
            defaultRoleToApply.Description = defaultRole.Description;
            defaultRoleToApply.ChangedBy = updatedById;

            await ApplyFunctionsToDefaultRole(defaultRole, defaultRoleToApply, updatedById, dryRun, validationErrors, defaultConfigurationName);
            await ApplyChildRolesToDefaultRole(defaultRole, defaultRoleToApply, updatedById, dryRun, validationErrors, defaultConfigurationName);

            try
            {
                if (roleIsNew)
                {
                    await roleRepository.CreateAsync(defaultRoleToApply);
                }
                else
                {
                    await roleRepository.UpdateAsync(defaultRoleToApply);
                }
            } catch (Exception e)
            {
                var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Error persisting role '{defaultRole.Name}' to the data store. Exception: {e.Message}";
                if (dryRun)
                {
                    validationErrors.Add(errorMessage);
                }
                else
                {
                    throw;
                }
            }
           
        }

        private async Task ApplyFunctionsToDefaultRole(SecurityContractDefaultConfigurationRole defaultRole, RoleModel defaultRoleToApply, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            defaultRoleToApply.RoleFunctions = new List<RoleFunctionModel>();

            if (defaultRole.Functions != null && defaultRole.Functions.Count > 0)
            {
                foreach (var functionToAdd in defaultRole.Functions)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Attempting to add function: '{functionToAdd}' to role '{defaultRole.Name}'.");
                    // Ensure that the function exists.
                    var existingFunction = await functionRepository.GetByNameAsync(functionToAdd);

                    if (existingFunction == null)
                    {
                        var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Function '{functionToAdd}' does not exist. Cannot assign it to role '{defaultRole.Name}'.";

                        if (dryRun)
                        {
                            validationErrors.Add(errorMessage);
                            continue;
                        }

                        throw new ItemNotFoundException(errorMessage);
                    }

                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Function '{functionToAdd}' exists and is being assigned to role '{defaultRole.Name}'.");

                    defaultRoleToApply.RoleFunctions.Add(new RoleFunctionModel
                    {
                        Role = defaultRoleToApply,
                        Function = existingFunction,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        private async Task ApplyChildRolesToDefaultRole(SecurityContractDefaultConfigurationRole defaultRole, RoleModel defaultRoleToApply, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            defaultRoleToApply.ChildRoles = new List<RoleRoleModel>();

            if (defaultRole.Roles != null && defaultRole.Roles.Count > 0)
            {
                foreach (var roleToAdd in defaultRole.Roles)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Attempting to add child role: '{roleToAdd}' to role '{defaultRole.Name}'.");

                    // Ensure that the role exists.
                    var existingChildRole = await roleRepository.GetByNameAsync(roleToAdd);

                    if (existingChildRole == null)
                    {
                        var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Child role '{roleToAdd}' does not exist. Cannot assign it to parent role '{defaultRole.Name}'.";

                        if (dryRun)
                        {
                            validationErrors.Add(errorMessage);
                            continue;
                        }

                        throw new ItemNotFoundException(errorMessage);
                    }

                    // Only non-compound roles can be added to compound roles. Therefore, prior to adding the potential child role to the parent role, it must be
                    // asserted that the child row has no child roles attached to it.
                    if (existingChildRole.ChildRoles.Count > 0)
                    {
                        var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Assigning a compound role as a child of a role is prohibited. Attempting to add Role '{existingChildRole.Name} with ID: '{existingChildRole.Id}' as a child role of Role: '{defaultRole.Name}'. However, it already has '{existingChildRole.ChildRoles.Count}' child roles assigned to it! Not adding it.";

                        if (dryRun)
                        {
                            validationErrors.Add(errorMessage);
                            continue;
                        }

                        throw new ItemNotProcessableException(errorMessage);
                    }

                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'][roles.name: '{defaultRole.Name}']: Role: '{defaultRole.Name}': Child role '{existingChildRole.Name}' exists and is being assigned to role '{defaultRole.Name}'.");

                    defaultRoleToApply.ChildRoles.Add(new RoleRoleModel
                    {
                        ParentRole = defaultRoleToApply,
                        ChildRole = existingChildRole,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        /// <summary>
        /// Processes the 'LdapAuthModes' component of the 'defaultConfigurations' component of a consolidated security contract. Creates or updates all the LDAP Auth modes defined
        /// within the list. Overwrites all roles that the users have with the list of roles defined for each user.
        /// </summary>
        /// <param name="securityContractDefaultConfiguration"></param>
        /// <returns></returns>
        private async Task ApplyAllDefaultLdapAuthModes(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            logger.Debug($"[defaultConfigurations.name: '{securityContractDefaultConfiguration.Name}'].[ldapAuthenticationModes]: Applying default LDAP Authentication modes configuration.");

            if (securityContractDefaultConfiguration.LdapAuthenticationModes == null || securityContractDefaultConfiguration.LdapAuthenticationModes.Count == 0)
                return;

            foreach (var defaultLdapAuthMode in securityContractDefaultConfiguration.LdapAuthenticationModes)
                await ApplyIndividualDefaultLdapAuthMode(defaultLdapAuthMode, updatedById, dryRun, validationErrors, securityContractDefaultConfiguration.Name);
        }

        private async Task ApplyIndividualDefaultLdapAuthMode(SecurityContractDefaultConfigurationLdapAuthMode defaultLdapAuthMode, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Operating on default ldap auth mode '{defaultLdapAuthMode.Name}'.");
            var defaultLdapAuthToApply = new LdapAuthenticationModeModel();

            bool newLdapAuthMode = false;
            var existingLdapAuthMode = await ldapAuthenticationModeRepository.GetByNameAsync(defaultLdapAuthMode.Name, false);

            if (existingLdapAuthMode != null)
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Default LDAP Auth Mode: '{defaultLdapAuthMode.Name}' already exist. Updating it.");
                defaultLdapAuthToApply = existingLdapAuthMode;
            }
            else
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Default LDAP Auth Mode: '{defaultLdapAuthMode.Name}' does not exist. Creating it.");
                newLdapAuthMode = true;
            }

            // Map the base components.
            defaultLdapAuthToApply.Name = defaultLdapAuthMode.Name;
            defaultLdapAuthToApply.HostName = defaultLdapAuthMode.HostName;
            defaultLdapAuthToApply.Port = defaultLdapAuthMode.Port;
            defaultLdapAuthToApply.IsLdaps = defaultLdapAuthMode.IsLdaps;
            defaultLdapAuthToApply.Account = defaultLdapAuthMode.Account;
            defaultLdapAuthToApply.Password = string.Empty;
            defaultLdapAuthToApply.BaseDn = defaultLdapAuthMode.BaseDn;
            defaultLdapAuthToApply.ChangedBy = updatedById;

            defaultLdapAuthToApply.LdapAttributes = new List<LdapAuthenticationModeLdapAttributeModel>();
            foreach (var attributeToApply in defaultLdapAuthMode.LdapAttributes)
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}'].[ldapAttributes]: Adding LDAP attribute: {attributeToApply.UserField} -> {attributeToApply.LdapField} to LDAP auth mode '{defaultLdapAuthMode.Name}'.");

                defaultLdapAuthToApply.LdapAttributes.Add(new LdapAuthenticationModeLdapAttributeModel()
                {
                    UserField = attributeToApply.UserField,
                    LdapField = attributeToApply.LdapField
                });
            }

            try
            {
                if (newLdapAuthMode)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Persisting new Ldap Auth Mode '{defaultLdapAuthMode.Name}' into the database.");
                    await ldapAuthenticationModeRepository.CreateAsync(defaultLdapAuthToApply);
                }
                else
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Updating existing Ldap Auth Mode '{defaultLdapAuthMode.Name}' into the database.");
                    await ldapAuthenticationModeRepository.UpdateAsync(defaultLdapAuthToApply);
                }
            } catch (Exception e)
            {
                var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'].[ldapAuthenticationModes.name: '{defaultLdapAuthMode.Name}']: Error persisting LDAP Auth Mode '{defaultLdapAuthMode.Name}'. Error: '{e.Message}'";

                if (dryRun)
                {
                    validationErrors.Add(errorMessage);
                }
                else
                {
                    throw;
                }
            }
            
        }

        /// <summary>
        /// Processes the 'users' component of the 'defaultConfigurations' component of a consolidated security contract. Creates or updates all the users defined
        /// within the list. Overwrites all roles that the users have with the list of roles defined for each user.
        /// </summary>
        /// <param name="securityContractDefaultConfiguration"></param>
        /// <returns></returns>
        private async Task ApplyAllDefaultUsers(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById, bool dryRun, List<string> validationErrors)
        {
            logger.Debug($"[defaultConfigurations.name: '{securityContractDefaultConfiguration.Name}'].[users]: Processing users configuration.");

            if (securityContractDefaultConfiguration.Users == null || securityContractDefaultConfiguration.Users.Count == 0)
                return;

            foreach (var defaultUser in securityContractDefaultConfiguration.Users)
                await ApplyIndividualDefaultUser(defaultUser, updatedById, dryRun, validationErrors, securityContractDefaultConfiguration.Name);
        }

        private async Task ApplyIndividualDefaultUser(SecurityContractDefaultConfigurationUser defaultUser, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: Processing user '{defaultUser.Username}'.");
            var defaultUserToApply = new UserModel();

            bool newUser = false;
            // Get the user with all it's associated relations, as we are going to be tweaking some of them, especially any assigned roles.
            var existingUser = await userRepository.GetByUsernameAsync(defaultUser.Username, true);

            if (existingUser != null)
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: User '{defaultUser.Username}' already exist. Updating it.");
                defaultUserToApply = existingUser;
            }
            else
            {
                logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: User: '{defaultUser.Username}' does not exist. Creating it.");
                newUser = true;
                // We can also bind the user's UUID here, but only if one is supplied. We should not bind it on any user updates, as UUIDs associated with users cannot be changed!
                // Recall: The user ID within the dotnet Identity table is purposely a string. Convert the GUID representation accordingly.
                if (defaultUser.Uuid != Guid.Empty)
                {
                    defaultUserToApply.Id = defaultUser.Uuid.ToString();
                }
            }

            // Map the base components.
            defaultUserToApply.Email = defaultUser.Email.Trim();
            defaultUserToApply.NormalizedEmail = defaultUserToApply.Email.ToUpper();
            defaultUserToApply.EmailConfirmed = true;
            defaultUserToApply.PhoneNumber = defaultUser.PhoneNumber;
            defaultUserToApply.PhoneNumberConfirmed = (defaultUserToApply.PhoneNumber != null);
            defaultUserToApply.UserName = defaultUser.Username.Trim();
            defaultUserToApply.NormalizedUserName = defaultUserToApply.UserName.ToUpper();
            defaultUserToApply.FirstName = defaultUser.Name;
            defaultUserToApply.Surname = defaultUser.Surname;
            defaultUserToApply.IsDeleted = defaultUser.IsDeleted;
            defaultUserToApply.DeletedTime = defaultUser.DeletedTime;
            defaultUserToApply.ChangedBy = updatedById;

            if (!string.IsNullOrEmpty(defaultUser.Avatar))
                defaultUserToApply.Avatar = Convert.FromBase64String(defaultUser.Avatar);

            // Overwrite any potentially assigned roles, as the declarative representation in the defaultUser is the authoratative version and must overwite any existing state.
            defaultUserToApply.UserRoles = new List<UserRoleModel>();

            await ApplyRolesToDefaultUser(defaultUser, defaultUserToApply, updatedById, dryRun, validationErrors, defaultConfigurationName);
            await ApplyLdapAuthModeToDefaultUser(defaultUser, defaultUserToApply, dryRun, validationErrors, defaultConfigurationName);

            try
            {
                if (newUser)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: Persisting new user '{defaultUser.Username}'.");

                    if (string.IsNullOrWhiteSpace(defaultUser.HashedPassword))
                        await userRepository.CreateAsync(defaultUserToApply, defaultUser.Password, isPlainTextPassword: true);
                    else
                        await userRepository.CreateAsync(defaultUserToApply, defaultUser.HashedPassword, isPlainTextPassword: false);
                }
                else
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: Persisting existing user '{defaultUser.Username}'.");
                    await userRepository.UpdateAsync(defaultUserToApply);
                }
            } catch (Exception e)
            {
                var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}']: Error persisting user. Error: {e.Message}";

                if (dryRun)
                {
                    validationErrors.Add(errorMessage);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task ApplyRolesToDefaultUser(SecurityContractDefaultConfigurationUser defaultUser, UserModel defaultUserToApply, Guid updatedById, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            if (defaultUser.Roles != null && defaultUser.Roles.Count > 0)
            {
                foreach (var roleToApply in defaultUser.Roles)
                {
                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}'].[roles.name: '{roleToApply}']: Attempting to add role '{roleToApply}' to user '{defaultUser.Username}'");
                    // Ensure the role actually exists first. If it does not, ignore the role assignment.
                    var existingRole = await roleRepository.GetByNameAsync(roleToApply);

                    if (existingRole == null)
                    {
                        var errorMessage = $"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}'].[roles.name: '{roleToApply}']: Role '{roleToApply}' not found when attempting to apply it to user '{defaultUser.Username}'.";

                        if (dryRun)
                        {
                            validationErrors.Add(errorMessage);
                        }
                        else
                        {
                            throw new ItemNotFoundException(errorMessage);
                        }
                    }

                    logger.Debug($"[defaultConfigurations.name: '{defaultConfigurationName}'].[users.username: '{defaultUser.Username}'].[roles.name: '{roleToApply}']: Assigning role '{roleToApply}' to user '{defaultUser.Username}'.");

                    defaultUserToApply.UserRoles.Add(new UserRoleModel
                    {
                        User = defaultUserToApply,
                        Role = existingRole,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        private async Task ApplyLdapAuthModeToDefaultUser(SecurityContractDefaultConfigurationUser defaultUser, UserModel defaultUserToApply, bool dryRun, List<string> validationErrors, string defaultConfigurationName)
        {
            if (!string.IsNullOrWhiteSpace(defaultUser.LdapAuthenticationMode))
            {
                logger.Debug($"Attempting to add Ldap Auth mode '{defaultUser.LdapAuthenticationMode}' to user '{defaultUser.Username}'");
                // Ensure the Ldap Auth mode actually exists first. If it does not, ignore the Ldap Auth mode assignment.
                var existingLdapAuthMode = await ldapAuthenticationModeRepository.GetByNameAsync(defaultUser.LdapAuthenticationMode);

                if (existingLdapAuthMode == null)
                {
                    throw new ItemNotFoundException($"Ldap Auth mode '{defaultUser.LdapAuthenticationMode}' not found when attempting to apply it to user '{defaultUser.Username}'.");
                }

                logger.Debug($"Ldap Auth mode '{defaultUser.LdapAuthenticationMode}' does exist within the database. Assigning it to user '{defaultUser.Username}'.");
                defaultUserToApply.LdapAuthenticationMode = existingLdapAuthMode;
                defaultUserToApply.LdapAuthenticationModeId = existingLdapAuthMode.Id;
            }
        }

        /// <summary>
        /// Processes the 'teams' component of the 'defaultConfigurations' section of a consolidated security contract. Either creates or updates an existing team.
        /// Overwrites user the memebership of a team with the list of users defined wihtin the security contract.
        /// </summary>
        /// <param name="securityContractDefaultConfiguration"></param>
        /// <returns></returns>
        private async Task ApplyAllDefaultTeams(SecurityContractDefaultConfiguration securityContractDefaultConfiguration, Guid updatedById)
        {
            logger.Debug($"Applying default teams configuration for default config: '{securityContractDefaultConfiguration.Name}'");

            if (securityContractDefaultConfiguration.Teams == null || securityContractDefaultConfiguration.Teams.Count == 0)
                return;

            // Execute only the simple team creation tasks in parallel
            logger.Debug("Applying simple team...");
            foreach (var simpleTeam in securityContractDefaultConfiguration.Teams.Where(x => x.Teams == null || x.Teams.Count == 0))
                await ApplyDefaultTeam(simpleTeam, updatedById);

            logger.Debug("Simple team applied.");

            // Now that all simple teams are created, execute the compound team creation tasks in parallel
            logger.Debug("Applying compound team...");
            foreach (var compoundTeam in securityContractDefaultConfiguration.Teams.Where(x => x.Teams != null && x.Teams.Count > 0))
                await ApplyDefaultTeam(compoundTeam, updatedById);

            logger.Debug("Compound team applied.");
        }

        public async Task ApplyDefaultTeam(SecurityContractDefaultConfigurationTeam defaultTeamToApply, Guid updatedById)
        {
            var teamModel = new TeamModel();
            bool newTeam = false;

            var existingTeam = await teamRepository.GetByNameAsync(defaultTeamToApply.Name, true);

            if (existingTeam == null)
            {
                logger.Debug($"No existing team with name '{defaultTeamToApply.Name}'. Creating a new team.");
                newTeam = true;

                // We can also bind the team's UUID here, but only if one is supplied. We should not bind it on any team updates, as UUIDs associated with teams cannot be changed!
                if (defaultTeamToApply.Uuid != Guid.Empty)
                    teamModel.Id = defaultTeamToApply.Uuid;
            }
            else
            {
                logger.Debug($"Existing team with name: '{defaultTeamToApply.Name}' found. Updating the existing team.");
                teamModel = existingTeam;
            }

            teamModel.Name = defaultTeamToApply.Name;
            teamModel.Description = defaultTeamToApply.Description;
            teamModel.ChangedBy = updatedById;
            teamModel.ChildTeams = new List<TeamTeamModel>();

            // It is very important to do the team assignments first, as the udpated teams configuration may affect whether users are able to be assigned to the team (Users cannot be directly assgined to compound teams)!
            await AssignChildTeamsToTeam(defaultTeamToApply, teamModel, updatedById);
            await AssignUsersToTeamFromUserNameList(teamModel, defaultTeamToApply.Users, updatedById);
            await AssignDataPoliciesToTeamFromDataPolicyNameList(teamModel, defaultTeamToApply.DataPolicies, updatedById);

            if (newTeam)
            {
                logger.Debug($"Persisting new team with name: '{defaultTeamToApply.Name}' to the database.");
                await teamRepository.CreateAsync(teamModel);
            }
            else
            {
                logger.Debug($"Persisting updated existing team with name: '{defaultTeamToApply.Name}' to the database.");
                await teamRepository.UpdateAsync(teamModel);
            }
        }

        private async Task AssignDataPoliciesToTeamFromDataPolicyNameList(TeamModel team, List<string> applicationDataPolicies, Guid updatedById)
        {
            // The application data policy associations for this team are going to be created or overwritten, its easier to rebuild it that apply a diff.
            team.ApplicationDataPolicies = new List<TeamApplicationDataPolicyModel>();

            if (applicationDataPolicies != null && applicationDataPolicies.Count > 0)
            {
                foreach (var applicationDataPolicy in applicationDataPolicies)
                {
                    var existingApplicationDataPolicy = await applicationDataPolicyRepository.GetByNameAsync(applicationDataPolicy);

                    if (existingApplicationDataPolicy == null)
                    {
                        throw new ItemNotFoundException($"Unable to find an application data policy with name: '{applicationDataPolicy}' when attempting to assing the application data policy to team '{team.Name}'");
                    }

                    logger.Debug($"Adding application data policy '{applicationDataPolicy}' to team: {team.Name}");

                    team.ApplicationDataPolicies.Add(new TeamApplicationDataPolicyModel
                    {
                        Team = team,
                        ApplicationDataPolicy = existingApplicationDataPolicy,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        /// <summary>
        /// Adds all the users from a list of usernames to a team. Checks if the user exists prior and only adds the user if it does.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="userNames"></param>
        /// <returns></returns>
        private async Task AssignUsersToTeamFromUserNameList(TeamModel team, List<string> userNames, Guid updatedById)
        {
            // The user associations for this team are going to be created or overwritten, its easier to rebuild it that apply a diff.
            team.UserTeams = new List<UserTeamModel>();

            if (userNames != null && userNames.Count > 0)
            {
                // Users are not allowd to be directly assigned to a parent team. Check for this!
                if (team.ChildTeams.Any())
                {
                    throw new ItemNotProcessableException($"Cannot add users directly to team '{team.Name}', as this team is a parent team.");
                }

                foreach (var userName in userNames)
                {
                    var user = await userRepository.GetByUsernameAsync(userName, false);

                    if (user == null)
                    {
                        throw new ItemNotFoundException($"Unable to find a user with Username: '{userName}' when attempting to assign the user to team {team.Name}.");
                    }

                    logger.Debug($"Adding user '{userName}' to team: {team.Name}");

                    team.UserTeams.Add(new UserTeamModel
                    {
                        Team = team,
                        User = user,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        private async Task AssignChildTeamsToTeam(SecurityContractDefaultConfigurationTeam defaultTeamToApply, TeamModel teamModel, Guid updatedById)
        {
            if (defaultTeamToApply.Teams != null && defaultTeamToApply.Teams.Count > 0)
            {
                // Check that the current team has no users! Users cannot be in a parent team, so the existence of users in a team prevents child teams being added to it.
                if (teamModel.UserTeams != null && teamModel.UserTeams.Any())
                {
                    throw new ItemNotProcessableException($"Cannot have compound teams with users in them! Team '{teamModel.Name}' already has users assigned to it, so cannot assign child teams to it!");
                }

                foreach (var teamToAdd in defaultTeamToApply.Teams)
                {
                    logger.Debug($"Attempting to add child team: '{teamToAdd}' to team '{defaultTeamToApply.Name}'.");

                    // Ensure that the role exists.
                    var existingchildTeam = await teamRepository.GetByNameAsync(teamToAdd, true);

                    if (existingchildTeam == null)
                    {
                        throw new ItemNotFoundException($"Child team '{teamToAdd}' not found when attempting to assinging it to team '{defaultTeamToApply.Name}'.");
                    }

                    // check that we are not attempting to add a compound team as a child, as this is prohibited.
                    if (existingchildTeam.ChildTeams.Any())
                    {
                        throw new ItemNotProcessableException($"Team '{existingchildTeam.Name}' already contains child teams. Cannot add it as a child team of team '{teamModel.Name}'");
                    }

                    logger.Debug($"Child team '{existingchildTeam}' exists and is being assigned to role '{defaultTeamToApply.Name}'.");
                    teamModel.ChildTeams.Add(new TeamTeamModel
                    {
                        ParentTeam = teamModel,
                        ChildTeam = existingchildTeam,
                        ChangedBy = updatedById
                    });
                }
            }
        }

        public async Task<SecurityContractDefaultConfiguration> GetDefaultConfigurationDefinitionAsync()
        {
            logger.Debug($"Retrieving default configuration security contract definitions.");

            var contractDefaultConfig = new SecurityContractDefaultConfiguration()
            {
                Name = "A3S Default configuration"
            };

            contractDefaultConfig.Applications = await RetrieveDefaultConfigApplications();
            contractDefaultConfig.Roles = await RetrieveDefaultConfigRoles();
            contractDefaultConfig.Users = await RetrieveDefaultConfigUsers();
            contractDefaultConfig.Teams = await RetrieveDefaultConfigTeams();
            contractDefaultConfig.LdapAuthenticationModes = await RetrieveDefaultConfigLdapAuthenticationModes();

            return contractDefaultConfig;
        }

        private async Task<List<SecurityContractDefaultConfigurationApplication>> RetrieveDefaultConfigApplications()
        {
            var contractAppList = new List<SecurityContractDefaultConfigurationApplication>();
            List<ApplicationModel> applications = await applicationRepository.GetListAsync();

            foreach (var application in applications.OrderBy(o => o.SysPeriod.LowerBound))
            {
                logger.Debug($"Retrieving default configuration contract definition for Application [{application.Name}].");

                var contractApplication = new SecurityContractDefaultConfigurationApplication()
                {
                    Name = application.Name,
                    Functions = new List<SecurityContractDefaultConfigurationFunction>()
                };

                foreach (var function in application.Functions.OrderBy(o => o.SysPeriod.LowerBound))
                {
                    logger.Debug($"Retrieving default configuration contract definition for Function [{function.Name}].");

                    var contractFunction = new SecurityContractDefaultConfigurationFunction()
                    {
                        Name = function.Name,
                        Description = function.Description,
                        Permissions = new List<string>()
                    };

                    foreach (var permission in function.FunctionPermissions.OrderBy(o => o.Permission.SysPeriod.LowerBound))
                        contractFunction.Permissions.Add(permission.Permission.Name);

                    contractApplication.Functions.Add(contractFunction);
                }

                contractAppList.Add(contractApplication);
            }

            return contractAppList;
        }

        private async Task<List<SecurityContractDefaultConfigurationRole>> RetrieveDefaultConfigRoles()
        {
            var contractRoleList = new List<SecurityContractDefaultConfigurationRole>();
            List<RoleModel> roles = await roleRepository.GetListAsync();

            // First retrieve simple roles
            foreach (var role in roles.Where(x => x.ChildRoles == null || x.ChildRoles.Count == 0).OrderBy(o => o.SysPeriod.LowerBound))
                contractRoleList.Add(RetrieveIndividualDefaultConfigRole(role));

            // Next retrieve complex roles
            foreach (var role in roles.Where(x => x.ChildRoles != null && x.ChildRoles.Count > 0).OrderBy(o => o.SysPeriod.LowerBound))
                contractRoleList.Add(RetrieveIndividualDefaultConfigRole(role));

            return contractRoleList;
        }

        private SecurityContractDefaultConfigurationRole RetrieveIndividualDefaultConfigRole(RoleModel role)
        {
            logger.Debug($"Retrieving default configuration contract definition for Role [{role.Name}].");

            var contractRole = new SecurityContractDefaultConfigurationRole()
            {
                Name = role.Name,
                Description = role.Description,
                Functions = new List<string>(),
                Roles = new List<string>()
            };

            foreach (var function in role.RoleFunctions.OrderBy(o => o.Function.SysPeriod.LowerBound))
                contractRole.Functions.Add(function.Function.Name);

            foreach (var childRole in role.ChildRoles.OrderBy(o => o.ChildRole.SysPeriod.LowerBound))
                contractRole.Roles.Add(childRole.ChildRole.Name);

            return contractRole;
        }

        private async Task<List<SecurityContractDefaultConfigurationUser>> RetrieveDefaultConfigUsers()
        {
            var contractUserList = new List<SecurityContractDefaultConfigurationUser>();
            List<UserModel> users = await userRepository.GetListAsync();

            foreach (var user in users.OrderBy(o => o.SysPeriod.LowerBound))
            {
                logger.Debug($"Retrieving default configuration contract definition for User [{user.UserName}].");

                var contractUser = new SecurityContractDefaultConfigurationUser()
                {
                    Uuid = new Guid(user.Id),
                    Username = user.UserName,
                    Name = user.FirstName,
                    Surname = user.Surname,
                    Email = user.Email,
                    HashedPassword = user.PasswordHash,
                    IsDeleted = user.IsDeleted,
                    DeletedTime = user.DeletedTime,
                    LdapAuthenticationMode = user.LdapAuthenticationMode?.Name,
                    Avatar = user.Avatar == null ? null : Convert.ToBase64String(user.Avatar),
                    Roles = new List<string>()
                };

                foreach (var role in user.UserRoles.OrderBy(o => o.Role.SysPeriod.LowerBound))
                    contractUser.Roles.Add(role.Role.Name);

                contractUserList.Add(contractUser);
            }

            return contractUserList;
        }

        private async Task<List<SecurityContractDefaultConfigurationLdapAuthMode>> RetrieveDefaultConfigLdapAuthenticationModes()
        {
            var contractLdapAuthModeList = new List<SecurityContractDefaultConfigurationLdapAuthMode>();
            List<LdapAuthenticationModeModel> ldapAuthenticationModes = await ldapAuthenticationModeRepository.GetListAsync(includePassword: false);

            foreach (var ldapAuthMode in ldapAuthenticationModes.OrderBy(o => o.SysPeriod.LowerBound))
            {
                logger.Debug($"Retrieving default configuration contract definition for Ldap Auth Mode [{ldapAuthMode.Name}].");

                var contractLdapAuthMode = new SecurityContractDefaultConfigurationLdapAuthMode()
                {
                    Name = ldapAuthMode.Name,
                    HostName = ldapAuthMode.HostName,
                    Port = ldapAuthMode.Port,
                    IsLdaps = ldapAuthMode.IsLdaps,
                    Account = ldapAuthMode.Account,
                    BaseDn = ldapAuthMode.BaseDn,
                    LdapAttributes = new List<SecurityContractDefaultConfigurationLdapAttributeLink>()
                };

                foreach (var attribute in ldapAuthMode.LdapAttributes.OrderBy(o => o.SysPeriod.LowerBound))
                    contractLdapAuthMode.LdapAttributes.Add(new SecurityContractDefaultConfigurationLdapAttributeLink()
                    {
                        UserField = attribute.UserField,
                        LdapField = attribute.LdapField
                    });

                contractLdapAuthModeList.Add(contractLdapAuthMode);
            }

            return contractLdapAuthModeList;
        }
        private async Task<List<SecurityContractDefaultConfigurationTeam>> RetrieveDefaultConfigTeams()
        {
            var contractTeamList = new List<SecurityContractDefaultConfigurationTeam>();
            List<TeamModel> teams = await teamRepository.GetListAsync();

            // Retrieve simple teams first
            foreach (var team in teams.Where(x => x.ChildTeams == null || x.ChildTeams.Count == 0).OrderBy(o => o.SysPeriod.LowerBound))
                contractTeamList.Add(RetrieveIndividualDefaultConfigTeam(team));

            // Retrieve compound teams next
            foreach (var team in teams.Where(x => x.ChildTeams != null && x.ChildTeams.Count > 0).OrderBy(o => o.SysPeriod.LowerBound))
                contractTeamList.Add(RetrieveIndividualDefaultConfigTeam(team));

            return contractTeamList;
        }

        private SecurityContractDefaultConfigurationTeam RetrieveIndividualDefaultConfigTeam(TeamModel team)
        {
            logger.Debug($"Retrieving default configuration contract definition for Team [{team.Name}].");

            var contractTeam = new SecurityContractDefaultConfigurationTeam()
            {
                Uuid = team.Id,
                Name = team.Name,
                Description = team.Description,
                Users = new List<string>(),
                Teams = new List<string>(),
                DataPolicies = new List<string>()
            };

            foreach (var teamDataPolicy in team.ApplicationDataPolicies.OrderBy(o => o.SysPeriod.LowerBound))
                contractTeam.DataPolicies.Add(teamDataPolicy.ApplicationDataPolicy.Name);

            foreach (var user in team.UserTeams.OrderBy(o => o.User.SysPeriod.LowerBound))
                contractTeam.Users.Add(user.User.UserName);

            foreach (var childTeam in team.ChildTeams.OrderBy(o => o.ChildTeam.SysPeriod.LowerBound))
                contractTeam.Teams.Add(childTeam.ChildTeam.Name);

            return contractTeam;
        }

        public void InitSharedTransaction()
        {
            applicationRepository.InitSharedTransaction();
            roleRepository.InitSharedTransaction();
            functionRepository.InitSharedTransaction();
            userRepository.InitSharedTransaction();
            teamRepository.InitSharedTransaction();
            applicationDataPolicyRepository.InitSharedTransaction();
            ldapAuthenticationModeRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            applicationRepository.CommitTransaction();
            roleRepository.CommitTransaction();
            functionRepository.CommitTransaction();
            userRepository.CommitTransaction();
            teamRepository.CommitTransaction();
            applicationDataPolicyRepository.CommitTransaction();
            ldapAuthenticationModeRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            applicationRepository.RollbackTransaction();
            roleRepository.RollbackTransaction();
            functionRepository.RollbackTransaction();
            userRepository.RollbackTransaction();
            teamRepository.RollbackTransaction();
            applicationDataPolicyRepository.RollbackTransaction();
            ldapAuthenticationModeRepository.RollbackTransaction();
        }
    }
}

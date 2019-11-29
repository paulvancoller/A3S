/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class SecurityContractDefaultConfigurationService_Tests
    {
        ISecurityContractDefaultConfigurationService securityContractDefaultConfigurationService;

        public SecurityContractDefaultConfigurationService_Tests()
        {
          
        }

        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_WithValidNoSectionsInput_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test without sub sections"
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_ApplicationFoundButNoFunctionsInInput_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            applicationRepository.GetByNameAsync(Arg.Any<string>()).Returns(new ApplicationModel {
                Name = "Test Application Model"
            });

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test with application section",
                Applications = new List <SecurityContractDefaultConfigurationApplication>{
                    new SecurityContractDefaultConfigurationApplication
                    {
                        Name = "Test Default Application"
                    }
                }
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_RolesWithFunctionsInInput_FunctionsAndRoleExistInDB_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            // The service will check for existing roles within the database, we want a result to test existing role.
            roleRepository.GetByNameAsync(Arg.Any<string>()).Returns(new RoleModel
            {
                Name = "Test Role Model",
                RoleFunctions = new List<RoleFunctionModel>
                {
                    new RoleFunctionModel
                    {
                        Function = new FunctionModel
                        {
                            Name = "Function in role test"
                        } 
                    }
                }
                
            });

            // Owing to the fact that all roles are found, the service will update model. Ensure that this can happen.
            roleRepository.UpdateAsync(Arg.Any<RoleModel>()).Returns(new RoleModel
            {
                Name = "Test Role Model",
                RoleFunctions = new List<RoleFunctionModel>
                {
                    new RoleFunctionModel
                    {
                        Function = new FunctionModel
                        {
                            Name = "Function in role test"
                        }
                    }
                }

            });

            // The service will also check that the functions attached to the roles exist. Ensure the repo returns one to test that flow.
            functionRepository.GetByNameAsync(Arg.Any<string>()).Returns(new FunctionModel
            {
                Name = "Test Role Model",
            });

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test with application section",
                Roles = new List<SecurityContractDefaultConfigurationRole>
                {
                    new SecurityContractDefaultConfigurationRole
                    {
                        Name = "Test security contract role",
                        Functions = new List<string>
                        {
                            "Test function in contract"
                        }
                    }
                }
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_RolesWithFunctionsInInput_FunctionsExistInDBRoleIsNew_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            // The service will check for existing roles within the database, we a null here to test new role creation.
            roleRepository.GetByNameAsync(Arg.Any<string>()).Returns((RoleModel)null);

            // Owing to the fact that all roles are found, the service will update model. Ensure that this can happen.
            roleRepository.CreateAsync(Arg.Any<RoleModel>()).Returns(new RoleModel
            {
                Name = "Test Role Model",
                RoleFunctions = new List<RoleFunctionModel>
                {
                    new RoleFunctionModel
                    {
                        Function = new FunctionModel
                        {
                            Name = "Function in role test"
                        }
                    }
                }

            });

            // The service will also check that the functions attached to the roles exist. Ensure the repo returns one to test that flow.
            functionRepository.GetByNameAsync(Arg.Any<string>()).Returns(new FunctionModel
            {
                Name = "Test Role Model",
            });

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test with application section",
                Roles = new List<SecurityContractDefaultConfigurationRole>
                {
                    new SecurityContractDefaultConfigurationRole
                    {
                        Name = "Test security contract role",
                        Functions = new List<string>
                        {
                            "Test function in contract"
                        }
                    }
                }
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }


        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_TeamsWithUsersInInput_UsersAndTeamsExistInDB_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            // The service will check for existing teams within the database, we a null here to test new role creation.
            teamRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(new TeamModel {
                Name = "Test existing team model"
            });

            // Owing to the fact that all roles are found, the service will update the team model. Ensure that this can happen.
            teamRepository.UpdateAsync(Arg.Any<TeamModel>()).Returns(new TeamModel
            {
                Name = "Test existing team model"
            });

            // The service will also check that the users attached to the teams exist. Ensure the repo returns one to test that flow.
            userRepository.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(new UserModel
            {
                UserName = "Test User Model",
            });

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test with application section",
                Teams = new List<SecurityContractDefaultConfigurationTeam>
                {
                    new SecurityContractDefaultConfigurationTeam
                    {
                        Name = "Test Team",
                        Users = new List<string>
                        {
                            "test user name"
                        }
                    }
                }
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }

        [Fact]
        public async Task ApplySecurityContractDefaultsAsync_UsersWithRolesInInput_UsersAndRolesExistInDB_NoExceptionsThrown()
        {
            // Set up the required security contract defaults service with all its dependencies mocked.
            var roleRepository = Substitute.For<IRoleRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var functionRepository = Substitute.For<IFunctionRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var ldapAuthenticationModeRepository = Substitute.For<ILdapAuthenticationModeRepository>();

            // The service will check for existing roles within the database, we want a result to test existing role.
            roleRepository.GetByNameAsync(Arg.Any<string>()).Returns(new RoleModel
            {
                Name = "Test Role Model",
                RoleFunctions = new List<RoleFunctionModel>
                {
                    new RoleFunctionModel
                    {
                        Function = new FunctionModel
                        {
                            Name = "Function in role test"
                        }
                    }
                }

            });

            // The service will also check that the users exist. Ensure the repo returns one to test that flow.
            userRepository.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(new UserModel
            {
                UserName = "Test User Model",
                Email = "test@email.com"
            });

            // The service should attempt an update on the users, ensure this is possible.
            userRepository.UpdateAsync(Arg.Any<UserModel>()).Returns(new UserModel
            {
                UserName = "Test User Model",
                Email = "test@email.com"
            });

            securityContractDefaultConfigurationService = new SecurityContractDefaultConfigurationService(roleRepository, userRepository, functionRepository, teamRepository, applicationRepository, applicationDataPolicyRepository, ldapAuthenticationModeRepository);

            // Create a security contract with a default configuration section, but dont set any of the sub components.
            var securityContract = new SecurityContract();
            securityContract.DefaultConfigurations = new List<SecurityContractDefaultConfiguration>{new SecurityContractDefaultConfiguration
            {
                Name = "Test with application section",
                Users = new List<SecurityContractDefaultConfigurationUser>
                {
                    new SecurityContractDefaultConfigurationUser
                    {
                        Username = "Test User",
                        Email = "test@email.com"
                    }
                }
            }};

            try
            {
                await securityContractDefaultConfigurationService.ApplyDefaultConfigurationDefinitionAsync(securityContract.DefaultConfigurations.First(), Guid.NewGuid(), false, new List<string>());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected Exception: '{e.Message}' thrown when applying security contract");
            }
        }
    }
}

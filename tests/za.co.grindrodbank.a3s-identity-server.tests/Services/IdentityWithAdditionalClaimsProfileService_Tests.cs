/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.tests.Fakes;
using za.co.grindrodbank.a3sidentityserver.Services;

namespace za.co.grindrodbank.a3sidentityserver.tests.Services
{
    public class IdentityWithAdditionalClaimsProfileService_Tests
    {
        private readonly CustomUserManagerFake fakeUserManager;
        private readonly IUserClaimsPrincipalFactory<UserModel> mockUserClaimsPrincipalFactory;
        private readonly ILogger<IdentityWithAdditionalClaimsProfileService> mockLogger;
        private readonly IConfiguration mockConfiguration;
        private readonly IProfileRepository mockProfileRepository;
        private readonly IApplicationDataPolicyRepository mockApplicationDataPolicyRepository;
        private readonly IPermissionRepository mockPermissionRepository;
        private readonly ITeamRepository mockTeamRepository;
        private readonly ProfileDataRequestContext profileDataRequestContext;
        private readonly IsActiveContext isActiveContext;
        private readonly UserModel userModel;

        public IdentityWithAdditionalClaimsProfileService_Tests()
        {
            var mockOptionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            var mockPasswordHasher = Substitute.For<IPasswordHasher<UserModel>>();
            var mockUserValidators = Substitute.For<IEnumerable<IUserValidator<UserModel>>>();
            var mockPasswordValidators = Substitute.For<IEnumerable<IPasswordValidator<UserModel>>>();
            var mockKeyNormalizer = Substitute.For<ILookupNormalizer>();
            var mockErrors = Substitute.For<IdentityErrorDescriber>();
            var mockServices = Substitute.For<IServiceProvider>();
            var mockUserLogger = Substitute.For<ILogger<UserManager<UserModel>>>();
            var fakeA3SContext = new A3SContextFake(new Microsoft.EntityFrameworkCore.DbContextOptions<A3SContext>());

            mockConfiguration = Substitute.For<IConfiguration>();
            var fakesCustomUserStore = new CustomUserStoreFake(fakeA3SContext, mockConfiguration);

            fakeUserManager = new CustomUserManagerFake(fakesCustomUserStore, mockOptionsAccessor, mockPasswordHasher, mockUserValidators, mockPasswordValidators, mockKeyNormalizer,
                mockErrors, mockServices, mockUserLogger);

            mockUserClaimsPrincipalFactory = Substitute.For<IUserClaimsPrincipalFactory<UserModel>>();
            mockLogger = Substitute.For<ILogger<IdentityWithAdditionalClaimsProfileService>>();
            mockProfileRepository = Substitute.For<IProfileRepository>();
            mockApplicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            mockPermissionRepository = Substitute.For<IPermissionRepository>();
            mockTeamRepository = Substitute.For<ITeamRepository>();

            var id = Guid.NewGuid().ToString();
            profileDataRequestContext = new ProfileDataRequestContext()
            {
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                Client = new Client()
                    {
                        ClientName = "mockClient"
                    },
                RequestedClaimTypes = new List<string>()
                    {
                        ClaimTypes.Name,
                        ClaimTypes.NameIdentifier,
                        "sub",
                        "custom-claim"
                    },
                Caller = "mockCaller"
            };

            isActiveContext = new IsActiveContext(
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                                new Claim(ClaimTypes.Name, "example name"),
                                new Claim(ClaimTypes.NameIdentifier, id),
                                new Claim("sub", id),
                                new Claim("custom-claim", "example claim value"),
                }, "mock")),
                new Client()
                {
                    ClientName = "mockClient"
                },
                "mockCaller");

            userModel = new UserModel()
            {
                UserName = "username",
                Id = id,
                Email = "temp@local",
                FirstName = "Temp",
                Surname = "User"
            };
        }

        [Fact]
        public async Task GetProfileDataAsync_NoProfileNameSpecified_ClaimsMapForUserGenerated()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);
            fakeUserManager.SetUserModel(userModel);
            mockUserClaimsPrincipalFactory.CreateAsync(userModel).Returns(profileDataRequestContext.Subject);
            mockPermissionRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<PermissionModel>()
            {
                new PermissionModel()
                {
                    Name = "Permission 1"
                },
                new PermissionModel()
                {
                    Name = "Permission 2"
                }
            });
            mockApplicationDataPolicyRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<ApplicationDataPolicyModel>()
            {
                new ApplicationDataPolicyModel()
                {
                    Name = "DP 1"
                }
            });
            mockTeamRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<TeamModel>()
            {
                new TeamModel()
                {
                    Name = "Name 1"
                },
                new TeamModel()
                {
                    Name = "Name 2"
                }
            });

            // Act
            await identityWithAdditionalClaimsProfileService.GetProfileDataAsync(profileDataRequestContext);

            // Assert
            Assert.True(profileDataRequestContext.IssuedClaims.Count > 0, "Issued claims must be greater than 0.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 1"), "Permission 1 claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 2"), "Permission 2 claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == IdentityServerConstants.StandardScopes.Email && x.Value == userModel.Email), "Email claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "username" && x.Value == userModel.UserName), "Username claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "given_name" && x.Value == userModel.FirstName), "Given Name claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "family_name" && x.Value == userModel.Surname), "Family Name claim must be present and correct.");
        }

        [Fact]
        public async Task GetProfileDataAsync_NoProfileNameSpecifiedAndNoSubject_ExceptionThrown()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);

            mockUserClaimsPrincipalFactory.CreateAsync(userModel).Returns(profileDataRequestContext.Subject);
            mockPermissionRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<PermissionModel>()
            {
                new PermissionModel()
                {
                    Name = "Permission 1"
                },
                new PermissionModel()
                {
                    Name = "Permission 2"
                }
            });
            mockApplicationDataPolicyRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<ApplicationDataPolicyModel>()
            {
                new ApplicationDataPolicyModel()
                {
                    Name = "DP 1"
                }
            });
            mockTeamRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<TeamModel>()
            {
                new TeamModel()
                {
                    Name = "Name 1"
                },
                new TeamModel()
                {
                    Name = "Name 2"
                }
            });

            profileDataRequestContext.Subject = null;

            // Act
            try
            {
                await identityWithAdditionalClaimsProfileService.GetProfileDataAsync(profileDataRequestContext);
                Assert.True(false, "No subject specified MUST throw exception.");
            }
            catch
            {
                Assert.True(true, "No subject specified MUST throw exception.");
            }
        }

        [Fact]
        public async Task GetProfileDataAsync_WithProfileNameSpecified_ClaimsMapForUserProfileGenerated()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);
            fakeUserManager.SetUserModel(userModel);
            mockUserClaimsPrincipalFactory.CreateAsync(userModel).Returns(profileDataRequestContext.Subject);
            mockPermissionRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<PermissionModel>()
            {
                new PermissionModel()
                {
                    Name = "Permission 1"
                },
                new PermissionModel()
                {
                    Name = "Permission 2"
                }
            });
            mockApplicationDataPolicyRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<ApplicationDataPolicyModel>()
            {
                new ApplicationDataPolicyModel()
                {
                    Name = "DP 1"
                }
            });
            mockTeamRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<TeamModel>()
            {
                new TeamModel()
                {
                    Name = "Name 1"
                },
                new TeamModel()
                {
                    Name = "Name 2"
                }
            });
            mockProfileRepository.GetByNameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(new ProfileModel()
            {
                Id = Guid.NewGuid(),
                Name = "mock_profile"
            });
            profileDataRequestContext.ValidatedRequest = new IdentityServer4.Validation.ValidatedRequest()
            {
                Raw = new System.Collections.Specialized.NameValueCollection()
            };
            profileDataRequestContext.ValidatedRequest.Raw.Add("profile_name", "mock_profile");

            // Act
            await identityWithAdditionalClaimsProfileService.GetProfileDataAsync(profileDataRequestContext);

            // Assert
            Assert.True(profileDataRequestContext.IssuedClaims.Count > 0, "Issued claims must be greater than 0.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 1"), "Permission 1 claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 2"), "Permission 2 claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == IdentityServerConstants.StandardScopes.Email && x.Value == userModel.Email), "Email claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "username" && x.Value == userModel.UserName), "Username claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "given_name" && x.Value == userModel.FirstName), "Given Name claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "family_name" && x.Value == userModel.Surname), "Family Name claim must be present and correct.");
        }

        [Fact]
        public async Task GetProfileDataAsync_WithProfileNameSpecifiedButNoProfileFound_NoClaimsMapForUserProfileGenerated()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);
            fakeUserManager.SetUserModel(userModel);
            mockUserClaimsPrincipalFactory.CreateAsync(userModel).Returns(profileDataRequestContext.Subject);
            mockPermissionRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<PermissionModel>()
            {
                new PermissionModel()
                {
                    Name = "Permission 1"
                },
                new PermissionModel()
                {
                    Name = "Permission 2"
                }
            });
            mockApplicationDataPolicyRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<ApplicationDataPolicyModel>()
            {
                new ApplicationDataPolicyModel()
                {
                    Name = "DP 1"
                }
            });
            mockTeamRepository.GetListAsync(Arg.Any<Guid>()).Returns(new List<TeamModel>()
            {
                new TeamModel()
                {
                    Name = "Name 1"
                },
                new TeamModel()
                {
                    Name = "Name 2"
                }
            });
            profileDataRequestContext.ValidatedRequest = new IdentityServer4.Validation.ValidatedRequest()
            {
                Raw = new System.Collections.Specialized.NameValueCollection()
            };
            profileDataRequestContext.ValidatedRequest.Raw.Add("profile_name", "mock_profile");

            // Act
            await identityWithAdditionalClaimsProfileService.GetProfileDataAsync(profileDataRequestContext);

            // Assert
            Assert.True(profileDataRequestContext.IssuedClaims.Count > 0, "Issued claims must be greater than 0.");
            Assert.False(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 1"), "Permission 1 claim must NOT be present and correct.");
            Assert.False(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "permission" && x.Value == "Permission 2"), "Permission 2 claim must NOT be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == IdentityServerConstants.StandardScopes.Email && x.Value == userModel.Email), "Email claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "username" && x.Value == userModel.UserName), "Username claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "given_name" && x.Value == userModel.FirstName), "Given Name claim must be present and correct.");
            Assert.True(profileDataRequestContext.IssuedClaims.Exists(x => x.Type == "family_name" && x.Value == userModel.Surname), "Family Name claim must be present and correct.");
        }

        [Fact]
        public async Task IsActiveAsync_WithUser_IsActive()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);

            fakeUserManager.SetUserModel(userModel);

            // Act
            await identityWithAdditionalClaimsProfileService.IsActiveAsync(isActiveContext);

            // Assert
            Assert.True(isActiveContext.IsActive, "Context must be active.");
        }

        [Fact]
        public async Task IsActiveAsync_WithoutUser_IsNotActive()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, mockProfileRepository,
                mockApplicationDataPolicyRepository, mockPermissionRepository, mockTeamRepository);

            // Act
            await identityWithAdditionalClaimsProfileService.IsActiveAsync(isActiveContext);

            // Assert
            Assert.False(isActiveContext.IsActive, "Context must be inactive.");
        }
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using za.co.grindrodbank.a3s.tests.Fakes;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.tests.Quickstart.TermsOfService
{
    public class TermsOfServiceController_Tests
    {
        private readonly CustomUserManagerFake fakeUserManager;
        private readonly IConfiguration mockConfiguration;
        private readonly UserModel userModel;
        private readonly ITermsOfServiceRepository termsOfServiceRepository;
        private readonly IClientStore mockClientStore;
        private readonly IEventService mockEventService;
        private readonly IIdentityServerInteractionService mockIdentityServerInteractionService;
        private readonly IArchiveHelper mockArchiveHelper;
        private readonly TermsOfServiceModel termsOfServiceModel;
        private readonly AuthorizationRequest authorizationRequest;
        private readonly Client client;
        private readonly IUserRepository mockUserRepository;
        private readonly IAuthenticationSchemeProvider mockAuthenticationSchemeProvider;
        private readonly CustomSignInManagerFake<UserModel> fakeSignInManager;

        private const string RETURN_URL = "/returnUrl";

        public TermsOfServiceController_Tests()
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

            var mockContextAccessor = Substitute.For<IHttpContextAccessor>();
            var mocClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<UserModel>>();
            var mockSignInLogger = Substitute.For<ILogger<SignInManager<UserModel>>>();
            mockUserRepository = Substitute.For<IUserRepository>();
            var mockLdapAuthenticationModeRepository = Substitute.For<LdapAuthenticationModeRepository>(fakeA3SContext, mockConfiguration);
            var mockLdapConnectionService = Substitute.For<LdapConnectionService>(mockLdapAuthenticationModeRepository, mockUserRepository);
            mockAuthenticationSchemeProvider = Substitute.For<IAuthenticationSchemeProvider>();

            fakeSignInManager = new CustomSignInManagerFake<UserModel>(fakeUserManager, mockContextAccessor, mocClaimsFactory, mockOptionsAccessor, mockSignInLogger, fakeA3SContext,
                mockAuthenticationSchemeProvider, mockLdapAuthenticationModeRepository, mockLdapConnectionService);

            termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            mockClientStore = Substitute.For<IClientStore>();
            mockEventService = Substitute.For<IEventService>();
            mockIdentityServerInteractionService = Substitute.For<IIdentityServerInteractionService>();
            mockArchiveHelper = Substitute.For<IArchiveHelper>();

            userModel = new UserModel()
            {
                UserName = "username",
                Id = Guid.NewGuid().ToString()
            };

            termsOfServiceModel = new TermsOfServiceModel()
            {
                AgreementName = "Test agreement",
                Version = "2019.1"
            };

            // Prepare controller contexts
            authorizationRequest = new AuthorizationRequest()
            {
                IdP = "testIdp",
                ClientId = "clientId1",
                LoginHint = "LoginHint"
            };

            client = new Client()
            {
                EnableLocalLogin = true
            };
        }

        [Fact]
        public async Task Index_ExecutedWithOutstandingAgreements_ViewResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>())
                .Returns(new List<Guid>()
                {
                    Guid.NewGuid(),
                    Guid.NewGuid()
                });

            termsOfServiceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(termsOfServiceModel);

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as TermsOfServiceViewModel;
            Assert.True(model.AgreementName == termsOfServiceModel.AgreementName, $"Model field AgreementName '{model.AgreementName}' should be '{termsOfServiceModel.AgreementName}'.");
            Assert.True(model.AgreementCount == 2, $"Model field AgreementCount '{model.AgreementCount}' should be '2'.");
        }

        [Fact]
        public async Task Index_ExecutedWithOutstandingAgreementsButUnfindableAgreementGuid_ThrowsItemNotFoundException()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>())
                .Returns(new List<Guid>()
                {
                    Guid.NewGuid(),
                    Guid.NewGuid()
                });

            // Act
            Exception caughtException = null;

            try
            {
                var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, "Caught exception must be sItemNotFoundException.");
        }

        [Fact]
        public async Task Index_ExecutedWithNullUser_ThrowsException()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(null);

            // Act
            Exception caughtException = null;

            try
            {
                var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is AuthenticationException, "Invalid login data must throw an AuthenticationException.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreementsWith2FAEnabled_RedirectResultToLoginSuccessfulReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "true";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);

            // Assert
            var viewResult = actionResult as RedirectToActionResult;
            Assert.NotNull(viewResult);
            Assert.True(viewResult.ActionName == "LoginSuccessful", "Redirect action must be 'LoginSuccessful'.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreements_RedirectResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "false";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            mockIdentityServerInteractionService.GetAuthorizationContextAsync(Arg.Any<string>()).Returns(authorizationRequest);

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);

            // Assert
            var viewResult = actionResult as RedirectResult;
            Assert.NotNull(viewResult);
            Assert.True(viewResult.Url == RETURN_URL, $"Redirect Url must be '{RETURN_URL}'.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreementsAndIsPkce_ViewResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "false";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            mockIdentityServerInteractionService.GetAuthorizationContextAsync(Arg.Any<string>()).Returns(authorizationRequest);

            client.RequirePkce = true;
            mockClientStore.FindEnabledClientByIdAsync(Arg.Any<string>()).Returns(client);

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(viewResult.ViewName == "Redirect", $"ViewName must be 'Redirect'.");

            var model = viewResult.Model as RedirectViewModel;
            Assert.True(model.RedirectUrl == RETURN_URL, $"Redirect Url must be '{RETURN_URL}'.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreementsAndNoContextWithLocalRedirectUrl_RedirectResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "false";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.IsLocalUrl(Arg.Any<string>()).Returns(true);
            termsOfServiceController.Url = urlHelper;

            // Act
            var actionResult = await termsOfServiceController.Index(RETURN_URL, 0);

            // Assert
            var viewResult = actionResult as RedirectResult;
            Assert.NotNull(viewResult);
            Assert.True(viewResult.Url == RETURN_URL, $"Redirect Url must be '{RETURN_URL}'.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreementsAndNoContextWithEmptyRedirectUrl_RedirectResultReturned()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "false";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.IsLocalUrl(Arg.Any<string>()).Returns(false);
            termsOfServiceController.Url = urlHelper;

            // Act
            var actionResult = await termsOfServiceController.Index(returnUrl: string.Empty, 0);

            // Assert
            var viewResult = actionResult as RedirectResult;
            Assert.NotNull(viewResult);
            Assert.True(viewResult.Url == "~/", $"Redirect Url must be '~/'.");
        }

        [Fact]
        public async Task Index_ExecutedWithNoOutstandingAgreementsAndNoContextWithNonLocalRedirectUrl_ThrowsException()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            using var termsOfServiceController = new TermsOfServiceController(fakeUserManager, termsOfServiceRepository, mockConfiguration, mockIdentityServerInteractionService,
                mockEventService, mockClientStore, mockArchiveHelper, fakeSignInManager)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "example name"),
                            new Claim(ClaimTypes.NameIdentifier, id),
                            new Claim("sub", id),
                            new Claim("custom-claim", "example claim value"),
                        }, "mock")),
                    }
                }
            };

            fakeUserManager.SetUserModel(userModel);
            termsOfServiceRepository.GetAllOutstandingAgreementsByUserAsync(Arg.Any<Guid>()).Returns(new List<Guid>());

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();

            var config = builder.Build();
            config.GetSection("TwoFactorAuthentication")["OrganizationEnforced"] = "false";
            config.GetSection("TwoFactorAuthentication")["AuthenticatorEnabled"] = "false";
            mockConfiguration.GetSection("TwoFactorAuthentication").Returns(config.GetSection("TwoFactorAuthentication"));

            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.IsLocalUrl(Arg.Any<string>()).Returns(false);
            termsOfServiceController.Url = urlHelper;

            // Act
            try
            {
                var actionResult = await termsOfServiceController.Index(returnUrl: "http://www.test.me/redirect", 0);

                // Assert
                Assert.True(false, "Non-local redirect with no context must throw Exception.");
            }
            catch
            {
                // Assert
                Assert.True(true, "Non-local redirect with no context must throw Exception.");
            }
        }
    }
}

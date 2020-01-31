using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly A3SContextFake a3SContextFake;
        private readonly IUserClaimsPrincipalFactory<UserModel> mockUserClaimsPrincipalFactory;
        private readonly ILogger<IdentityWithAdditionalClaimsProfileService> mockLogger;
        private readonly IConfiguration mockConfiguration;
        private readonly IProfileRepository mockProfileRepository;
        private ProfileDataRequestContext profileDataRequestContext;

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

            a3SContextFake = new A3SContextFake(new Microsoft.EntityFrameworkCore.DbContextOptions<A3SContext>());
            mockUserClaimsPrincipalFactory = Substitute.For<IUserClaimsPrincipalFactory<UserModel>>();
            mockLogger = Substitute.For<ILogger<IdentityWithAdditionalClaimsProfileService>>();
            mockProfileRepository = Substitute.For<IProfileRepository>();

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
        }

        [Fact]
        public async Task GetProfileDataAsync_Executed_GeneratesClaims()
        {
            // Assert
            var identityWithAdditionalClaimsProfileService = new IdentityWithAdditionalClaimsProfileService(fakeUserManager, mockUserClaimsPrincipalFactory, mockLogger, a3SContextFake, mockProfileRepository);

            // Act
            await identityWithAdditionalClaimsProfileService.GetProfileDataAsync(profileDataRequestContext);

            // Assert
            Assert.True(profileDataRequestContext.IssuedClaims.Count > 0, "Issued claims must be greater than 0");
        }

    }
}

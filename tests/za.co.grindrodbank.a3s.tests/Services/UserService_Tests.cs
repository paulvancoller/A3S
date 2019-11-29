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
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class UserService_Tests
    {
        IMapper mapper;
        Guid userGuid;
        Guid roleGuid;
        Guid teamGuid;

        UserModel mockedUserModel;

        public UserService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserResourceUserModelProfile());
                cfg.AddProfile(new UserSubmitResourceUserModelProfiles());
                cfg.AddProfile(new TeamResourceTeamModelProfile());
                cfg.AddProfile(new RoleResourceRoleModelProfile());
            });

            mapper = config.CreateMapper();

            // Configure a mock user model.
            userGuid = Guid.NewGuid();
            teamGuid = Guid.NewGuid();
            roleGuid = Guid.NewGuid();

            mockedUserModel = new UserModel();
            mockedUserModel.Email = "testuser@test.com";
            mockedUserModel.UserName = "test-user";
            mockedUserModel.NormalizedUserName = "test-user";

            mockedUserModel.UserRoles = new List<UserRoleModel>
            {
                new UserRoleModel
                {
                    Role = new RoleModel
                    {
                        Id = roleGuid,
                        Name = "Test Role"
                    },
                    User = mockedUserModel
                }
            };

            mockedUserModel.UserTeams = new List<UserTeamModel>
            {
                new UserTeamModel
                {
                    Team = new TeamModel
                    {
                        Name = "Test Team",
                        Id = teamGuid
                    },
                    User = mockedUserModel
                }
            };
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnUserResource()
        {
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var userRepository = Substitute.For<IUserRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            userRepository.GetByIdAsync(userGuid, true).Returns(mockedUserModel);

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);
            var userResource = await userService.GetByIdAsync(userGuid, true);

            Assert.True(userResource.Email == "testuser@test.com", $"Expected User Resource Email: '{userResource.Email}' does not match the expected value: 'testuser@test.com'");
            Assert.True(userResource.Roles.First().Name == "Test Role", $"Expected User Resource Role Name: '{userResource.Roles.First().Name}' does not match the expected value: 'Test Role'");
            Assert.True(userResource.Teams.First().Name == "Test Team", $"Expected User Resource Team Name: '{userResource.Teams.First().Name}' does not match the expected value: 'Test Team'");
        }

        [Fact]
        public async Task UpdateUser_GivenUserSubmit_ReturnsUserResource()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var guid = Guid.NewGuid();
            var userModelMock = new UserModel
            {
                UserName = "Test Name",
                Id = guid.ToString(),
                FirstName = "Test User Firstname",
                Surname = "Test User Surname",
                Email = "Test User Email"
            };

            var userSubmit = new UserSubmit()
            {
                Uuid = guid,
                Username = userModelMock.UserName,
                Name = userModelMock.FirstName,
                Surname = userModelMock.Surname,
                Email = userModelMock.Email
            };

            userRepository.UpdateAsync(Arg.Any<UserModel>()).Returns(userModelMock);
            userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(userModelMock);

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);
            var userResource = await userService.UpdateAsync(userSubmit, Guid.NewGuid());

            Assert.True(userResource.Uuid.ToString() == userModelMock.Id, $"User resource Id: '{userResource.Uuid}' does not match expected value: '{userModelMock.Id}'");
            Assert.True(userResource.Name == userModelMock.FirstName, $"User resource name: '{userResource.Name}' does not match expected value: '{userModelMock.FirstName}'");
            Assert.True(userResource.Surname == userModelMock.Surname, $"User resource surname: '{userResource.Surname}' does not match expected value: '{userModelMock.Surname}'");
            Assert.True(userResource.Username == userModelMock.UserName, $"User resource username: '{userResource.Username}' does not match expected value: '{userModelMock.UserName}'");
            Assert.True(userResource.Email == userModelMock.Email, $"User resource email: '{userResource.Email}' does not match expected value: '{userModelMock.Email}'");
        }

        [Fact]
        public async Task CreateUser_GivenUserCreate_ReturnsUserResource()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var userModelMock = new UserModel
            {
                UserName = "Test Name",
                FirstName = "Test User Firstname",
                Surname = "Test User Surname",
                Email = "Test User Email",
                PhoneNumber = "Test User Phone",
                LdapAuthenticationModeId = null
            };

            var userCreate = new UserSubmit()
            {
                Username = userModelMock.UserName,
                Password = "Password1#",
                Name = userModelMock.FirstName,
                Surname = userModelMock.Surname,
                Email = userModelMock.Email,
                PhoneNumber = userModelMock.PhoneNumber,
                LdapAuthenticationModeId = userModelMock.LdapAuthenticationModeId
            };

            userRepository.CreateAsync(Arg.Any<UserModel>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(userModelMock);
            userRepository.UpdateAsync(Arg.Any<UserModel>()).Returns(userModelMock);
            userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(userModelMock);

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            var userResource = await userService.CreateAsync(userCreate, Guid.NewGuid());

            Assert.True(userResource.Name == userModelMock.FirstName, $"User resource name: '{userResource.Name}' does not match expected value: '{userModelMock.FirstName}'");
            Assert.True(userResource.Surname == userModelMock.Surname, $"User resource surname: '{userResource.Surname}' does not match expected value: '{userModelMock.Surname}'");
            Assert.True(userResource.Username == userModelMock.UserName, $"User resource username: '{userResource.Username}' does not match expected value: '{userModelMock.UserName}'");
            Assert.True(userResource.Email == userModelMock.Email, $"User resource email: '{userResource.Email}' does not match expected value: '{userModelMock.Email}'");
            Assert.True(userResource.PhoneNumber == userModelMock.PhoneNumber, $"User resource PhoneNumber: '{userResource.PhoneNumber}' does not match expected value: '{userModelMock.PhoneNumber}'");
        }

        [Fact]
        public async Task DeleteUser_GivenWrongGuid_ThrowsNotFoundException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            Exception catchingException = null;
            try
            {
                await userService.DeleteAsync(Guid.NewGuid());
            }
            catch (Exception ex)
            {
                catchingException = ex;
            }

            Assert.True(catchingException is ItemNotFoundException);
        }

        [Fact]
        public async Task DeleteUser_GivenCorrectGuid_ThrowsNoException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var testGuid = Guid.NewGuid();
            userRepository.GetByIdAsync(testGuid, Arg.Any<bool>()).Returns(new UserModel() { Id = testGuid.ToString() });
            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            Exception catchingException = null;
            try
            {
                await userService.DeleteAsync(testGuid);
            }
            catch (Exception ex)
            {
                catchingException = ex;
                Assert.True(false);
            }

            Assert.True(catchingException is null);
        }

        [Fact]
        public async Task ChangePassword_GivenUserChangeSubmit_CompletesSuccessfully()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var userPasswordChangeSubmit = new UserPasswordChangeSubmit()
            {
                Uuid = Guid.NewGuid(),
                OldPassword = "oldPassword",
                NewPassword = "newPassword1",
                NewPasswordConfirmed = "newPassword1"
            };

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            Exception caughtException = null;
            try
            {
                await userService.ChangePasswordAsync(userPasswordChangeSubmit);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException == null, "Correctly changing password should NOT throw any exeption");
        }

        [Fact]
        public async Task ChangePassword_GivenNoOldPassword_ThrowsException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var userPasswordChangeSubmit = new UserPasswordChangeSubmit()
            {
                Uuid = Guid.NewGuid(),
                NewPassword = "newPassword1",
                NewPasswordConfirmed = "newPassword1"
            };

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            Exception caughtException = null;
            try
            {
                await userService.ChangePasswordAsync(userPasswordChangeSubmit);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ItemNotProcessableException, "Correctly changing password should NOT throw any exeption");
            Assert.True(caughtException.Message == "Old password must be specified.", "Missing old password returned should return 'Old password must be specified.'");
        }

        [Fact]
        public async Task ChangePassword_GivenDifferentNewAndConfirmPassword_ThrowsException()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var roleRepository = Substitute.For<IRoleRepository>();
            var teamRepository = Substitute.For<ITeamRepository>();
            var ldapRepository = Substitute.For<ILdapAuthenticationModeRepository>();
            var ldapConnectionService = Substitute.For<ILdapConnectionService>();

            var userPasswordChangeSubmit = new UserPasswordChangeSubmit()
            {
                Uuid = Guid.NewGuid(),
                OldPassword = "oldPassword",
                NewPassword = "newPassword1",
                NewPasswordConfirmed = "newPassword2"
            };

            var userService = new UserService(userRepository, roleRepository, teamRepository, ldapRepository, mapper, ldapConnectionService);

            Exception caughtException = null;
            try
            {
                await userService.ChangePasswordAsync(userPasswordChangeSubmit);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.True(caughtException is ItemNotProcessableException, "Correctly changing password should NOT throw any exeption");
            Assert.True(caughtException.Message == "New password and confirm new password fields do not match.", "Differing new passwords should return 'New password and confirm new password fields do not match.'");
        }
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class UserController_Tests
    {
        [Fact]
        public async Task GetUserAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            var result = await controller.GetUserAsync(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetUserAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            var result = await controller.GetUserAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUserAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestUserName";

            userService.GetByIdAsync(testGuid, true).Returns(new User { Uuid = testGuid, Username = testName });

            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.GetUserAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var user = okResult.Value as User;
            Assert.NotNull(user);
            Assert.True(user.Uuid == testGuid, $"Retrieved Id {user.Uuid} not the same as sample id {testGuid}.");
            Assert.True(user.Username == testName, $"Retrieved Name {user.Username} not the same as sample id {testName}.");          
        }

        [Fact]
        public async Task ListUsersAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();

            var inList = new List<User>();
            inList.Add(new User { Name = "Test Users 1", Uuid = Guid.NewGuid() });
            inList.Add(new User { Name = "Test Users 2", Uuid = Guid.NewGuid() });
            inList.Add(new User { Name = "Test Users 3", Uuid = Guid.NewGuid() });

            userService.GetListAsync().Returns(inList);

            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.ListUsersAsync(false, false, false, string.Empty, 0, 50, string.Empty, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<User>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.UpdateUserAsync(Guid.Empty, null);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateUserAsync_WithTestUser_ReturnsUpdatedUser()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var inputModel = new UserSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test Name",
                Surname = "Test Surname",
                Email = "Test Email",
                Username = "Test Username",
                RoleIds = new List<Guid>()
                    {
                        new Guid(),
                        new Guid()
                    }
            };

            userService.UpdateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new User()
                {
                    Uuid = inputModel.Uuid,
                    Name = inputModel.Name,
                    Surname = inputModel.Surname,
                    Email = inputModel.Email,
                    Username = inputModel.Username,
                    Roles = new List<Role>()
                    {
                        new Role() { Uuid = inputModel.RoleIds[0] },
                        new Role() { Uuid = inputModel.RoleIds[1] }
                    }
                }
                );

            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.UpdateUserAsync(inputModel.Uuid, inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var user = okResult.Value as User;
            Assert.NotNull(user);
            Assert.True(user.Uuid == inputModel.Uuid, $"Retrieved Id {user.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(user.Name == inputModel.Name, $"Retrieved Name {user.Name} not the same as sample Name {inputModel.Name}.");
            Assert.True(user.Surname == inputModel.Surname, $"Retrieved Surname {user.Surname} not the same as sample Surname {inputModel.Surname}.");
            Assert.True(user.Email == inputModel.Email, $"Retrieved Email {user.Email} not the same as sample Email {inputModel.Email}.");
            Assert.True(user.Username == inputModel.Username, $"Retrieved Username {user.Username} not the same as sample Username {inputModel.Username}.");
            Assert.True(user.Roles.Count == 2, $"Retrieved role count {user.Roles.Count} not the same as sample role count {inputModel.RoleIds.Count}.");
            Assert.True(user.Roles[0].Uuid == inputModel.RoleIds[0], $"Retrieved role id {user.Roles[0].Uuid} not the same as sample role id {inputModel.RoleIds[0]}.");
            Assert.True(user.Roles[1].Uuid == inputModel.RoleIds[1], $"Retrieved role id {user.Roles[1].Uuid} not the same as sample role id {inputModel.RoleIds[1]}.");
        }

        [Fact]
        public async Task CreateUserAsync_WithTestUser_ReturnsCreatesdUser()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var inputModel = new UserSubmit()
            {
                Name = "Test Name",
                Surname = "Test Surname",
                Email = "Test Email",
                PhoneNumber = "Test Phone",
                Username = "Test Username"
            };

            userService.CreateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new User()
                {
                    Name = inputModel.Name,
                    Surname = inputModel.Surname,
                    Email = inputModel.Email,
                    PhoneNumber = inputModel.PhoneNumber,
                    Username = inputModel.Username
                }
                );

            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.CreateUserAsync(inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var user = okResult.Value as User;
            Assert.NotNull(user);
            Assert.True(user.Name == inputModel.Name, $"Retrieved Name {user.Name} not the same as sample Name {inputModel.Name}.");
            Assert.True(user.Surname == inputModel.Surname, $"Retrieved Surname {user.Surname} not the same as sample Surname {inputModel.Surname}.");
            Assert.True(user.Email == inputModel.Email, $"Retrieved Email {user.Email} not the same as sample Email {inputModel.Email}.");
            Assert.True(user.PhoneNumber == inputModel.PhoneNumber, $"Retrieved Email {user.PhoneNumber} not the same as sample Email {inputModel.PhoneNumber}.");
            Assert.True(user.Username == inputModel.Username, $"Retrieved Username {user.Username} not the same as sample Username {inputModel.Username}.");
        }

        [Fact]
        public async Task DeleteUserAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.DeleteUserAsync(Guid.Empty);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidGuid_ReturnsNoData()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.DeleteUserAsync(Guid.NewGuid());

            // Assert
            var notContentResult = actionResult as NoContentResult;
            Assert.NotNull(notContentResult);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            // Act
            IActionResult actionResult = await controller.ChangeUserPasswordAsync(Guid.Empty, null);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task ChangePassword_GivenUserChangeSubmit_ReturnsNoContent()
        {
            // Arrange
            var userService = Substitute.For<IUserService>();
            var profileService = Substitute.For<IProfileService>();
            var controller = new UserController(userService, profileService);

            var guid = Guid.NewGuid();
            var userPasswordChangeSubmit = new UserPasswordChangeSubmit()
            {
                Uuid = guid,
                OldPassword = "oldPassword",
                NewPassword = "newPassword1",
                NewPasswordConfirmed = "newPassword1"
            };

            // Act
            Exception caughtException = null;
            IActionResult actionResult = null;
            try
            {
                actionResult = await controller.ChangeUserPasswordAsync(guid, userPasswordChangeSubmit);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            var noContentResult = actionResult as NoContentResult;
            Assert.NotNull(noContentResult);
        }
    }
}

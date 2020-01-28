/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Controllers;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class ClientController_Tests
    {
        private readonly IClientService clientService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public ClientController_Tests()
        {
            clientService = Substitute.For<IClientService>();
            orderByHelper = Substitute.For<IOrderByHelper>();
            paginationHelper = Substitute.For<IPaginationHelper>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new Oauth2ClientResourceClientModelProfile());
            });

            mapper = config.CreateMapper();
        }


        [Fact]
        public async Task ListClientsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var clientService = Substitute.For<IClientService>();

            var inList = new List<Client>
            {
                new Client { ClientName = "Test Client 1", ClientId = "test-client-1", AllowOfflineAccess = true },
                new Client { ClientName = "Test Client 2", ClientId = "test-client-2", AllowOfflineAccess = false },
                new Client { ClientName = "Test Client 3", ClientId = "test-client-3", AllowOfflineAccess = true }
            };

            PaginatedResult<Client> paginatedResult = new PaginatedResult<Client>
            {
                CurrentPage = 1,
                PageCount = 1,
                PageSize = 3,
                Results = inList
            };

            clientService.GetPaginatedListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<List<KeyValuePair<string, string>>>()).Returns(paginatedResult);

            var controller = new ClientController(clientService, orderByHelper, paginationHelper, mapper);

            // Act
            IActionResult actionResult = await controller.ListClientsAsync(1, 10, string.Empty, string.Empty, string.Empty);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Oauth2Client>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].ClientId, inList[i].ClientId);
                Assert.Equal(outList[i].Name, inList[i].ClientName);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        public async Task GetClientAsync_WithVariousEmptyStrings_ReturnsBadRequest(string id)
        {
            // Arrange
            var controller = new ClientController(clientService, orderByHelper, paginationHelper, mapper);

            // Act
            var result = await controller.GetClientAsync(id);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetClientAsync_WithUnfindableId_ReturnsNotFoundRequest()
        {
            // Arrange
            var controller = new ClientController(clientService, orderByHelper, paginationHelper, mapper);

            // Act
            var result = await controller.GetClientAsync("unfindable-id");

            // Assert
            var requestResult = result as NotFoundResult;
            Assert.NotNull(requestResult);
        }

        [Fact]
        public async Task GetClientAsync_WithTestString_ReturnsCorrectResult()
        {
            // Arrange
            var testId = "test-id";
            var testName = "TestUserName";

            clientService.GetByClientIdAsync(testId).Returns(new Oauth2Client { ClientId = testId, Name = testName });

            var controller = new ClientController(clientService, orderByHelper, paginationHelper, mapper);

            // Act
            IActionResult actionResult = await controller.GetClientAsync(testId);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var resource = okResult.Value as Oauth2Client;
            Assert.NotNull(resource);
            Assert.True(resource.ClientId == testId, $"Retrieved Id {resource.ClientId} not the same as sample id {testId}.");
            Assert.True(resource.Name == testName, $"Retrieved Name {resource.Name} not the same as sample id {testName}.");
        }
    }
}

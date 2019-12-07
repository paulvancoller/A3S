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
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class TeamController_Tests
    {
        private readonly ClaimsPrincipal mockClaimsPrincipal;

        public TeamController_Tests()
        {
            // Setup mock claims principle
            mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "example name"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("custom-claim", "example claim value"),
            }, "mock"));
        }

        [Fact]
        public async Task GetTeamAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var controller = new TeamController(teamService);

            // Act
            var result = await controller.GetTeamAsync(Guid.Empty);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task GetTeamAsync_WithRandomGuid_ReturnsNotFoundResult()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var controller = new TeamController(teamService);

            // Act
            var result = await controller.GetTeamAsync(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTeamAsync_WithTestGuid_ReturnsCorrectResult()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var testGuid = Guid.NewGuid();
            var testName = "TestTeamName";

            teamService.GetByIdAsync(testGuid, true).Returns(new Team { Uuid = testGuid, Name = testName });

            var controller = new TeamController(teamService);

            // Act
            IActionResult actionResult = await controller.GetTeamAsync(testGuid);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var team = okResult.Value as Team;
            Assert.NotNull(team);
            Assert.True(team.Uuid == testGuid, $"Retrieved Id {team.Uuid} not the same as sample id {testGuid}.");
            Assert.True(team.Name == testName, $"Retrieved Name {team.Name} not the same as sample id {testName}.");
        }

        [Fact]
        public async Task ListTeamsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();

            var inList = new List<Team>();
            inList.Add(new Team { Name = "Test Teams 1", Uuid = Guid.NewGuid() });
            inList.Add(new Team { Name = "Test Teams 2", Uuid = Guid.NewGuid() });
            inList.Add(new Team { Name = "Test Teams 3", Uuid = Guid.NewGuid() });

            teamService.GetListAsync().Returns(inList);
            var controller = new TeamController(teamService);

            // Act
            IActionResult actionResult = await controller.ListTeamsAsync(false, false, false, 0, 50, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Team>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }

        [Fact]
        public async Task ListTeamsAsync_WithDataPolicy_ReturnsList()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();

            var inList = new List<Team>();
            inList.Add(new Team { Name = "Test Teams 1", Uuid = Guid.NewGuid() });
            inList.Add(new Team { Name = "Test Teams 2", Uuid = Guid.NewGuid() });
            inList.Add(new Team { Name = "Test Teams 3", Uuid = Guid.NewGuid() });

            // setup dataPolicy claim
            var claimsIdentity = (ClaimsIdentity)mockClaimsPrincipal.Identity;
            claimsIdentity.AddClaim(new Claim("dataPolicy", "a3s.viewYourTeamsOnly"));

            teamService.GetListAsync(Arg.Any<Guid>()).Returns(inList);
            var controller = new TeamController(teamService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = mockClaimsPrincipal }
                }
            };

            // Act
            IActionResult actionResult = await controller.ListTeamsAsync(false, false, false, 0, 50, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Team>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Uuid);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }

        [Fact]
        public async Task UpdateTeamAsync_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var controller = new TeamController(teamService);

            // Act
            IActionResult actionResult = await controller.UpdateTeamAsync(Guid.Empty, null);

            // Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task UpdateTeamAsync_WithTestTeam_ReturnsUpdatedTeam()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var inputModel = new TeamSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test Team Name"
            };

            teamService.UpdateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new Team()
                {
                    Uuid = inputModel.Uuid,
                    Name = inputModel.Name
                }
                );

            var controller = new TeamController(teamService);

            // Act
            IActionResult actionResult = await controller.UpdateTeamAsync(inputModel.Uuid, inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var team = okResult.Value as Team;
            Assert.NotNull(team);
            Assert.True(team.Uuid == inputModel.Uuid, $"Retrieved Id {team.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(team.Name == inputModel.Name, $"Retrieved Name {team.Name} not the same as sample Name {inputModel.Name}.");
        }

        [Fact]
        public async Task CreateTeamAsync_WithTestTeam_ReturnsCreatedTeam()
        {
            // Arrange
            var teamService = Substitute.For<ITeamService>();
            var inputModel = new TeamSubmit()
            {
                Uuid = Guid.NewGuid(),
                Name = "Test Team Name"
            };

            teamService.CreateAsync(inputModel, Arg.Any<Guid>())
                .Returns(new Team()
                {
                    Uuid = inputModel.Uuid,
                    Name = inputModel.Name
                }
                );

            var controller = new TeamController(teamService);

            // Act
            IActionResult actionResult = await controller.CreateTeamAsync(inputModel);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var team = okResult.Value as Team;
            Assert.NotNull(team);
            Assert.True(team.Uuid == inputModel.Uuid, $"Retrieved Id {team.Uuid} not the same as sample id {inputModel.Uuid}.");
            Assert.True(team.Name == inputModel.Name, $"Retrieved Name {team.Name} not the same as sample Name {inputModel.Name}.");
        }
    }
}

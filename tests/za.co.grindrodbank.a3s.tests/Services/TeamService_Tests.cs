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
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class TeamService_Tests
    {
        private readonly IMapper mapper;
        private readonly Guid userGuid;
        private readonly Guid teamGuid;
        private readonly Guid childTeamGuid;
        private readonly Guid policyGuid;
        private readonly Guid termGuid;
        private readonly TeamModel mockedTeamModel;
        private readonly TeamSubmit mockedTeamSubmitModel;

        public TeamService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new TeamResourceTeamModelProfile());
                cfg.AddProfile(new TeamSubmitResourceTeamModelProfile());
                cfg.AddProfile(new UserResourceUserModelProfile());
            });

            mapper = config.CreateMapper();
            teamGuid = Guid.NewGuid();
            childTeamGuid = Guid.NewGuid();
            userGuid = Guid.NewGuid();
            policyGuid = Guid.NewGuid();
            termGuid = Guid.NewGuid();

            mockedTeamModel = new TeamModel
            {
                Name = "Test team",
                Id = teamGuid,
                Description = "Test Description",
                TermsOfServiceId = termGuid,
                TermsOfService = new TermsOfServiceModel()
                {
                    Id = termGuid,
                    AgreementName = "Test Agreement"
                }
            };

            mockedTeamModel.UserTeams = new List<UserTeamModel>
            {
                new UserTeamModel()
                {
                    Team = mockedTeamModel,
                    TeamId = teamGuid,
                    // The identity server user primary keys are stored as strings, not Guids.
                    UserId = userGuid.ToString(),
                    User = new UserModel
                    {
                        // The mapper will attempt to map the user IDs and flatten them, so it needs to be set on the mock.
                        Id = userGuid.ToString(),
                    }
                }
            };

            mockedTeamModel.ChildTeams = new List<TeamTeamModel>()
            {
                new TeamTeamModel()
                {
                    ParentTeamId = teamGuid,
                    ChildTeamId = childTeamGuid,
                    ParentTeam = mockedTeamModel,
                    ChildTeam = new TeamModel()
                    {
                        Id = childTeamGuid
                    }
                }
            };
            
            mockedTeamModel.ApplicationDataPolicies = new List<TeamApplicationDataPolicyModel>()
            {
                new TeamApplicationDataPolicyModel()
                {
                    ApplicationDataPolicyId = policyGuid,
                    Team = mockedTeamModel,
                    TeamId = mockedTeamModel.Id,
                    ApplicationDataPolicy = new ApplicationDataPolicyModel()
                    {
                        Name = "Test Policy Name",
                        Id = policyGuid,
                        Description = "Test Policy Description"
                    }
                }
            };


            mockedTeamSubmitModel = new TeamSubmit()
            {
                Uuid = mockedTeamModel.Id,
                Name = mockedTeamModel.Name,
                Description = mockedTeamModel.Description,
                TermsOfServiceId = mockedTeamModel.TermsOfServiceId,
                TeamIds = new List<Guid>(),
                DataPolicyIds = new List<Guid>()
            };

            foreach (var team in mockedTeamModel.ChildTeams)
                mockedTeamSubmitModel.TeamIds.Add(team.ChildTeamId);

            foreach (var policy in mockedTeamModel.ApplicationDataPolicies)
                mockedTeamSubmitModel.DataPolicyIds.Add(policy.ApplicationDataPolicyId);
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsTeamResource()
        {
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();

            teamRepository.GetByIdAsync(teamGuid, false).Returns(mockedTeamModel);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);
            var teamResource = await teamService.GetByIdAsync(teamGuid);

            Assert.NotNull(teamResource);
            Assert.True(teamResource.Name == "Test team", $"Expected team name: '{teamResource.Name}' does not equal expected value: 'Test team'");
            Assert.True(teamResource.Uuid == teamGuid, $"Expected team UUID: '{teamResource.Uuid}' does not equal expected value: '{teamGuid}'");
            Assert.True(teamResource.UserIds.First() == userGuid, $"Expected User Team User UUID: '{teamResource.UserIds.First()}' does not equal expected value: '{userGuid}'");
        }

        [Fact]
        public async Task GetById_GivenInvalidTermsOfServiceEntry_ThrowsItemNotFoundException()
        {
            var teamRepository = Substitute.For<ITeamRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();

            teamRepository.GetByIdAsync(teamGuid, false).Returns(mockedTeamModel);
            termsOfServiceRepository.When(x => x.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>())).Do(x => { throw new ItemNotFoundException(); });

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);
            var teamResource = await teamService.GetByIdAsync(teamGuid);

            Assert.NotNull(teamResource);
            Assert.True(teamResource.Name == "Test team", $"Expected team name: '{teamResource.Name}' does not equal expected value: 'Test team'");
            Assert.True(teamResource.Uuid == teamGuid, $"Expected team UUID: '{teamResource.Uuid}' does not equal expected value: '{teamGuid}'");
            Assert.True(teamResource.UserIds.First() == userGuid, $"Expected User Team User UUID: '{teamResource.UserIds.First()}' does not equal expected value: '{userGuid}'");
        }

        [Fact]
        public async Task CreateAsync_GivenFullProcessableModel_ReturnsCreatedModel()
        {
            // Arrange
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.GetByIdAsync(mockedTeamModel.UserTeams[0].TeamId, Arg.Any<bool>())
                .Returns(mockedTeamModel.UserTeams[0].Team);
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);
            teamRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>())
                .Returns(new TeamModel()
                {
                    Id = Guid.NewGuid(),
                    ChildTeams = new List<TeamTeamModel>()
                });
            applicationDataPolicyRepository.GetByIdAsync(Arg.Any<Guid>())
                .Returns(mockedTeamModel.ApplicationDataPolicies[0].ApplicationDataPolicy);
            termsOfServiceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(mockedTeamModel.TermsOfService);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());

            // Assert
            Assert.NotNull(teamResource);
            Assert.True(teamResource.Name == mockedTeamSubmitModel.Name, $"Team Resource name: '{teamResource.Name}' not the expected value: '{mockedTeamSubmitModel.Name}'");
            Assert.True(teamResource.TeamIds.Count == mockedTeamSubmitModel.TeamIds.Count, $"Team Resource Teams Count: '{teamResource.TeamIds.Count}' not the expected value: '{mockedTeamSubmitModel.TeamIds.Count}'");
            Assert.True(teamResource.DataPolicyIds.Count == mockedTeamSubmitModel.DataPolicyIds.Count, $"Team Resource Data Policy Count: '{teamResource.DataPolicyIds.Count}' not the expected value: '{mockedTeamSubmitModel.DataPolicyIds.Count}'");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableFunction_ThrowsItemNotFoundException()
        {
            // Arrange
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.When(x => x.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>())).Do(x => throw new ItemNotFoundException());
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, $"Unfindable functions must throw and ItemNotFoundException.");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableTeam_ThrowsItemNotFoundException()
        {
            // Arrange
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.GetByIdAsync(mockedTeamModel.UserTeams[0].TeamId, Arg.Any<bool>())
                .Returns(mockedTeamModel.UserTeams[0].Team);
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, $"Unfindable functions must throw and ItemNotFoundException.");
        }

        [Fact]
        public async Task CreateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.GetByIdAsync(mockedTeamModel.UserTeams[0].TeamId, Arg.Any<bool>())
                .Returns(mockedTeamModel.UserTeams[0].Team);
            teamRepository.GetByIdAsync(mockedTeamModel.Id, Arg.Any<bool>()).Returns(mockedTeamModel);
            teamRepository.GetByNameAsync(mockedTeamSubmitModel.Name, Arg.Any<bool>()).Returns(mockedTeamModel);
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task CreateAsync_GivenUnfindableChildTeam_ThrowsItemNotFoundException()
        {
            // Arrange
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.GetByIdAsync(mockedTeamModel.UserTeams[0].TeamId, Arg.Any<bool>())
                .Returns(mockedTeamModel.UserTeams[0].Team);
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotFoundException, $"Unfindable child teams must throw and ItemNotFoundException.");
        }

        [Fact]
        public async Task CreateAsync_GivenCompoundChildTeam_ThrowsItemNotProcessableException()
        {
            // Arrange
            var teamRepository = Substitute.For<ITeamRepository>();
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var applicationDataPolicyRepository = Substitute.For<IApplicationDataPolicyRepository>();

            teamRepository.GetByIdAsync(mockedTeamModel.UserTeams[0].TeamId, Arg.Any<bool>())
                .Returns(mockedTeamModel.UserTeams[0].Team);
            teamRepository.CreateAsync(Arg.Any<TeamModel>()).Returns(mockedTeamModel);
            teamRepository.GetByIdAsync(mockedTeamModel.ChildTeams[0].ChildTeamId, Arg.Any<bool>())
                .Returns(new TeamModel()
                {
                    Id = Guid.NewGuid(),
                    ChildTeams = new List<TeamTeamModel>()
                    {
                        new TeamTeamModel(),
                        new TeamTeamModel()
                    }
                });

            var teamService = new TeamService(teamRepository, applicationDataPolicyRepository, termsOfServiceRepository, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var teamResource = await teamService.CreateAsync(mockedTeamSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotProcessableException, $"Compound child teams must throw and ItemNotProcessableException.");
        }

    }
}

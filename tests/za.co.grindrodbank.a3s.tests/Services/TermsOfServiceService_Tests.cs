/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class TermsOfServiceService_Tests
    {
        private readonly IMapper mapper;
        private readonly TermsOfServiceModel mockedTermsOfServiceModel;
        private readonly TermsOfServiceSubmit mockedTermsOfServiceSubmitModel;
        private readonly Guid termsOfServiceGuid;

        public TermsOfServiceService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new TermsOfServiceSubmitResourceTermsOfServiceModel());
                cfg.AddProfile(new TermsOfServiceResourceTermsOfServiceModel());
            });

            mapper = config.CreateMapper();
            termsOfServiceGuid = Guid.NewGuid();

            mockedTermsOfServiceModel = new TermsOfServiceModel
            {
                AgreementName = "Test TermsOfService",
                Id = termsOfServiceGuid
            };

            mockedTermsOfServiceModel.Teams = new List<TeamModel>
            {
                new TeamModel
                {
                    TermsOfService = mockedTermsOfServiceModel,
                    Name = "Team Name"
                }
            };

            mockedTermsOfServiceSubmitModel = new TermsOfServiceSubmit()
            {
                Uuid = mockedTermsOfServiceModel.Id,
                AgreementName = mockedTermsOfServiceModel.AgreementName,
            };
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsTermsOfServiceResource()
        {
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var archiveHelper = Substitute.For<IArchiveHelper>();

            termsOfServiceRepository.GetByIdAsync(termsOfServiceGuid, Arg.Any<bool>()).Returns(mockedTermsOfServiceModel);

            var termsOfServiceService = new TermsOfServiceService(termsOfServiceRepository, archiveHelper, mapper);
            var termsOfServiceResource = await termsOfServiceService.GetByIdAsync(termsOfServiceGuid);

            Assert.True(termsOfServiceResource.AgreementName == "Test TermsOfService", $"TermsOfService resource Name: '{termsOfServiceResource.AgreementName}' does not match expected value: 'Test TermsOfService'");
            Assert.True(termsOfServiceResource.Uuid == termsOfServiceGuid, $"TermsOfService resource UUID: '{termsOfServiceResource.Uuid}' does not match expected value: '{termsOfServiceGuid}'");
        }

        [Fact]
        public async Task CreateAsync_GivenFullProcessableModel_ReturnsCreatedModel()
        {
            // Arrange
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var archiveHelper = Substitute.For<IArchiveHelper>();

            termsOfServiceRepository.CreateAsync(Arg.Any<TermsOfServiceModel>()).Returns(mockedTermsOfServiceModel);
            archiveHelper.ReturnFilesListInTarGz(Arg.Any<byte[]>(), Arg.Any<bool>()).Returns(
                new List<string>()
                {
                    "terms_of_service.html",
                    "terms_of_service.css"
                });

            var termsOfServiceService = new TermsOfServiceService(termsOfServiceRepository, archiveHelper, mapper);

            // Act
            var termsOfServiceResource = await termsOfServiceService.CreateAsync(mockedTermsOfServiceSubmitModel, Guid.NewGuid());

            // Assert
            Assert.True(termsOfServiceResource.AgreementName == mockedTermsOfServiceSubmitModel.AgreementName, $"TermsOfService Resource name: '{termsOfServiceResource.AgreementName}' not the expected value: '{mockedTermsOfServiceSubmitModel.AgreementName}'");
        }

        [Fact]
        public async Task CreateAsync_GivenNonTermsOfServiceArchive_ThrowsItemNotProcessableException()
        {
            // Arrange
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var archiveHelper = Substitute.For<IArchiveHelper>();

            termsOfServiceRepository.CreateAsync(Arg.Any<TermsOfServiceModel>()).Returns(mockedTermsOfServiceModel);
            archiveHelper.ReturnFilesListInTarGz(Arg.Any<byte[]>(), Arg.Any<bool>()).Returns(
                new List<string>()
                {
                    "terms_of_service_other_File.html",
                    "terms_of_service.css"
                });

            var termsOfServiceService = new TermsOfServiceService(termsOfServiceRepository, archiveHelper, mapper);

            // Act
            Exception caughtException = null;

            try
            {
                var termsOfServiceResource = await termsOfServiceService.CreateAsync(mockedTermsOfServiceSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.True(caughtException is ItemNotProcessableException, $"A non-terms-of-service archive musth throw an ItemNotProcessable exception.");
        }

        [Fact]
        public async Task CreateAsync_GivenAlreadyUsedName_ThrowsItemNotProcessableException()
        {
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var archiveHelper = Substitute.For<IArchiveHelper>();

            termsOfServiceRepository.GetByIdAsync(mockedTermsOfServiceModel.Id, Arg.Any<bool>()).Returns(mockedTermsOfServiceModel);
            termsOfServiceRepository.GetByAgreementNameAsync(mockedTermsOfServiceSubmitModel.AgreementName, Arg.Any<bool>()).Returns(mockedTermsOfServiceModel);
            termsOfServiceRepository.CreateAsync(Arg.Any<TermsOfServiceModel>()).Returns(mockedTermsOfServiceModel);
            archiveHelper.ReturnFilesListInTarGz(Arg.Any<byte[]>(), Arg.Any<bool>()).Returns(new List<string>());

            var termsOfServiceService = new TermsOfServiceService(termsOfServiceRepository, archiveHelper, mapper);

            // Act
            Exception caughEx = null;
            try
            {
                var termsOfServiceResource = await termsOfServiceService.CreateAsync(mockedTermsOfServiceSubmitModel, Guid.NewGuid());
            }
            catch (Exception ex)
            {
                caughEx = ex;
            }

            // Assert
            Assert.True(caughEx is ItemNotProcessableException, "Attempted create with an already used name must throw an ItemNotProcessableException.");
        }

        [Fact]
        public async Task GetListAsync_Executed_ReturnsList()
        {
            // Arrange
            var termsOfServiceRepository = Substitute.For<ITermsOfServiceRepository>();
            var archiveHelper = Substitute.For<IArchiveHelper>();

            termsOfServiceRepository.GetListAsync().Returns(
                new List<TermsOfServiceModel>()
                {
                    mockedTermsOfServiceModel,
                    mockedTermsOfServiceModel
                });

            var termsOfServiceService = new TermsOfServiceService(termsOfServiceRepository, archiveHelper, mapper);

            // Act
            var termsOfServiceList = await termsOfServiceService.GetListAsync();

            // Assert
            Assert.True(termsOfServiceList.Count == 2, "Expected list count is 2");
            Assert.True(termsOfServiceList[0].AgreementName == mockedTermsOfServiceModel.AgreementName, $"Expected applicationTermsOfService name: '{termsOfServiceList[0].AgreementName}' does not equal expected value: '{mockedTermsOfServiceModel.AgreementName}'");
            Assert.True(termsOfServiceList[0].Uuid == mockedTermsOfServiceModel.Id, $"Expected applicationTermsOfService UUID: '{termsOfServiceList[0].Uuid}' does not equal expected value: '{mockedTermsOfServiceModel.Id}'");
        }
    }
}

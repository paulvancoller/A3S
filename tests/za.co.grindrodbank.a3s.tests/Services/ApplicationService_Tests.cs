/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Xunit;
using NSubstitute;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Models;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Collections.Generic;
using za.co.grindrodbank.a3s.MappingProfiles;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class ApplicationService_Tests
    {
        private readonly IMapper mapper;

        public ApplicationService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ApplicationResourceApplicationModelProfile());
            });

            mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetById_GivenGuid_ReturnsApplicationResource()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            var guid = Guid.NewGuid();
            applicationRepository.GetByIdAsync(guid).Returns(new ApplicationModel { Name = "Test Name", Id = guid });

            var applicationService = new ApplicationService(applicationRepository, mapper);
            var serviceApplication = await applicationService.GetByIdAsync(guid);

            Assert.NotNull(serviceApplication);
            Assert.True(serviceApplication.Name == "Test Name", "Retrieved resource name must be 'Test Name'.");
            Assert.True(serviceApplication.Uuid == guid, $"Retrived resource Uuid must be '{guid}'.");
        }

        [Fact]
        public async Task GetList_GivenNoInput_ReturnsApplicationResourceList()
        {
            var applicationRepository = Substitute.For<IApplicationRepository>();
            List<ApplicationModel> mockedApplicationModels = new List<ApplicationModel>();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            mockedApplicationModels.Add(new ApplicationModel { Name = "Test Name 1", Id = guid1 });
            mockedApplicationModels.Add(new ApplicationModel { Name = "Test Name 2", Id = guid2 });
            applicationRepository.GetListAsync().Returns(mockedApplicationModels);

            var applicationService = new ApplicationService(applicationRepository, mapper);
            var applicationsList = await applicationService.GetListAsync();

            var applicationResource1 = applicationsList.Find(am => am.Name == "Test Name 1");
            Assert.NotNull(applicationResource1);
            Assert.True(applicationResource1.GetType() == typeof(Application));
            Assert.True(applicationResource1.Uuid == guid1, $"Retrived resource 1 Uuid must be '{guid1}'.");
            Assert.True(applicationResource1.Name == "Test Name 1", "Retrieved resource 1 name must be 'Test Name 1'.");

            var applicationResource2 = applicationsList.Find(am => am.Name == "Test Name 2");
            Assert.NotNull(applicationResource2);
            Assert.True(applicationResource2.GetType() == typeof(Application));
            Assert.True(applicationResource2.Uuid == guid2, $"Retrived resource 2 Uuid must be '{guid2}'.");
            Assert.True(applicationResource2.Name == "Test Name 2", "Retrieved resource 2 name must be 'Test Name 2'.");
        }
    }
}

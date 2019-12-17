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

            Assert.True(serviceApplication.Name == "Test Name");
            Assert.True(serviceApplication.Uuid == guid);
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
            Assert.True(applicationResource1.GetType() != null);
            Assert.True(applicationResource1.GetType() == typeof(Application));
            Assert.True(applicationResource1.Uuid == guid1);

            var applicationResource2 = applicationsList.Find(am => am.Name == "Test Name 2");
            Assert.True(applicationResource2.GetType() != null);
            Assert.True(applicationResource2.GetType() == typeof(Application));
        }
    }
}

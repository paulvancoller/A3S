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
using AutoMapper;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class ApplicationController_Tests
    {
        private readonly IMapper mapper;

        public ApplicationController_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ApplicationResourceApplicationModelProfile());
            });

            mapper = config.CreateMapper();
        }

        [Fact]
        public async Task ListApplicationsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var applicationService = Substitute.For<IApplicationService>();

            var inList = new List<ApplicationModel>();
            inList.Add(new ApplicationModel { Name = "Test Applications 1", Id = Guid.NewGuid() });
            inList.Add(new ApplicationModel { Name = "Test Applications 2", Id = Guid.NewGuid() });
            inList.Add(new ApplicationModel { Name = "Test Applications 3", Id = Guid.NewGuid() });

            //var outList = new List<Application>();
            //outList.Add(new Application { Name = "Test Applications 1", Uuid = Guid.NewGuid() });
            //outList.Add(new Application { Name = "Test Applications 2", Uuid = Guid.NewGuid() });
            //outList.Add(new Application { Name = "Test Applications 3", Uuid = Guid.NewGuid() });


            // Set up the paginated response object
            PaginatedResult<ApplicationModel> paginatedResult = new PaginatedResult<ApplicationModel>
            {
                CurrentPage = 1,
                PageCount = 1,
                PageSize = 3,
                Results = inList
            };

            applicationService.GetListAsync(1, 50, string.Empty, null).Returns(paginatedResult);

            var controller = new ApplicationController(applicationService, mapper);

            // Act
            IActionResult actionResult = await controller.ListApplicationsAsync(1, 50, string.Empty, null);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<Application>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Id);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }
    }
}

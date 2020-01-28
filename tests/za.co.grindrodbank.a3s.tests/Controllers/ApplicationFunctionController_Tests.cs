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
using za.co.grindrodbank.a3s.Helpers;
using AutoMapper;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.MappingProfiles;

namespace za.co.grindrodbank.a3s.tests.Controllers
{
    public class ApplicationFunctionController_Tests
    {
        private readonly IApplicationFunctionService applicationFunctionService;
        private readonly IOrderByHelper orderByHelper;
        private readonly IPaginationHelper paginationHelper;
        private readonly IMapper mapper;

        public ApplicationFunctionController_Tests()
        {
            applicationFunctionService = Substitute.For<IApplicationFunctionService>();
            orderByHelper = Substitute.For<IOrderByHelper>();
            paginationHelper = Substitute.For<IPaginationHelper>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ApplicationFunctionResourceApplicationFunctionModelProfile());
            });

            mapper = config.CreateMapper();
        }

        [Fact]
        public async Task ListApplicationFunctionsAsync_WithNoInputs_ReturnsList()
        {
            // Arrange
            var applicationFunctionService = Substitute.For<IApplicationFunctionService>();

            var inList = new List<ApplicationFunctionModel>
            {
                new ApplicationFunctionModel { Name = "Test ApplicationFunctions 1", Id = Guid.NewGuid() },
                new ApplicationFunctionModel { Name = "Test ApplicationFunctions 2", Id = Guid.NewGuid() },
                new ApplicationFunctionModel { Name = "Test ApplicationFunctions 3", Id = Guid.NewGuid() }
            };

            PaginatedResult<ApplicationFunctionModel> paginatedResult = new PaginatedResult<ApplicationFunctionModel>
            {
                CurrentPage = 1,
                PageCount = 1,
                PageSize = 3,
                Results = inList
            };

            applicationFunctionService.GetPaginatedListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<List<KeyValuePair<string, string>>>()).Returns(paginatedResult);

            var controller = new ApplicationFunctionController(applicationFunctionService, orderByHelper, paginationHelper, mapper);

            // Act
            IActionResult actionResult = await controller.ListApplicationFunctionsAsync(1, 10, true, string.Empty, string.Empty);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);

            var outList = okResult.Value as List<ApplicationFunction>;
            Assert.NotNull(outList);

            for (var i = 0; i < outList.Count; i++)
            {
                Assert.Equal(outList[i].Uuid, inList[i].Id);
                Assert.Equal(outList[i].Name, inList[i].Name);
            }
        }
    }
}

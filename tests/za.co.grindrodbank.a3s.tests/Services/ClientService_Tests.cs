/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.MappingProfiles;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.tests.Services
{
    public class ClientService_Tests
    {
        private readonly IMapper mapper;

        public ClientService_Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new Oauth2ClientResourceClientModelProfile());
            });

            mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetList_GivenNoInput_ReturnsClientResourceList()
        {
            var clientRepository = Substitute.For<IIdentityClientRepository>();
            var mockedClientModels = new List<Client>();

            mockedClientModels.Add(new Client { ClientName = "Test Client 1", ClientId = "client-id-1" });
            mockedClientModels.Add(new Client { ClientName = "Test Client 2", ClientId = "client-id-2" });
            clientRepository.GetListAsync().Returns(mockedClientModels);

            var clientService = new ClientService(clientRepository, mapper);
            var clientsList = await clientService.GetListAsync();

            var clientResource1 = clientsList.Find(am => am.Name == "Test Client 1");
            Assert.True(clientResource1.GetType() != null, "Returned client resource 1 must not be null.");
            Assert.True(clientResource1.GetType() == typeof(Oauth2Client), "Returned client resource 1 must be of type Oauth2Client.");
            Assert.True(clientResource1.ClientId == "client-id-1", "Returned client resource 1 ClientId must be 'client-id-1'.");

            var clientResource2 = clientsList.Find(am => am.Name == "Test Client 2");
            Assert.True(clientResource2.GetType() != null, "Returned client resource 2 must not be null.");
            Assert.True(clientResource2.GetType() == typeof(Oauth2Client), "Returned client resource 2 must be of type Oauth2Client.");
            Assert.True(clientResource2.ClientId == "client-id-2", "Returned client resource 2 ClientId must be 'client-id-2'.");
        }
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class SubRealmSubmitResourceSubRealmModelProfile : Profile
    {
        public SubRealmSubmitResourceSubRealmModelProfile()
        {
            CreateMap<SubRealmSubmit, SubRealmModel>();
        }
    }
}

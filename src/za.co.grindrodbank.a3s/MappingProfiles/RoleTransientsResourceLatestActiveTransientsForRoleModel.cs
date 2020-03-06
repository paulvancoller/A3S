/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class RoleTransientsResourceLatestRoleTransientsModelProfile : Profile
    {

        public RoleTransientsResourceLatestRoleTransientsModelProfile()
        {
            CreateMap<LatestActiveTransientsForRoleModel, RoleTransients>().ForMember(dest => dest.LatestActiveRoleTransients, opt => opt.MapFrom(src => src.LatestActiveRoleTransients))
                                                          .ForMember(dest => dest.LatestTransientRoleFunctions, opt => opt.MapFrom(src => src.LatestActiveRoleFunctionTransients))
                                                          .ForMember(dest => dest.LatestTransientRoleChildRoles, opt => opt.MapFrom(src => src.LatestActiveChildRoleTransients));
        }
    }
}

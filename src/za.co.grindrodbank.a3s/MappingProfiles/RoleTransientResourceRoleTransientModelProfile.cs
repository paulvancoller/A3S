/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using AutoMapper;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class RoleTransientResourceRoleTransientModelProfile : Profile
    {
        public RoleTransientResourceRoleTransientModelProfile()
        {
            CreateMap<RoleTransientModel, RoleTransient>().ForMember(dest => dest.RState, opt => opt.MapFrom(src => src.R_State))
                                                          .ForMember(dest => dest.ApprovalCount, opt => opt.MapFrom(src => src.ApprovalCount));
            CreateMap<RoleTransient, RoleTransientModel>().ForMember(dest => dest.R_State, opt => opt.MapFrom(src => src.RState));
        }
    }
}

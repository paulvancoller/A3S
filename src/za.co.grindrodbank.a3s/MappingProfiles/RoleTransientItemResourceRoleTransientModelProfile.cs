/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class RoleTransientItemResourceRoleTransientModelProfile : Profile
    {
        public RoleTransientItemResourceRoleTransientModelProfile()
        {
            CreateMap<RoleTransientModel, RoleTransientsItem>().ForMember(dest => dest.RState, opt => opt.MapFrom(src => src.R_State))
                                                          .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                                                          .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                                          .ForMember(dest => dest.ApprovalCount, opt => opt.MapFrom(src => src.ApprovalCount));
            CreateMap<RoleTransientsItem, RoleTransientModel>().ForMember(dest => dest.R_State, opt => opt.MapFrom(src => src.RState));
        }
    }
}

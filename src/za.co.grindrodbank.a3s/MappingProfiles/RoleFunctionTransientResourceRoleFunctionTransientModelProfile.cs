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
    public class RoleFunctionTransientResourceRoleFunctionTransientModelProfile : Profile
    {
        public RoleFunctionTransientResourceRoleFunctionTransientModelProfile()
        {
            CreateMap<RoleFunctionTransientModel, RoleFunctionTransient>().ForMember(dest => dest.RState, opt => opt.MapFrom(src => src.R_State))
                                                          .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                                                          .ForMember(dest => dest.FunctionId, opt => opt.MapFrom(src => src.FunctionId))
                                                          .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                                          .ForMember(dest => dest.ApprovalCount, opt => opt.MapFrom(src => src.ApprovalCount));
            CreateMap<RoleFunctionTransient, RoleFunctionTransientModel>().ForMember(dest => dest.R_State, opt => opt.MapFrom(src => src.RState));
        }
    }
}

using System;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class RoleRoleTransientResourceRoleRoleTransientModelProfile : Profile
    {
        public RoleRoleTransientResourceRoleRoleTransientModelProfile()
        {
            CreateMap<RoleRoleTransientModel, RoleChildRoleTransient>().ForMember(dest => dest.RState, opt => opt.MapFrom(src => src.R_State))
                                                          .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.ParentRoleId))
                                                          .ForMember(dest => dest.ChildRoleId, opt => opt.MapFrom(src => src.ChildRoleId))
                                                          .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                                          .ForMember(dest => dest.ApprovalCount, opt => opt.MapFrom(src => src.ApprovalCount));
            CreateMap<RoleChildRoleTransient, RoleRoleTransientModel>().ForMember(dest => dest.R_State, opt => opt.MapFrom(src => src.RState));
        }
    }
}

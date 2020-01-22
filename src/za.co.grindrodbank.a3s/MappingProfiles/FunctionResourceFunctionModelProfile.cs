/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Linq;
using za.co.grindrodbank.a3s.Models;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class FunctionResourceFunctionModelProfile : Profile
    {
        public FunctionResourceFunctionModelProfile()
        {
            CreateMap<Function, FunctionModel>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uuid));
            CreateMap<FunctionModel, Function>().ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.FunctionPermissions.Select(fp => fp.Permission)))
                                                .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Application.Id));
        }
    }
}
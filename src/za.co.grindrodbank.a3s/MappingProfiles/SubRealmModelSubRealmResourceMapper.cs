/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Linq;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class SubRealmModelSubRealmResourceMapper : Profile
    {
        public SubRealmModelSubRealmResourceMapper()
        {
            CreateMap<SubRealmModel, SubRealm>().ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.SubRealmPermissions.Select(fp => fp.Permission)))
                                                .ForMember(dest => dest.ApplicationDataPolicies, opt => opt.MapFrom(src => src.SubRealmApplicationDataPolicies.Select(fp => fp.ApplicationDataPolicy)));
        }
    }
}

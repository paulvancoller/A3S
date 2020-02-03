/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using za.co.grindrodbank.a3s.Models;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class LdapAuthenticationModeSubmitResourceLdapAuthenticationModeModelProfile : Profile
    {
        public LdapAuthenticationModeSubmitResourceLdapAuthenticationModeModelProfile()
        { 
            CreateMap<LdapAuthenticationModeSubmit, LdapAuthenticationModeModel>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uuid));
            CreateMap<LdapAuthenticationModeModel, LdapAuthenticationModeSubmit>().ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id));
        }
    }
}

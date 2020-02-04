/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using AutoMapper;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class IdentityServerApiResourceMappingProfile : Profile
    {
        public IdentityServerApiResourceMappingProfile()
        {
            CreateMap<IdentityServer4.EntityFramework.Entities.ApiResourceProperty, KeyValuePair<string, string>>()
                .ReverseMap();

            CreateMap<IdentityServer4.EntityFramework.Entities.ApiResource, IdentityServer4.Models.ApiResource>(MemberList.Destination)
                .ConstructUsing(src => new IdentityServer4.Models.ApiResource())
                .ForMember(x => x.ApiSecrets, opts => opts.MapFrom(x => x.Secrets))
                .ReverseMap();

            CreateMap<IdentityServer4.EntityFramework.Entities.ApiResourceClaim, string>()
                .ConstructUsing(x => x.Type)
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src));

            CreateMap<IdentityServer4.EntityFramework.Entities.ApiSecret, IdentityServer4.Models.Secret>(MemberList.Destination)
                .ForMember(dest => dest.Type, opt => opt.Condition(srs => srs != null))
                .ReverseMap();

            CreateMap<IdentityServer4.EntityFramework.Entities.ApiScope, IdentityServer4.Models.Scope>(MemberList.Destination)
                .ConstructUsing(src => new IdentityServer4.Models.Scope())
                .ReverseMap();

            CreateMap<IdentityServer4.EntityFramework.Entities.ApiScopeClaim, string>()
               .ConstructUsing(x => x.Type)
               .ReverseMap()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src));
        }
    }
}

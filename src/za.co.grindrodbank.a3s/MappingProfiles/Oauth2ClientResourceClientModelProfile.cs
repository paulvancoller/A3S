/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class Oauth2ClientResourceClientModelProfile : Profile
    {
        public Oauth2ClientResourceClientModelProfile()
        {
            CreateMap<Oauth2Client, Client>().ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Name))
                                             .ForMember(dest => dest.AllowOfflineAccess, opt => opt.MapFrom(src => src.AllowedOfflineAccess))
                                             .ReverseMap();

            CreateMap<ClientProperty, KeyValuePair<string, string>>()
                .ReverseMap();

            CreateMap<ClientCorsOrigin, string>()
                .ConstructUsing(src => src.Origin)
                .ReverseMap()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src));

            CreateMap<ClientIdPRestriction, string>()
                .ConstructUsing(src => src.Provider)
                .ReverseMap()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src));

            CreateMap<ClientClaim, Claim>(MemberList.None)
                .ConstructUsing(src => new Claim(src.Type, src.Value))
                .ReverseMap();

            CreateMap<ClientScope, string>()
                .ConstructUsing(src => src.Scope)
                .ReverseMap()
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(src => src));

            CreateMap<ClientPostLogoutRedirectUri, string>()
                .ConstructUsing(src => src.PostLogoutRedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.PostLogoutRedirectUri, opt => opt.MapFrom(src => src));

            CreateMap<ClientRedirectUri, string>()
                .ConstructUsing(src => src.RedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.RedirectUri, opt => opt.MapFrom(src => src));

            CreateMap<ClientGrantType, string>()
                .ConstructUsing(src => src.GrantType)
                .ReverseMap()
                .ForMember(dest => dest.GrantType, opt => opt.MapFrom(src => src));
        }
    }
}

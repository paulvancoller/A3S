using System;
using System.Linq;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class UserProfileResourceUserProfileModelProfile : Profile
    {
        public UserProfileResourceUserProfileModelProfile()
        {
            CreateMap<ProfileModel, UserProfile>().ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.ProfileRoles.Select(pr => pr.Role)))
                                                  .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.ProfileTeams.Select(pt => pt.Team)));
        }
    }
}

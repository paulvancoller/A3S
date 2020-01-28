/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using System.Linq;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class TermsOfServiceResourceTermsOfServiceModel : Profile
    {
        public TermsOfServiceResourceTermsOfServiceModel()
        {
            CreateMap<TermsOfService, TermsOfServiceModel>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uuid))
                                        .ForMember(dest => dest.AgreementFile, opt => opt.MapFrom(src => Convert.FromBase64String(src.AgreementFileData)));
            CreateMap<TermsOfServiceModel, TermsOfService>().ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                        .ForMember(dest => dest.AgreementFileData, opt => opt.MapFrom(src => Convert.ToBase64String(src.AgreementFile)))
                                        .ForMember(dest => dest.TeamIds, opt => opt.MapFrom(src => src.Teams.Select(t => t.Id)))
                                        .ForMember(dest => dest.AcceptedUserIds, opt => opt.MapFrom(src => src.TermsOfServiceAcceptances.Select(a => a.UserId)));

            CreateMap<TermsOfServiceModel, TermsOfServiceListItem>().ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Id))
                                        .ForMember(dest => dest.TeamIds, opt => opt.MapFrom(src => src.Teams.Select(t => t.Id)))
                                        .ForMember(dest => dest.AcceptedUserIds, opt => opt.MapFrom(src => src.TermsOfServiceAcceptances.Select(a => a.UserId)));
        }
    }
}

/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using AutoMapper;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.MappingProfiles
{
    public class ConsentOfServiceResourceConsentOfServiceModel : Profile
    {
        public ConsentOfServiceResourceConsentOfServiceModel()
        {
            CreateMap<ConsentOfService, ConsentOfServiceModel>().ForMember(dest => dest.ConsentFile,
                opt => opt.MapFrom(src => Convert.FromBase64String(src.ConsentFileData)));
            CreateMap<ConsentOfServiceModel, ConsentOfService>().ForMember(dest => dest.ConsentFileData,
                opt => opt.MapFrom(src => Convert.ToBase64String(src.ConsentFile)));
        }
    }
}
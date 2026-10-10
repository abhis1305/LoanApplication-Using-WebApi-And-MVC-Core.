using AutoMapper;
using LoanApp.Models;
using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Mapper
{
    public class MappingData : Profile
    {
        public MappingData()
        {
            CreateMap<CreateLoanDealDto, LoanDeal>();

            CreateMap<LoanDeal, LoanDealDto>();

            CreateMap<LoanDeal, ApprovedLoanDto>() .ForMember(dest => dest.RequestedAmount,
                         opt => opt.MapFrom(src => src.LoanAmount));


            CreateMap<SanctionLetter, SanctionLetterDocumentDto>().ForMember( dest => dest.FileName,
                       opt => opt.MapFrom(src => $"SanctionLetter_{src.DealId}.pdf"));
        }
    }
}

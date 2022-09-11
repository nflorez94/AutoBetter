using AutoBetterScrapper.Models.Dto;
using AutoBetterScrapper.Models.Entities;
using AutoMapper;

namespace AutoBetterScrapper.Transversals.Mappers
{
    public class ScrappingMappers : Profile
    {
        public ScrappingMappers()
        {
            CreateMap<BettingParameter, BettingParameterDto>().ReverseMap();
            CreateMap<Coupon, CouponDto>().ReverseMap();
        }
    }
}

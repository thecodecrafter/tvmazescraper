using API.DTOs;
using API.Models;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<TvShow, TvShowDto>()
                .ForMember(
                    d => d.CastMembers,
                    o => o.MapFrom(s => s.CastMembers.OrderByDescending(x => x.Birthday))
                );
            CreateMap<CastMember, CastMemberDto>();
        }
    }
}

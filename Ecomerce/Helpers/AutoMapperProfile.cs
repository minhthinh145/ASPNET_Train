using AutoMapper;
using Ecomerce.Data;
using Ecomerce.ViewModels;

namespace Ecomerce.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<RegisterVM, KhachHang>();
            // .ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM =>
            //RegisterVM.HoTen));
            //.ReverseMap();
        }
    }
}

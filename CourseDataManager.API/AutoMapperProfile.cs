using AutoMapper;
using CourseDataManager.BLL.DTOs;
using CourseDataManager.DAL.Entities;

namespace CourseDataManager.API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() {
            CreateMap<UserRegisterDTO, User>();
        }
    }
}

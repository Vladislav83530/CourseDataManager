using CourseDataManager.DAL.Entities;

namespace CourseDataManager.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> SaveUserAsync(User user);
        Task<User> GetUserAsync(string email);
    }
}

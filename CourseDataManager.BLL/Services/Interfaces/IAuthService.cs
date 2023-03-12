using CourseDataManager.DAL.Entities;

namespace CourseDataManager.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        Task<bool> IsRegisteredAsync(string email);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateToken(User user);
        Task UpdateUserJwtToken(string email, long chatId, string JwtToken);
        Task<string> GetUserJwtToken(long chatId);
    }
}

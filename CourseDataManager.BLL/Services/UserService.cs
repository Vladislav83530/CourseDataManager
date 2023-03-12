using CourseDataManager.BLL.Services.Interfaces;
using CourseDataManager.DAL.EF;
using CourseDataManager.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseDataManager.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>user</returns>
        public async Task<User> GetUserAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        

        /// <summary>
        /// Save user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Saved user</returns>
        public async Task<User> SaveUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
         
        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>list if user</returns>
        public async Task<IEnumerable<User>> GetStudentsAsync() =>
            await _context.Users.Where(x=>x.Role == Roles.Student).OrderBy(x=>x.Group).ToListAsync();

        /// <summary>
        /// Reverse isAvailable value
        /// </summary>
        /// <param name="email"></param>
        /// <param name="isAvailable_"></param>
        /// <returns></returns>
        public async Task ReverseIsAvailableValueAsync(string email, bool isAvailable_)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email == email && x.isAvailable == isAvailable_);
            if (user != null)
            {
                user.isAvailable = !isAvailable_;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

    }
}

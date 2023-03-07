using CourseDataManager.BLL.Services.Interfaces;
using CourseDataManager.DAL.EF;
using CourseDataManager.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace CourseDataManager.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext appDbContext, IConfiguration config)
        {
            _appDbContext = appDbContext;
            _config = config;
        }

        /// <summary>
        /// Creating password hash
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Check if user is registered 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Is user registere? true/false</returns>
        public async Task<bool> IsRegisteredAsync(string email)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            if (user != null)
                return true;
            return false;
        }

        /// <summary>
        /// Verify password hash
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        /// <return>Verify result (true/false)</returns>
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        /// <summary>
        /// Create jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns>jwt token</returns>
        public string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        /// <summary>
        /// Update jwt token 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task UpdateUserJwtToken(string email, long chatId, string jwtToken)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.ChatId == chatId);
            if (user != null)
            {
                user.JwtToken = jwtToken;
                user.ChatId = chatId;
                _appDbContext.Users.Update(user);
                await _appDbContext.SaveChangesAsync();
            }
            else
            {
                var user_ = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user_ != null)
                {
                    user_.JwtToken = jwtToken;
                    user_.ChatId = chatId;
                    _appDbContext.Users.Update(user_);
                    await _appDbContext.SaveChangesAsync();
                }
                else
                    throw new Exception("Не знайдено користувача");
            }
        }

        /// <summary>
        /// Get user jwt token
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns>jwt token</returns>
        public async Task<string> GetUserJwtToken(long chatId)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x=>x.ChatId == chatId);
            if (user == null)
                return null;

            return user.JwtToken;
        }
    }
}

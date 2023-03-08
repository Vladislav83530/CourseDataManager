using AutoMapper;
using CourseDataManager.BLL.DTOs;
using CourseDataManager.BLL.Services.Interfaces;
using CourseDataManager.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseDataManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public AuthController(IAuthService authService, IUserService userService, IMapper mapper)
        {
            _authService = authService;
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="request"></param>
        /// <returns>registered user</returns>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> Register(UserRegisterDTO request)
        {
            if (await _authService.IsRegisteredAsync(request.Email.ToLower()))
                return BadRequest($"Користувач з поштою {request.Email} вже зареєстрований.");

            _authService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = _mapper.Map<User>(request);
            user.isAvailable = true;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Role = Roles.Student;

            var savedUser =  await _userService.SaveUserAsync(user);
            return Ok(savedUser);
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Jwt token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Не вірно введено логін чи пароль.");

            if (!await _authService.IsRegisteredAsync(request.Email.ToLower()))
                return BadRequest($"Користувач з поштою {request.Email} не зареєстрований.");

            User user = await _userService.GetUserAsync(request.Email);

            if (!_authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest("Не правильний пароль.");

            string token = _authService.CreateToken(user);
            await _authService.UpdateUserJwtToken(user.Email, request.ChatId, token);

            return Ok(token);
        }

        /// <summary>
        /// Get user JWT token
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns>JWT token</returns>
        [HttpGet("token")]
        public async Task<ActionResult<string>> GetUserJwtToken(long chatId)
        {
           var output = await _authService.GetUserJwtToken(chatId);
           return Ok(output);
        }

        /// <summary>
        /// Update Jwt token
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        [HttpPost("updatetoken")]
        public async Task<ActionResult<string>> UpdateJwtToken(long chatId, string? jwtToken)
        {
            await _authService.UpdateUserJwtToken(null, chatId, jwtToken);
            return Ok();
        }
    }
}

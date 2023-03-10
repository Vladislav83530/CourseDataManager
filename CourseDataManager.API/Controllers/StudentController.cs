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
    public class StudentController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public StudentController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all students
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAllStudent()
        {
            var students = await _userService.GetStudentsAsync();
            var result = _mapper.Map<IEnumerable<User>, List<UserDTO>>(students);
            return Ok(result);
        }

        /// <summary>
        /// Reverse isAvailable value
        /// </summary>
        /// <param name="email"></param>
        /// <param name="isAvailable"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("isAvailable")]
        [Authorize(Roles="Admin")]
        public async Task<ActionResult> ReverseIsAvailableValue(string email, bool isAvailable)
        {
            try
            {
                await _userService.ReverseIsAvailableValueAsync(email, isAvailable);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

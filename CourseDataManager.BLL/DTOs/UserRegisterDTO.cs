using CourseDataManager.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace CourseDataManager.BLL.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string UserSurname { get; set; } = string.Empty;
        [Required] 
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public Roles Role { get; set; }
    }
}

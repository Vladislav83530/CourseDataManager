using System.ComponentModel.DataAnnotations;

namespace CourseDataManager.BLL.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public long ChatId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CourseDataManager.Bot.Models
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public long ChatId { get; set; }
    }
}

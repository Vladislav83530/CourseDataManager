using System.ComponentModel.DataAnnotations;

namespace CourseDataManager.DAL.Entities
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string UserSurname { get; set; } = string.Empty;
        [Required] 
        public string Email { get; set; } = string.Empty;
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        [Required]
        public Roles Role { get; set; }
        [Required]
        public bool isAvailable { get; set; }
        [Required]
        public int Group { get; set; }
        public string? JwtToken { get; set; } = string.Empty;
        public long ChatId { get; set; }
    }

    public enum Roles
    {
        Student,
        Admin
    }
}

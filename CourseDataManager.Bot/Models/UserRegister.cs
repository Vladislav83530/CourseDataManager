namespace CourseDataManager.Bot.Models
{
    internal class UserRegister
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; } 
        public int Group { get; set; }
    }
}

namespace CourseDataManager.BLL.DTOs
{
    public class UserDTO
    {
        public string UserName { get; set; } 
        public string UserSurname { get; set; } 
        public string Email { get; set; } 
        public bool isAvailable { get; set; }
        public int Grouup { get; set; }
    }
}

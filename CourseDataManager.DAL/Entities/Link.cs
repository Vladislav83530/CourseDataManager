using System.ComponentModel.DataAnnotations;

namespace CourseDataManager.DAL.Entities
{
    public class Link
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Link_ { get; set; } = string.Empty;
        [Required]
        public int Group { get; set; }
    }
}

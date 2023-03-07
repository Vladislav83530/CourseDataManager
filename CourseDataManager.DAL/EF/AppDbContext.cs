using CourseDataManager.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseDataManager.DAL.EF
{
    /// <summary>
    /// Application Database Context
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}

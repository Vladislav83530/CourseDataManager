using CourseDataManager.DAL.Entities;

namespace CourseDataManager.BLL.Services.Interfaces
{
    public interface ILinkService
    {
        Task<Link> CreateLinkAsync(Link link);
        Task<IEnumerable<Link>> GetLinksByNameAsync(string linkName);
    }
}

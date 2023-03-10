using CourseDataManager.BLL.Services.Interfaces;
using CourseDataManager.DAL.EF;
using CourseDataManager.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseDataManager.BLL.Services
{
    public class LinkService : ILinkService
    {
        private readonly AppDbContext _context;
        public LinkService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// create link
        /// </summary>
        /// <param name="link"></param>
        /// <returns>created link</returns>
        public async Task<Link> CreateLinkAsync(Link link)
        {
            _context.Links.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        /// <summary>
        /// Get links by name
        /// </summary>
        /// <param name="linkName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<Link>> GetLinksByNameAsync(string linkName)
        {
            if (string.IsNullOrWhiteSpace(linkName))
            {
               var links = await _context.Links.Where(x=>x.Name.Contains(linkName, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();
               return links;
            }
            else
                throw new Exception("Назва посилання не можу бути пуста");
        }
    }
}

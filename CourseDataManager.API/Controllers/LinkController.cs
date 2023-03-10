using CourseDataManager.BLL.Services.Interfaces;
using CourseDataManager.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseDataManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        private readonly ILinkService _linkService;
        public LinkController(ILinkService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet]
        [Route("links")]
        [Authorize]
        public async Task<ActionResult> GetLinksByName([FromQuery] string linkName)
        {
            var links = await _linkService.GetLinksByNameAsync(linkName);    
            return Ok(links);
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateLink(Link link)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _linkService.CreateLinkAsync(link);

            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectWBSAPI.Helper;

namespace ProjectWBSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectWBSController : ControllerBase
    {
        private readonly ProjectSyncService _orderService;

        public ProjectWBSController(ProjectSyncService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("ProjectSync")]
        public async Task<IActionResult> ProjectSync(bool DateFlag)
        {
            await _orderService.SyncProjects(DateFlag);
            return Ok("Project Sync successfully");
        }

        [HttpPost("WBSSync")]
        public async Task<IActionResult> WBSSync(bool DateFlag)
        {
            await _orderService.SyncWBS(DateFlag);
            return Ok("WBS Sync successfully");
        }
    }
}

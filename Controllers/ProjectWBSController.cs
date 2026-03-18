using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectWBSAPI.Helper;

namespace ProjectWBSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectWBSController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public ProjectWBSController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process()
        {
            await _orderService.ProcessOrdersAsync();
            return Ok("Processed");
        }
    }
}

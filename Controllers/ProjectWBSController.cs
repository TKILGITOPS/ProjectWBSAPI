using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectWBSAPI.Helper;

namespace ProjectWBSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectWBSController : ControllerBase
    {
        private readonly ProjectSyncService _orderService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ProjectWBSController(ProjectSyncService orderService, IEmailService emailService, IConfiguration config)
        {
            _orderService = orderService;
            _emailService = emailService;
            _config = config;
        }

        [HttpPost("ProjectSync")]
        public async Task<IActionResult> ProjectSync(bool DateFlag)
        {
            var result = await _orderService.SyncProjects(DateFlag);
            if (Convert.ToInt32(result.projectCount) > 0 )
            {
                var projectListHtml = "<ul>" + string.Join("", result.projects.Select(p => $"<li>{p.ProjectCode}</li>")) + "</ul>";
                string body = $"Hello Team,<br/><br/>This is to inform you that there are {result.projectCount} projects were synced and inserted into the Pragati Application.<br/><br/>" +
                               "Please see the following project list for your reference : <br/>" +
                               $"{projectListHtml} <br/><br/>" +
                               "Regards,<br/>" +
                               "System Administration <br/><br/>" +
                "<span style='color:red; font-weight:bold;'>Note:- This is system generated email, please do not reply.</span>";
                await _emailService.SendEmailAsync(_config["Email:TO"]!, _config["Email:CC"]!, "Project Sync Completed Successfully in Pragati Application", body);
            }
            return Ok("Project Sync successfully");
        }

        [HttpPost("WBSSync")]
        public async Task<IActionResult> WBSSync(bool DateFlag)
        {
            var wbsresult = await _orderService.SyncWBS(DateFlag);
            if (Convert.ToInt32(wbsresult.wbsCount) > 0)
            {
                var projectListHtml = "<ul>" + string.Join("", wbsresult.projects.Select(p => $"<li>{p.ProjectCode}</li>")) + "</ul>";
                string body = $"Hello Team,<br/><br/>This is to inform you that there are {wbsresult.wbsCount} WBS were synced and inserted into the Pragati Application.<br/><br/>" +
                               "Additionally, Please see the following project list that WBS not inserted in the Pragati Application : <br/>" +
                               $"{projectListHtml} <br/><br/>" +
                               "Regards,<br/>" +
                               "System Administration <br/><br/>" +
                "<span style='color:red; font-weight:bold;'>Note:- This is system generated email, please do not reply.</span>";
                await _emailService.SendEmailAsync(_config["Email:TO"]!, _config["Email:CC"]!, "WBS Sync Completed Successfully in Pragati Application", body);
            }
            return Ok("WBS Sync successfully");
        }
    }
}

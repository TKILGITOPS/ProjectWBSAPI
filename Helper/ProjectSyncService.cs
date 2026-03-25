using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectWBSAPI.Model;
using System;
using static SAP.Middleware.Connector.RfcAbapClassException;

namespace ProjectWBSAPI.Helper
{
    public class ProjectSyncService
    {
        private readonly SapProjectService _sapService;
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectSyncService> _logger;
        private readonly IEmailService _emailService;
        private readonly bool _dynamicDateFlag;

        public ProjectSyncService(SapProjectService sapService, AppDbContext context, ILogger<ProjectSyncService> logger, IEmailService emailService, IOptions<DateSetting> options)
        {
            _sapService = sapService;
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _dynamicDateFlag = options.Value.UseDynamicDates;
        }

        public async Task<(string projectCount,List<ProjectCodes> projects)> SyncProjects(bool dateflag)
        {
            try
            {
                _logger.LogInformation("Project sync started"+0, DateTime.Now);
                if (!string.IsNullOrEmpty(Convert.ToString(dateflag)))
                {
                    dateflag = _dynamicDateFlag;
                }
                int projectcount = 0;
                var projects = new List<Project>();
                var orders = _sapService.GetProjects(dateflag);
                var existingProjectCodes = _context.Project
                                            .Select(x => x.ProjectCode!.Trim().ToUpper())
                                            .ToHashSet();

                foreach (var order in orders)
                {
                    if (!existingProjectCodes.Contains(order.ProjectCode!.ToUpper().Trim()))
                    {
                        var project = new Project
                        {
                            ProjectCode = order.ProjectCode,
                            ProjectName = order.ProjectDescription,
                            BU = _context.BusinessDivisions
                                .Where(x => x.BusinessDivision!.ToUpper() == order.BU!.ToUpper())
                                .Select(x => x.BusinessID)
                                .FirstOrDefault(),
                            ProjectStatus = "In Progress",
                            CreatedBy = "System"
                        };
                        _context.Project.Add(project);
                        projects.Add(project);
                        existingProjectCodes.Add(order.ProjectCode!.ToUpper().Trim());

                        projectcount++;
                    }
                    else
                    {
                        //Duplicate entries skip
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("No. of project inserted -" + projectcount, DateTime.Now);
                _logger.LogInformation("Project sync ended"+0, DateTime.Now);
                var projectCodesList = projects.Select(p => new ProjectCodes
                {
                    ProjectCode = p.ProjectCode
                }).ToList();

                return (projectcount.ToString(), projectCodesList);
                //if (projectcount > 0)
                //{
                //    string body = $"This is to inform you that {projectcount} projects were synced and inserted into the Pragati Application.<br/><br/>" +
                //                   "Regards,<br/>" +
                //                   "System Administration";
                //    await _emailService.SendEmailAsync("prathmesh.parkhe@tkil.com","dattatray.mhase@tkil.com", "Project Sync in Pragati Application", body);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing scheduled job.");
                return ("0", new List<ProjectCodes>());
            }
        }

        public async Task<(string wbsCount, List<WBSCodes> wbss)> SyncWBS(bool dateflag)
        {
            try
            {
                _logger.LogInformation("WBS sync started"+1, DateTime.Now);
                if (!string.IsNullOrEmpty(Convert.ToString(dateflag)))
                {
                    dateflag = _dynamicDateFlag;
                }
                int wbscount = 0;
                var orders = _sapService.GetWBS(dateflag);

                var wbsscode = new List<WBS>();
                HashSet<string> existingSet = new HashSet<string>();
                var projects = await _context.Project
                                .Where(x => !string.IsNullOrWhiteSpace(x.ProjectCode) && x.ProjectID != null)
                                .Select(x => new
                                {
                                    ProjectCode = x.ProjectCode.Trim().ToUpper(),
                                    x.ProjectID
                                })
                                .Distinct()
                                .ToDictionaryAsync(
                                    x => x.ProjectCode!.Trim().ToUpper(), 
                                    x => x.ProjectID);
                var existingWBS = await _context.WBS
                    //.Select(x => new { x.ProjectID,x.WBSCode,x.WBSName })
                    .ToListAsync();

                existingSet = existingWBS
                                  .Select(x => $"{x.ProjectID}_{x.WBSCode}_{x.WBSName}")
                                  .ToHashSet();

                foreach (var order in orders)
                {
                    var projectCode = order.ProjectName!.Trim().ToUpper();

                    if (projects.TryGetValue(projectCode, out var projectID))
                    {
                        var key = $"{projectID}_{order.WBSCode}_{order.WBSName}";

                        if (!existingSet.Contains(key))
                        {
                            var wbs = new WBS
                            {
                                ProjectID = projectID,
                                WBSCode = order.WBSCode,
                                WBSName = order.WBSName,
                                CreatedBy = "System"
                            };
                            _context.WBS.Add(wbs);
                            wbsscode.Add(wbs);
                            existingWBS.Add(wbs);
                            existingSet.Add(key); // prevent duplicate in same batch
                            wbscount++;
                        }
                        else
                        {
                            //string abc = "exists record";
                        }
                    }
                    else
                    {
                        //string abc = "exists record";
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("No. of WBS inserted -" + wbscount, DateTime.Now);
                _logger.LogInformation("WBS sync end" + 1, DateTime.Now);

                var wbsCodesList = wbsscode.Select(p => new WBSCodes
                {
                    WBSCode = p.WBSCode
                }).ToList();

                //var existingWBSs = await _context.WBS.ToListAsync();
                var wbsLookup = existingWBS
                                .GroupBy(x => new { x.ProjectID, x.WBSCode })
                                .ToDictionary(
                                    g => (g.Key.ProjectID, g.Key.WBSCode),
                                    g => g.First());
                _logger.LogInformation("WBS updation start" + 2, DateTime.Now);
                foreach (var order in orders)
                {
                    var projectCode = order.ProjectName!.Trim().ToUpper();

                    if (!projects.TryGetValue(projectCode, out var projectID))
                        continue;

                    if (!wbsLookup.TryGetValue((projectID, order.WBSCode), out var child))
                        continue;

                    // CASE 1: No Superior → self parent
                    if (string.IsNullOrWhiteSpace(order.Superior))
                    {
                        child.ParentWBSID = child.WBSID;
                    }
                    else
                    {
                        // CASE 2: Has Superior → map to parent
                        var superiorCode = order.Superior.Trim();

                        if (wbsLookup.TryGetValue((projectID, superiorCode), out var parent))
                        {
                            child.ParentWBSID = parent.WBSID;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("WBS updation end" + 2, DateTime.Now);

                return (wbscount.ToString(), wbsCodesList);
                //if (wbscount > 0)
                //{
                //    string body = $"This is to inform you that {wbscount} WBS were synced and inserted into the Pragati Application.<br/><br/>" +
                //                   "Regards,<br/>" +
                //                   "System Administration";

                //    await _emailService.SendEmailAsync("prathmesh.parkhe@tkil.com", "dattatray.mhase@tkil.com", "WBS Sync in Pragati Application", body);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing scheduled job.");
                return ("0", new List<WBSCodes>());
            }
        }

    }
}

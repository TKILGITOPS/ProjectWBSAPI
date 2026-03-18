using Microsoft.EntityFrameworkCore;
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

        public ProjectSyncService(SapProjectService sapService, AppDbContext context, ILogger<ProjectSyncService> logger)
        {
            _sapService = sapService;
            _context = context;
            _logger = logger;
        }

        public async Task SyncProjects()
        {
            try
            {
                _logger.LogInformation("Project sync started"+0, DateTime.Now);
                int projectcount = 0;
                var orders = _sapService.GetProjects();
                var existingProjectCodes = _context.Project
                                            .Select(x => x.ProjectCode!.Trim().ToUpper())
                                            .ToHashSet();

                foreach (var order in orders)
                {
                    if (!existingProjectCodes.Contains(order.ProjectCode!.ToUpper().Trim()))
                    {
                        _context.Project.Add(new Project
                        {
                            ProjectCode = order.ProjectCode,
                            ProjectName = order.ProjectDescription,
                            BU = _context.BusinessDivisions
                                .Where(x => x.BusinessDivision!.ToUpper() == order.BU!.ToUpper())
                                .Select(x => x.BusinessID)
                                .FirstOrDefault(),
                            ProjectStatus = "In Progress",
                            CreatedBy = "System"
                        });
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing scheduled job.");
            }
        }

        public async Task SyncWBS()
        {
            try
            {
                _logger.LogInformation("WBS sync started"+1, DateTime.Now);
                int wbscount = 0;
                var orders = _sapService.GetWBS();

                HashSet<string> existingSet = new HashSet<string>();
                var projects = await _context.Project
                                .ToDictionaryAsync(
                                    x => x.ProjectCode!.Trim().ToUpper(),
                                    x => x.ProjectID);
                var existingWBS = await _context.WBS.ToListAsync();

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
                            existingWBS.Add(wbs);
                            existingSet.Add(key); // prevent duplicate in same batch
                            wbscount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("No. of WBS inserted -" + wbscount, DateTime.Now);
                _logger.LogInformation("WBS sync end" + 1, DateTime.Now);
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectWBSAPI.Helper;
using ProjectWBSAPI.Model;

public class BackgroundWorkerService : BackgroundService
{
    readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _scheduledTime;
    private readonly bool _dynamicDateFlag;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IServiceScopeFactory serviceScopeFactory, IOptions<JobSettings> options, IOptions<DateSetting> dateOptions, IEmailService emailService, IConfiguration config)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _scheduledTime = options.Value.RunTime;
        _dynamicDateFlag = dateOptions.Value.UseDynamicDates;
        _emailService = emailService;
        _config = config;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            // Set your desired run time (e.g., 1:00 AM)
            //var scheduledTime = new TimeSpan(10, 0, 0); // removed hardcode and set the dynamic code here

            var nextRun = DateTime.Today.Add(_scheduledTime);
            //var nextRun = DateTime.Today.Add(scheduledTime);
            // If time already passed today, schedule for tomorrow
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;

            _logger.LogInformation("Next run scheduled at {time}", nextRun);

            await Task.Delay(delay, stoppingToken);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var orderService = scope.ServiceProvider.GetRequiredService<ProjectWBSAPI.Helper.ProjectSyncService>();
                var projectresult = await orderService.SyncProjects(_dynamicDateFlag);
                var wbsresult = await orderService.SyncWBS(_dynamicDateFlag);

                if (Convert.ToInt32(projectresult.projectCount) > 0 || Convert.ToInt32(wbsresult.wbsCount) > 0)
                {
                    var projectListHtml = "<ul>" + string.Join("", projectresult.projects.Select(p => $"<li>{p.ProjectCode}</li>"))+ "</ul>";
                    var projectListwbsHtml = "<ul>" + string.Join("", wbsresult.projects.Select(p => $"<li>{p.ProjectCode}</li>")) + "</ul>";
                    string body = $"Hello Team,<br/><br/>This is to inform you that there are {projectresult.projectCount} projects and {wbsresult.wbsCount} WBS were synced and inserted into the Pragati Application.<br/><br/>" +
                                   "Please see the following project list for your reference : <br/>" +
                                   $"{projectListHtml} <br/><br/>" +
                                   "Additionally, Please see the following project list that WBS not inserted in the Pragati Application : <br/>" +
                                   $"{projectListwbsHtml} <br/><br/>" +
                                   "Regards,<br/>" +
                                   "System Administration <br/><br/>"+
                    "<span style='color:red; font-weight:bold;'>Note:- This is system generated email, please do not reply.</span>";
                    await _emailService.SendEmailAsync(_config["Email:TO"]!, _config["Email:CC"]!, "Project and WBS Sync Completed Successfully in Pragati Application", body);
                }

                await RunDailyJob();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " - Error occurred while executing scheduled job.", DateTime.Now);
            }
        }
    }

    private Task RunDailyJob()
    {
        _logger.LogInformation("Daily job executed at {time}", DateTime.Now);
        return Task.CompletedTask;
    }
}
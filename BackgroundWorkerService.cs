
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectWBSAPI.Helper;
using ProjectWBSAPI.Model;

public class BackgroundWorkerService :BackgroundService
{
    readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _scheduledTime;
    private readonly bool _dynamicDateFlag;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IServiceScopeFactory serviceScopeFactory, IOptions<JobSettings> options, IOptions<DateSetting> dateOptions)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _scheduledTime = options.Value.RunTime;
        _dynamicDateFlag = dateOptions.Value.UseDynamicDates;
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
            await orderService.SyncProjects(_dynamicDateFlag);
            await orderService.SyncWBS(_dynamicDateFlag);

            await RunDailyJob();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message+ " - Error occurred while executing scheduled job.", DateTime.Now);
            }
        }
    }

    private Task RunDailyJob()
    {
        _logger.LogInformation("Daily job executed at {time}", DateTime.Now);
        return Task.CompletedTask;
    }
}
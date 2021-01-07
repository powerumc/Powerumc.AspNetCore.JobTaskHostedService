# Powerumc.AspNetCore.JobTaskHostedService
Background Hosted Job Service based the `Task`.

### Example

Add in `Startup.cs`
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJobTaskHostedService();
}
```

```cs
public class TestService
{
    private readonly IJobTaskHostedService _jobService;

    public TestService(IJobTaskHostedService jobService)
    {
        _jobService = jobService;
    }

    public void Start()
    {
        // Create a job.
        var job = _jobService.CreateJob("Job 1");
        job.Enqueue(() =>
        {
            // Some bulk task.
        });
        
        return Task.CompletedTask;
    }
}
```
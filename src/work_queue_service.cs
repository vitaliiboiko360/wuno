using System.Threading.Channels;

public interface IBackgroundTaskQueue
{
  ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

  ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
  private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

  public BackgroundTaskQueue(int capacity)
  {
    var options = new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait
    };
    _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
  }

  public async ValueTask QueueBackgroundWorkItemAsync(
    Func<CancellationToken, ValueTask> workItem
  )
  {
    if (workItem == null)
    {
      throw new ArgumentNullException(nameof(workItem));
    }

    await _queue.Writer.WriteAsync(workItem);
  }

  public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
    CancellationToken cancellationToken
  )
  {
    var workItem = await _queue.Reader.ReadAsync(cancellationToken);

    return workItem;
  }
}


public class QueuedService : BackgroundService
{
  private readonly ILogger<QueuedService> _logger;

  public QueuedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedService> logger)
  {
    taskQueue = taskQueue;
    _logger = logger;
  }

  public IBackgroundTaskQueue TaskQueue { get; }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("service is running");

    await BackgroundProcessing(stoppingToken);
  }

  private async Task BackgroundProcessing(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var workItem = await TaskQueue.DequeueAsync(stoppingToken);


      try
      {
        await workItem(stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "error {workItem}", nameof(workItem));
      }
    }
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("service is stopping");
    await base.StopAsync(stoppingToken);
  }
}


public class MonitorLoop
{
  private readonly IBackgroundTaskQueue _taskQueue;
  private readonly ILogger _logger;
  private readonly CancellationToken _cancellationToken;

  public MonitorLoop(IBackgroundTaskQueue taskQueue, ILogger<MonitorLoop> logger, IHostApplicationLifetime applicationLifetime)
  {
    _taskQueue = taskQueue;
    _logger = logger;
    _cancellationToken = applicationLifetime.ApplicationStopping;
  }

  public void StartMonitorLoop()
  {
    _logger.LogInformation("monitor loop is starting");

    Task.Run(async () => await MonitorAsync());
  }

  private async ValueTask MonitorAsync()
  {
    while (!_cancellationToken.IsCancellationRequested)
    {
      // Enqueue a background work item
      await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItem);
    }
  }

  private async ValueTask BuildWorkItem(CancellationToken token)
  {
    int delayLoop = 0;
    var guid = Guid.NewGuid().ToString();
    _logger.LogInformation("queued background task {guid} is starting", guid);

    while (!token.IsCancellationRequested && delayLoop < 3)
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(5), token);
      }
      catch (OperationCanceledException)
      {
        // no throw
      }

      delayLoop++;
      _logger.LogInformation("queued background task {guid} is running", guid);
    }

    if (delayLoop == 3)
    {
      _logger.LogInformation("queued background task {guid} is complete", guid);
    }
    else
    {
      _logger.LogInformation("queued background task {guid} was cancelled", guid);
    }
  }
}
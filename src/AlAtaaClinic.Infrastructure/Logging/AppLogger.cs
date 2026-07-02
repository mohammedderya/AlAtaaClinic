using AlAtaaClinic.Application.Abstractions.Logging;
using Microsoft.Extensions.Logging;

namespace AlAtaaClinic.Infrastructure.Logging;

public sealed class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void Information(string message)
    {
        _logger.LogInformation("{Message}", message);
    }

    public void Warning(string message)
    {
        _logger.LogWarning("{Message}", message);
    }

    public void Error(Exception exception, string message)
    {
        _logger.LogError(exception, "{Message}", message);
    }
}

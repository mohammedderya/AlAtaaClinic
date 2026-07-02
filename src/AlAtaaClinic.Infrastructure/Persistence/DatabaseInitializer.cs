using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlAtaaClinic.Infrastructure.Persistence;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ClinicDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        ClinicDbContext context,
        IServiceProvider serviceProvider,
        IAppLogger<DatabaseInitializer> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Applying database migrations.");
        await _context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        _logger.Information("Seeding initial data.");
        await SeedData.InitializeAsync(_serviceProvider).ConfigureAwait(false);

        _logger.Information("Database initialization completed.");
    }
}

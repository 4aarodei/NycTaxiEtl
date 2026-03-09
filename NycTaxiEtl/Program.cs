using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Infrastructure.DependencyInjection;
using TaxiEtl.Shared.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.Configure<TaxiEtlOptions>(
    builder.Configuration.GetSection(TaxiEtlOptions.SectionName));

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var host = builder.Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var logger = services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Program");

var importService = services.GetRequiredService<ITaxiRideImportService>();
var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<TaxiEtlOptions>>().Value;

var inputFilePath = options.InputCsvPath;
var duplicatesFilePath = options.DuplicatesCsvPath;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

logger.LogInformation("Import started.");

var insertedRows = await importService.RunAsync(
    inputFilePath,
    duplicatesFilePath,
    connectionString,
    options.BatchSize,
    CancellationToken.None);

logger.LogInformation("Import finished. Inserted rows: {InsertedRows}", insertedRows);
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Infrastructure.BulkInsert;
using TaxiEtl.Infrastructure.Csv;
using TaxiEtl.Infrastructure.Database;
using TaxiEtl.Shared.Configuration;

namespace TaxiEtl.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TaxiDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("NycTaxiEtl")));

        services.AddSingleton<ITaxiRideTransformer>(serviceProvider =>
        {
            var opts = serviceProvider.GetRequiredService<IOptions<TaxiEtlOptions>>().Value;
            return new TaxiRideTransformer(opts);
        });

        services.AddSingleton<IDeduplicationService, DeduplicationService>();
        services.AddSingleton<ICsvTaxiRideReader, CsvTaxiRideReader>();
        services.AddSingleton<IBulkInsertService, SqlBulkInsertService>();
        services.AddSingleton<ITaxiRideImportService, TaxiRideImportService>();

        return services;
    }
}
using Codebasky.Application.Common.Abstractions;
using Codebasky.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codebasky.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Codebasky")
            ?? "Data Source=codebasky.db";

        services.AddDbContext<CodebaskyDbContext>(options =>
        {
            if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
                return;
            }

            options.UseSqlite(connectionString);
        });

        services.AddScoped<ICodebaskyDbContext>(provider => provider.GetRequiredService<CodebaskyDbContext>());
        services.AddScoped<CodebaskyDbContextInitialiser>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Codebasky.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<Services.SessionService>();
        services.AddScoped<Services.WorkspaceService>();
        services.AddScoped<Services.ProjectService>();
        services.AddScoped<Services.TaskService>();
        services.AddScoped<Services.CommentService>();
        services.AddScoped<Services.AnalyticsService>();
        services.AddScoped<Services.NotificationService>();
        return services;
    }
}

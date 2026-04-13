using Codebasky.Application.Common.Abstractions;
using Codebasky.Web.Auth;
using Codebasky.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace Codebasky.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSignalR();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "smart";
                options.DefaultChallengeScheme = "smart";
            })
            .AddPolicyScheme("smart", "Smart authentication", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var hasBearer = context.Request.Headers.Authorization.Any(header =>
                        header?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true);
                    return hasBearer ? JwtBearerDefaults.AuthenticationScheme : DebugHeaderAuthenticationHandler.SchemeName;
                };
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Auth0:Authority"];
                options.Audience = configuration["Auth0:Audience"];
            })
            .AddScheme<AuthenticationSchemeOptions, DebugHeaderAuthenticationHandler>(
                DebugHeaderAuthenticationHandler.SchemeName,
                _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy("WorkspaceRead", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("MemberWrite", policy => policy.RequireRole("Manager", "Member"))
            .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));

        services.AddCors(options =>
        {
            options.AddPolicy("frontend", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5173",
                        "http://127.0.0.1:5173",
                        "https://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Codebasky API",
                Version = "v1",
                Description = "Core MVP API for workspace, projects, tasks, comments, analytics and notifications.",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
            });
        });

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

        return services;
    }
}

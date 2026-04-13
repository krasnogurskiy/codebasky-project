using Codebasky.Application;
using Codebasky.Infrastructure;
using Codebasky.Infrastructure.Persistence;
using Codebasky.Web;
using Codebasky.Web.Endpoints;
using Codebasky.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var initialiser = scope.ServiceProvider.GetRequiredService<CodebaskyDbContextInitialiser>();
    await initialiser.InitialiseAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Codebasky API");
    options.RoutePrefix = "api";
});

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/api")).AllowAnonymous();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapCodebaskyApi();
app.MapHub<CodebaskyHub>("/hubs/codebasky");

app.Run();

public partial class Program;


using MoveLens.Api;
using MoveLens.Api.Extensions;
using MoveLens.API.Middlewares;
using MoveLens.Application;
using MoveLens.Infrastructure;
using MoveLens.Infrastructure.Persistence.Data.Initializer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddTransient<GlobalExceptionMiddleware>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();



var app = builder.Build();


await app.InitializeDatabaseAsync();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MoveLens API v1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();
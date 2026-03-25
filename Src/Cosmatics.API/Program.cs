using Cosmatics.Applicatiion;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using Cosmatics.Infrastructure;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.EntityFrameworkCore;
using Cosmatics.API.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

   builder.Services.AddInfrastructure( builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState
                .OrderBy(x => x.Key == "CountryCode" ? 0 : x.Key == "PhoneNumber" ? 1 : x.Key == "Password" ? 2 : 3)
                .SelectMany(v => v.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            var result = new Microsoft.AspNetCore.Mvc.UnprocessableEntityObjectResult(new { message = firstError });
            result.ContentTypes.Add("application/json");
            return result;
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>    
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"  
                }
            },
            new string[] {}
        }
    });
});

// JWT Authentication
    var jwtKey = builder.Configuration.GetSection("Jwt:Key").Value
        ?? throw new InvalidOperationException("JWT Key is not configured");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero 
            };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
       
                if (context.Response.HasStarted) return Task.CompletedTask;

                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { message = "Token is required or invalid." });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { message = "You must be an admin to perform this action." });
            }
        };
    });
            builder.Services.AddAuthorization();
var app = builder.Build();

// Apply pending migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
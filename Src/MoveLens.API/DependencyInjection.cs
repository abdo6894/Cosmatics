
using Microsoft.OpenApi.Models;
using MoveLens.Api.Filters;
using MoveLens.Api.Services;
using MoveLens.Application.Common.Interfaces;

namespace MoveLens.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddControllers(options => options.Filters.Add<ValidationFilter>())
            .ConfigureApiBehaviorOptions(o =>
                o.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddSwaggerWithJwt();

        return services;
    }

    private static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MoveLens API",
                Version = "v1",
                Description = "Egyptian Transport & Entertainment Platform",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your token}",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        });
    }
}
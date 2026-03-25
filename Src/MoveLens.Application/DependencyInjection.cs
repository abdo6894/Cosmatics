using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MoveLens.Application.Common.Behaviours;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MoveLens.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
                cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
                //cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            });

            services.AddAutoMapper(cfg =>
             cfg.AddMaps(assembly));
            return services;
        }
    }

}

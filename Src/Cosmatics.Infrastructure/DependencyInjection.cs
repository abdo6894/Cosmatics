using Cosmatics.Application.Common;
using Cosmatics.Infrastructure.Background_Services;
using Cosmatics.Infrastructure.Persistense.Data;
using Cosmatics.Infrastructure.Persistense.Repository_Pattern;
using Cosmatics.Infrastructure.Services;
using Cosmatics.Infrastructure.Services.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cosmatics.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddHttpClient();
            services.AddScoped<ICacheService, DistributedCache>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ISlidersService, SlidersService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            services.AddScoped<StripeGateway>();
            services.AddScoped<PaymobGateway>();
            services.AddScoped<PayPalGateway>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisCache") ?? "localhost:6379";
                options.InstanceName = " Cosmatics:";
            });

            //services.AddMemoryCache(option => option.SizeLimit=100);
            return services;
        }
    }
}

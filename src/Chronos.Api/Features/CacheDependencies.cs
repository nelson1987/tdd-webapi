using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Mime;

namespace Chronos.Api.Features
{
    public static class CacheDependencies
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            return services;
        }
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("ConexaoRedis")!;
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "Redis-Dev";
                options.Configuration = connectionString;
            });
            services.AddHealthChecks()
                    .AddRedis(connectionString);
            return services;
        }
        public static IApplicationBuilder AddRedisHealthCheck(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/redisState",
                new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var jsonresult = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            statusApplication = report.Status.ToString(),
                            healthChecks = report.Entries.Select(x => new
                            {
                                checkName = x.Key,
                                state = Enum.GetName(typeof(HealthStatus), x.Value.Status)
                            })
                        });
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(jsonresult);
                    }
                });
            return app;
        }
    }
}
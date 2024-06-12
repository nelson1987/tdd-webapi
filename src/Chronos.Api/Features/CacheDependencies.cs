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
            string connectionString = configuration.GetConnectionString("ConexaoRedis");
            services.AddStackExchangeRedisCache(options => {
                options.InstanceName = "Redis-Dev";
                options.Configuration = connectionString;
            });
            return services;
        }
    }
}

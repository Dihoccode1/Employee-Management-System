using API.Configuration;
using BUS;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new DatabaseSettings(
            configuration["HRM_CONNECTION_STRING"]
            ?? configuration.GetConnectionString("HumanResourceManagement"));
        services.AddSingleton(settings);
        services.AddScoped(_ => ServiceFactory.CreateEmployeeService(settings.ConnectionString ?? string.Empty));
        return services;
    }
}

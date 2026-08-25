using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Infrastructure.Persistence;

namespace RoomFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<RoomFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRoomReadStore, RoomReadStore>();
        services.AddScoped<IRoomWriteStore, RoomWriteStore>();

        return services;
    }
}

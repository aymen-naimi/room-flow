using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Infrastructure.Persistence;
using RoomFlow.Infrastructure.Security;

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
        services.AddScoped<IUserWriteStore, UserWriteStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}

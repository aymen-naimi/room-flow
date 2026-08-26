using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Infrastructure.Persistence;
using RoomFlow.Infrastructure.Security;
using System.Text;

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
        services.AddScoped<IUserReadStore, UserReadStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Issuer)
                    && !string.IsNullOrWhiteSpace(options.Audience)
                    && Encoding.UTF8.GetByteCount(options.SigningKey) >= 32
                    && options.AccessTokenExpirationMinutes > 0
                    && options.RefreshTokenExpirationHours > 0,
                "Jwt:Issuer, Audience, SigningKey (>= 32 bytes), AccessTokenExpirationMinutes and RefreshTokenExpirationHours are required.")
            .ValidateOnStart();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        return services;
    }
}

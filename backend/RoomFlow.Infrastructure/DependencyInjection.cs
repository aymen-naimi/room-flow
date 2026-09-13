using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Messaging;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Infrastructure.Messaging;
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
        services.AddScoped<IBookingReadStore, BookingReadStore>();
        services.AddScoped<IBookingWriteStore, BookingWriteStore>();
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
        AddBookingEmailQueue(services, configuration);

        return services;
    }

    private static void AddBookingEmailQueue(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceBusOptions>().Bind(configuration.GetSection(ServiceBusOptions.SectionName));

        var namespaceName = configuration["ServiceBus:FullyQualifiedNamespace"];
        var connectionString = configuration["ServiceBus:ConnectionString"];
        if (string.IsNullOrWhiteSpace(namespaceName) && string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IBookingEmailQueue, NoOpBookingEmailQueue>();
            return;
        }

        services.AddSingleton(sp =>
        {
            var configured = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(configured.ConnectionString))
            {
                return new ServiceBusClient(configured.ConnectionString);
            }

            return new ServiceBusClient(configured.FullyQualifiedNamespace, new DefaultAzureCredential());
        });
        services.AddSingleton<IBookingEmailQueue, ServiceBusBookingEmailQueue>();
    }
}

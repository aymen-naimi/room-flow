using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RoomFlow.Application.Abstractions.Concurrency;
using RoomFlow.Application.Behaviors;
using RoomFlow.Application.Concurrency;

namespace RoomFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IRoomBookingLock, RoomBookingLock>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}

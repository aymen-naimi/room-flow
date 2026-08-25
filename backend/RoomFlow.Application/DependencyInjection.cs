using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RoomFlow.Application.Behaviors;

namespace RoomFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}

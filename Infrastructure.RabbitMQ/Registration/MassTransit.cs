using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.RabbitMQ.Registration
{
    public static class MassTransit
    {
        public static IReadOnlyDictionary<Type, string> ExchangeNamesForMessageTypes { get; private set; } = new Dictionary<Type, string>();

        public static void RegisterMassTransit(this IServiceCollection services, string connectionString, MassTransitConfigurator massTransitConfigurator)
        {
            services.AddMassTransit(busRegistrationConfigurator =>
            {
                busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
                {
                    rabbitMqBusFactoryConfigurator.Host(connectionString);

                    foreach (var configureMessage in massTransitConfigurator.ConfigureMessages)
                    {
                        configureMessage(busRegistrationConfigurator, busRegistrationContext, rabbitMqBusFactoryConfigurator);
                    }
                });
            });

            ExchangeNamesForMessageTypes = massTransitConfigurator.ExchangeNamesForMessageTypes;
        }
    }
}

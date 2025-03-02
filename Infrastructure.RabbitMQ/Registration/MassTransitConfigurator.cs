using MassTransit;
using RabbitMQ.Client;
using System;

namespace Infrastructure.RabbitMQ.Registration;

public sealed class MassTransitConfigurator
{
    public IEnumerable<Action<IBusRegistrationConfigurator, IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>> ConfigureMessages => _configureMessages;

    private readonly HashSet<Type> _messageTypes = [];

    private readonly List<Action<IBusRegistrationConfigurator, IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>> _configureMessages = [];

    public MassTransitConfigurator AddConsumer<TMessage, TConsumer>(string exchangeName) 
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>, new()
    {
        _configureMessages.Add((x, y, configurator) =>
        {
            RegisterMessageType<TMessage>(configurator);

            configurator.ReceiveEndpoint($"{typeof(TMessage).Name}.queue", endpoint =>
            {
                endpoint.ConfigureConsumeTopology = false;

                endpoint.Bind(exchangeName, binding =>
                {
                    binding.ExchangeType = ExchangeType.Fanout;
                });

                endpoint.Consumer<TConsumer>();
            });
        });

        return this;
    }

    public MassTransitConfigurator AddPublisher<TMessage>() 
        where TMessage : class
    {
        _configureMessages.Add((_, _, configurator) =>
        {
            RegisterMessageType<TMessage>(configurator);

            configurator.Publish<TMessage>(cfg =>
            {
                cfg.Exclude = true;
                cfg.ExchangeType = ExchangeType.Fanout;
            });
        });

        return this;
    }


    private void RegisterMessageType<TMessage>(IRabbitMqBusFactoryConfigurator configurator) 
        where TMessage : class
    {
        if (_messageTypes.Contains(typeof(TMessage)))
        {
            return;
        }

        configurator.Message<TMessage>(topologyConfigurator =>
        {              
            topologyConfigurator.SetEntityNameFormatter(new MessageEntityNameFormatter<TMessage>(new MessageNameFormatter()));
        });

        _messageTypes.Add(typeof(TMessage));
    }
}
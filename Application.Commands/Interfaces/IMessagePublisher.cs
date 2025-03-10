namespace Application.Commands.Interfaces;

public interface IMessagePublisher
{
    Task Publish<TMessage>(TMessage message) where TMessage : Domain.Common.Seedwork.Abstract.Event;
}
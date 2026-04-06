using Azure.Messaging.ServiceBus;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IMessageProcessingOutboundService
    {
        Task ProcessMessageEnvio(ServiceBusReceivedMessage message);

    }
}

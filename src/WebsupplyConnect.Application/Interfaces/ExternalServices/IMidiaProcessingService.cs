using Azure.Messaging.ServiceBus;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IMidiaProcessingService
    {
        Task ProcessMidiaUsuario(ServiceBusReceivedMessage media);
    }
}

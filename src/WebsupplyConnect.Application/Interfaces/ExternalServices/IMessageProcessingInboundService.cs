using Azure.Messaging.ServiceBus;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IMessageProcessingInboundService
    {
        /// <summary>
        /// Processa o conteúdo que está sendo consumido do azure bus
        /// </summary>
        /// <param name="message">
        /// Contém a mensagem recebida do Azure Service Bus e permite gerenciar o ciclo de 
        /// processamento, como concluir, abandonar ou mover a mensagem para a fila de dead-letter.
        /// </param>
        Task ProcessMessageCliente(ServiceBusReceivedMessage message);
    }
}

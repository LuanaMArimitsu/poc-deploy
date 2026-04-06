using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.WhatsAppMidiaEnvio;

public class WhatsAppMidiaOutboundFuntion(ILogger<WhatsAppMidiaOutboundFuntion> logger, IMidiaProcessingService midiaProcessingService)
{
    private readonly ILogger<WhatsAppMidiaOutboundFuntion> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMidiaProcessingService _midiaProcessingService = midiaProcessingService ?? throw new ArgumentNullException(nameof(midiaProcessingService));

    [Function(nameof(WhatsAppMidiaOutboundFuntion))]
    public async Task Run([ServiceBusTrigger("websupplyconnect.midiasoutbound", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            // Chama sua lógica de processamento
            await _midiaProcessingService.ProcessMidiaUsuario(message);

            // Marca a mensagem como processada
            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar a mensagem. ID: {id}", message.MessageId);

            // ⚠️ Opcional: Dead-letter ou Abandon
            await messageActions.DeadLetterMessageAsync(message, null, "ProcessingError", ex.Message);

            // Usar o Abandon caso seja um erro temporário, pois ele reenvia a mensagem para o bus e tenta processar novamente.
            //await messageActions.AbandonMessageAsync(message);
        }
    }
}
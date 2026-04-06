using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.EscalonamentoAutomatico;

public class EscalonamentoAutomaticoFunction(ILoggerFactory loggerFactory, IEscalonamentoAutomaticoService escalonamentoAutomaticoService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<EscalonamentoAutomaticoFunction>();
    private readonly IEscalonamentoAutomaticoService _escalonamentoAutomaticoService = escalonamentoAutomaticoService;

    [Function("EscalonamentoAutomatico")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {       
        try
        {
            await _escalonamentoAutomaticoService.ProcessarEscalonamento();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao tratar conversa com inativadade}");
        }
    }
}
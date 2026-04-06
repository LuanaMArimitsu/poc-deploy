using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.WhatsAppInatividade;

public class WhatsAppInatividadeFuction(ILoggerFactory loggerFactory, IWhatsAppInatividadeService whatsAppInatividadeService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<WhatsAppInatividadeFuction>();
    private readonly IWhatsAppInatividadeService _whatsAppInatividadeService = whatsAppInatividadeService;

    [Function("WhatsAppInatividade")]
    public async Task Run([TimerTrigger("0 0,5,10,15,20,25,30,35,40,45,50,55 * * * *")] TimerInfo myTimer)
    {
        var agora = TimeHelper.GetBrasiliaTime();

        _logger.LogWarning(
            "⏰ TIMER DISPAROU | " +
            "Hora: {Agora} | " +
            "Próxima execução: {Proxima} | " +
            "Última execução: {Ultima}",
            agora,
            myTimer.ScheduleStatus?.Next,
            myTimer.ScheduleStatus?.Last
        );

        try
        {
            await  _whatsAppInatividadeService.ProcessarInatividade();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao tratar conversa com inativadade}");
        }
    }
}

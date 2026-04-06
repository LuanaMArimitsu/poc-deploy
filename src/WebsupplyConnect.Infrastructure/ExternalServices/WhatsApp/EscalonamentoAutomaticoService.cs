using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class EscalonamentoAutomaticoService(ILogger<EscalonamentoAutomaticoService> logger, IConversaReaderService conversaReaderService, IMembroEquipeReaderService membroEquipeReaderService, INotificacaoClient notificacaoClient, IRedistribuicaoService redistribuicaoService, IUsuarioReaderService usuarioReaderService) : IEscalonamentoAutomaticoService
    {
        private readonly ILogger<EscalonamentoAutomaticoService> _logger = logger;
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService;
        private readonly IRedistribuicaoService _redistribuicaoService = redistribuicaoService;
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient;
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService;
        private const int TAMANHO_LOTE = 50; // Processa 50 conversas por vez
        private const int MAX_PARALELO = 5; // Máximo 5 mensagens simultâneas
        private const int DELAY_ENTRE_LOTES_MS = 200; // Pausa de 200ms entre lotes
        public async Task ProcessarEscalonamento()
        {
            await ProcessarEscalonamentosPendentes();
        }

        private async Task ProcessarEscalonamentosPendentes()
        {
            var totalProcessados = 0;
            var totalErros = 0;
            var pagina = 0;

            while (true)
            {
                var loteConversas = await _conversaReaderService
                    .GetConversasSemAtendimento(pagina, TAMANHO_LOTE);

                if (loteConversas == null || loteConversas.Count == 0)
                    break;

                var equipeIds = loteConversas
                    .Select(c => c.Lead?.Responsavel?.EquipeId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                if (equipeIds.Count == 0)
                {
                    pagina++;
                    continue;
                }

                var lideresPorEquipe = await _membroEquipeReaderService.ObterLideresDaEquipeAsync(equipeIds);

                foreach (var conversa in loteConversas)
                {
                    try
                    {
                        var equipeId = conversa.Lead?.Responsavel?.EquipeId;

                        if (!equipeId.HasValue)
                        {
                            _logger.LogWarning("Conversa {conversaId} sem equipe associada", conversa.Id);
                            continue;
                        }

                        if (!lideresPorEquipe.TryGetValue(equipeId.Value, out var lider))
                        {
                            _logger.LogWarning("Equipe {equipeId} sem líder configurado", equipeId.Value);
                            continue;
                        }

                        var usuarioHorario = await _usuarioReaderService.ObterHorariosUsuarioAsync(conversa.UsuarioId);
                        var agora = TimeHelper.GetBrasiliaTime();
                        var horaAtual = agora.TimeOfDay;

                        var diaSemanaId = agora.DayOfWeek switch
                        {
                            DayOfWeek.Sunday => 1,
                            DayOfWeek.Monday => 2,
                            DayOfWeek.Tuesday => 3,
                            DayOfWeek.Wednesday => 4,
                            DayOfWeek.Thursday => 5,
                            DayOfWeek.Friday => 6,
                            DayOfWeek.Saturday => 7,
                            _ => 0
                        };

                        var intervalosDoDia = (usuarioHorario ?? [])
                            .Where(h => h.DiaSemanaId == diaSemanaId)
                            .Where(h => !h.SemExpediente)
                            .Where(h => h.HorarioInicio.HasValue && h.HorarioFim.HasValue)
                            .ToList();

                        if (intervalosDoDia.Count == 0)
                        {
                            _logger.LogInformation(
                                "Conversa {conversaId}: usuário {usuarioId} sem horários para o dia {diaSemanaId}. Não será transferido.",
                                conversa.Id, conversa.UsuarioId, diaSemanaId);
                            continue;
                        }

                        bool dentroHorarioNormal = false;
                        bool emCarenciaInicioTurno = false;

                        foreach (var i in intervalosDoDia)
                        {
                            var ini = i.HorarioInicio!.Value;
                            var fim = i.HorarioFim!.Value;

                            // 1) Primeiro: precisa estar dentro do expediente
                            if (!EstaDentro(horaAtual, ini, fim))
                                continue;

                            // 2) Está dentro do expediente: aplica carência de 5 min no começo do turno
                            var carencia = TimeSpan.FromMinutes(5);
                            var limiteCarencia = ini + carencia;

                            if (horaAtual <= limiteCarencia)
                            {
                                emCarenciaInicioTurno = true;
                                break;
                            }

                            // 3) Está dentro do expediente e já passou da carência => pode transferir
                            dentroHorarioNormal = true;
                            break;
                        }

                        if (emCarenciaInicioTurno)
                        {
                            _logger.LogInformation(
                                "Conversa {conversaId}: responsável {usuarioId} nos primeiros 10 min do turno ({agora}). Não será transferido.",
                                conversa.Id, conversa.UsuarioId, agora);
                            continue;
                        }

                        if (!dentroHorarioNormal)
                        {
                            _logger.LogInformation(
                                "Conversa {conversaId}: responsável {usuarioId} fora do horário ({agora}). Não será transferido.",
                                conversa.Id, conversa.UsuarioId, agora);
                            continue;
                        }

                        var membros = await _membroEquipeReaderService
                            .ObterMembrosPorUsuarioAsync(conversa.UsuarioId, equipeId.Value);
                        var membro = membros.FirstOrDefault();

                        if (membro == null)
                        {
                            _logger.LogWarning(
                                "Conversa {conversaId} com responsável {usuarioId} que não é membro da equipe {equipeId}",
                                conversa.Id, conversa.UsuarioId, equipeId.Value);
                            continue;
                        }

                        await EscalonarParaResponsavel(
                            conversa.LeadId,
                            membro,
                            lider,
                            equipeId.Value,
                            conversa.Lead!.EmpresaId);

                        totalProcessados++;
                    }
                    catch (Exception ex)
                    {
                        totalErros++;
                        _logger.LogError(ex, "Erro ao escalonar conversa ID: {conversaId}", conversa.Id);
                    }
                }

                pagina++;
                await Task.Delay(DELAY_ENTRE_LOTES_MS);
            }

            _logger.LogInformation(
                "Escalonamento automático concluído. Total processados: {total}, Total erros: {erros}",
                totalProcessados, totalErros);
        }


        static bool EstaDentro(TimeSpan atual, TimeSpan ini, TimeSpan fim)
        {
            // Intervalo normal (ex: 08:00-18:00)
            if (fim >= ini)
                return atual >= ini && atual <= fim;

            // Intervalo atravessa meia-noite (ex: 22:00-02:00)
            return atual >= ini || atual <= fim;
        }

        private async Task EscalonarParaResponsavel(
            int leadId,
            MembroEquipe responsavelAnterior,
            MembroEquipe responsavel,
            int equipeId,
            int empresaId)
        {
            _logger.LogWarning("Dados enviados {leadId}, {responsavel}, {equipeid}, {empresaid}", leadId, responsavel.Id, equipeId, empresaId);
            await _redistribuicaoService.TransferirLeadSemOportunidadeAsync(leadId, responsavel.Id, equipeId, empresaId, true);

            var dtoLider = new NotificacaoEscalonamentoDTO() { LeadId = leadId, UsuarioId = responsavel.UsuarioId };
            await _notificacaoClient.EscalonamentoAutomaticoLider(dtoLider);

            var dtoVendedor = new NotificacaoEscalonamentoDTO() { LeadId = leadId, UsuarioId = responsavelAnterior.UsuarioId };
            await _notificacaoClient.EscalonamentoAutomaticoVendedor(dtoVendedor);
        }
    }
}

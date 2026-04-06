using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Helpers
{
    public static class NotificacaoEquipeHelper
    {
        /// <summary>
        /// Calcula o novo estado das notificações (destinatário, líderes e SLA)
        /// a partir do estado atual + valores enviados no PATCH (parciais).
        /// </summary>
        public static (
            bool NotificarDestinatario,
            bool NotificarLideres,
            bool NotificarSemAtendimentoLideres,
            TimeSpan? TempoMaxSemAtendimento
        ) CalcularNovoEstado(
            // estado atual (no banco)
            bool atualNotificarDestinatario,
            bool atualNotificarLideres,
            bool atualSlaAtivo,
            TimeSpan? atualTempoMaxSemAtendimento,
            // valores do PATCH (podem ser nulos)
            bool? patchNotificarDestinatario,
            bool? patchNotificarLideres,
            bool? patchSlaAtivo,
            int? patchHoras,
            int? patchMinutos)
        {
            // coalesce simples para os toggles "simples"
            var notificarDestinatario = patchNotificarDestinatario ?? atualNotificarDestinatario;
            var notificarLideres = patchNotificarLideres ?? atualNotificarLideres;
            var slaAtivo = patchSlaAtivo ?? atualSlaAtivo;
            var tempoMaxSemAtendimento = atualTempoMaxSemAtendimento;

            // 1) Toggle SLA veio TRUE => precisa de tempo (do PATCH ou mantém o atual)
            if (patchSlaAtivo == true)
            {
                var horas = patchHoras ?? tempoMaxSemAtendimento?.Hours ?? 0;
                var minutos = patchMinutos ?? tempoMaxSemAtendimento?.Minutes ?? 0;

                var novoTempo = TimeSpan.FromHours(horas) + TimeSpan.FromMinutes(minutos);
                if (novoTempo <= TimeSpan.Zero)
                    throw new DomainException("Defina horas/minutos válidos para a notificação por SLA (sem atendimento).");

                slaAtivo = true;
                tempoMaxSemAtendimento = novoTempo;
            }

            // 2) Toggle SLA veio FALSE => desliga e zera o tempo
            if (patchSlaAtivo == false)
            {
                slaAtivo = false;
                tempoMaxSemAtendimento = null;
            }

            // 3) Não veio toggle, mas veio tempo => só pode se SLA já está ativo
            var enviouTempo = patchHoras.HasValue || patchMinutos.HasValue;
            if (patchSlaAtivo is null && enviouTempo)
            {
                if (!slaAtivo)
                    throw new DomainException("Para definir horas/minutos, ative a notificação por SLA.");

                var horas = patchHoras ?? 0;
                var minutos = patchMinutos ?? 0;

                var novoTempo = TimeSpan.FromHours(horas) + TimeSpan.FromMinutes(minutos);
                if (novoTempo <= TimeSpan.Zero)
                    throw new DomainException("Defina horas/minutos válidos para a notificação por SLA (sem atendimento).");

                tempoMaxSemAtendimento = novoTempo;
            }

            return (
                NotificarDestinatario: notificarDestinatario,
                NotificarLideres: notificarLideres,
                NotificarSemAtendimentoLideres: slaAtivo,
                TempoMaxSemAtendimento: tempoMaxSemAtendimento
            );
        }
    }
}

using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Notificacao
{
    public class UsuarioNotificacaoConfiguracao : EntidadeBase
    {
        /// <summary>
        /// ID do usuário proprietário da configuração
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// Indica se o usuário deseja receber notificações push
        /// </summary>
        public bool ReceberPush { get; private set; }

        /// <summary>
        /// Indica se o usuário deseja receber notificações em tempo real via SignalR
        /// </summary>
        public bool ReceberSignalR { get; private set; }

        /// <summary>
        /// Indica se o usuário deseja receber notificações por email
        /// </summary>
        public bool ReceberEmail { get; private set; }

        /// <summary>
        /// Indica se o usuário deseja ver notificações na interface web
        /// </summary>
        public bool ExibirNaWeb { get; private set; }

        /// <summary>
        /// Horário de início para envio de notificações (formato HH:mm)
        /// </summary>
        public string HorarioInicio { get; private set; }

        /// <summary>
        /// Horário de fim para envio de notificações (formato HH:mm)
        /// </summary>
        public string HorarioFim { get; private set; }

        /// <summary>
        /// Indica se o usuário deseja receber notificações durante finais de semana
        /// </summary>
        public bool ReceberFinalSemana { get; private set; }

        /// <summary>
        /// Intervalo mínimo em minutos entre notificações para evitar spam
        /// </summary>
        public int IntervalMinimoMinutos { get; private set; }

        // Propriedade de navegação
        public virtual Usuario.Usuario Usuario { get; private set; }

        // Construtor para EF Core
        protected UsuarioNotificacaoConfiguracao() { }

        /// <summary>
        /// Construtor para criação de nova configuração de notificação
        /// </summary>
        public UsuarioNotificacaoConfiguracao(
            int usuarioId,
            bool receberPush = true,
            bool receberSignalR = true,
            bool receberEmail = false,
            bool exibirNaWeb = true,
            string horarioInicio = "08:00",
            string horarioFim = "18:00",
            bool receberFinalSemana = false,
            int intervalMinimoMinutos = 5)
        {
            if (usuarioId <= 0)
                throw new DomainException("Usuário ID deve ser maior que zero", nameof(UsuarioNotificacaoConfiguracao));

            ValidarHorarios(horarioInicio, horarioFim);
            ValidarIntervalMinimo(intervalMinimoMinutos);

            UsuarioId = usuarioId;
            ReceberPush = receberPush;
            ReceberSignalR = receberSignalR;
            ReceberEmail = receberEmail;
            ExibirNaWeb = exibirNaWeb;
            HorarioInicio = horarioInicio;
            HorarioFim = horarioFim;
            ReceberFinalSemana = receberFinalSemana;
            IntervalMinimoMinutos = intervalMinimoMinutos;
        }

        /// <summary>
        /// Atualiza as preferências de canais de notificação
        /// </summary>
        public void AtualizarPreferenciasCanais(
            bool receberPush,
            bool receberSignalR,
            bool receberEmail,
            bool exibirNaWeb)
        {
            ReceberPush = receberPush;
            ReceberSignalR = receberSignalR;
            ReceberEmail = receberEmail;
            ExibirNaWeb = exibirNaWeb;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza os horários de recebimento de notificações
        /// </summary>
        public void AtualizarHorarios(string horarioInicio, string horarioFim)
        {
            ValidarHorarios(horarioInicio, horarioFim);

            HorarioInicio = horarioInicio;
            HorarioFim = horarioFim;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza a configuração de final de semana
        /// </summary>
        public void AtualizarConfiguracaoFinalSemana(bool receberFinalSemana)
        {
            ReceberFinalSemana = receberFinalSemana;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o intervalo mínimo entre notificações
        /// </summary>
        public void AtualizarIntervalMinimo(int intervalMinimoMinutos)
        {
            ValidarIntervalMinimo(intervalMinimoMinutos);

            IntervalMinimoMinutos = intervalMinimoMinutos;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se o usuário deve receber notificações no horário atual
        /// </summary>
        public bool DeveReceberNotificacaoAgora()
        {
            var agora = DateTime.Now;

            // Verifica se é final de semana e se o usuário não quer receber
            if (!ReceberFinalSemana && (agora.DayOfWeek == DayOfWeek.Saturday || agora.DayOfWeek == DayOfWeek.Sunday))
                return false;

            // Verifica se está dentro do horário permitido
            if (TimeSpan.TryParse(HorarioInicio, out var inicio) && TimeSpan.TryParse(HorarioFim, out var fim))
            {
                var horarioAtual = agora.TimeOfDay;
                return horarioAtual >= inicio && horarioAtual <= fim;
            }

            return true; // Se não conseguir parsear os horários, permite o envio
        }

        private void ValidarHorarios(string horarioInicio, string horarioFim)
        {
            if (!TimeSpan.TryParse(horarioInicio, out var inicio))
                throw new DomainException("Horário de início inválido", nameof(UsuarioNotificacaoConfiguracao));

            if (!TimeSpan.TryParse(horarioFim, out var fim))
                throw new DomainException("Horário de fim inválido", nameof(UsuarioNotificacaoConfiguracao));

            if (inicio >= fim)
                throw new DomainException("Horário de início deve ser anterior ao horário de fim", nameof(UsuarioNotificacaoConfiguracao));
        }

        private void ValidarIntervalMinimo(int intervalMinimoMinutos)
        {
            if (intervalMinimoMinutos < 0)
                throw new DomainException("Intervalo mínimo não pode ser negativo", nameof(UsuarioNotificacaoConfiguracao));

            if (intervalMinimoMinutos > 1440) // 24 horas
                throw new DomainException("Intervalo mínimo não pode ser maior que 24 horas", nameof(UsuarioNotificacaoConfiguracao));
        }
    }
}

using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Usuario
{
    /// <summary>
    /// Entidade que representa um horário de trabalho de um usuário em um dia da semana
    /// </summary>
    public class UsuarioHorario 
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID do usuário
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// ID do dia da semana
        /// </summary>
        public int DiaSemanaId { get; private set; }

        /// <summary>
        /// Horário de início do expediente
        /// </summary>
        public TimeSpan HorarioInicio { get; private set; }

        /// <summary>
        /// Horário de fim do expediente
        /// </summary>
        public TimeSpan HorarioFim { get; private set; }

        /// <summary>
        /// Navegação para o usuário
        /// </summary>
        public virtual Usuario Usuario { get; private set; }

        /// <summary>
        /// Navegação para o dia da semana
        /// </summary>
        public virtual DiaSemana DiaSemana { get; private set; }

        public bool IsTolerancia { get; set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected UsuarioHorario()
        {
        }

        /// <summary>
        /// Construtor para criar um novo horário de trabalho
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="diaSemanaId">ID do dia da semana</param>
        /// <param name="horarioInicio">Horário de início do expediente</param>
        /// <param name="horarioFim">Horário de fim do expediente</param>
        public UsuarioHorario(int usuarioId, int diaSemanaId, TimeSpan horarioInicio, TimeSpan horarioFim)
        {
            ValidarDominio(usuarioId, diaSemanaId, horarioInicio, horarioFim);

            UsuarioId = usuarioId;
            DiaSemanaId = diaSemanaId;
            HorarioInicio = horarioInicio;
            HorarioFim = horarioFim;
        }

        /// <summary>
        /// Construtor para criar um horário sem expediente
        /// </summary>
        public UsuarioHorario(int usuarioId, int diaSemanaId)
        {
            ValidarDominio(usuarioId, diaSemanaId);

            UsuarioId = usuarioId;
            DiaSemanaId = diaSemanaId;
            HorarioInicio = TimeSpan.Zero;
            HorarioFim = TimeSpan.Zero;
        }

        /// <summary>
        /// Atualiza o horário de trabalho
        /// </summary>
        /// <param name="horarioInicio">Novo horário de início</param>
        /// <param name="horarioFim">Novo horário de fim</param>
        public void AtualizarHorario(TimeSpan horarioInicio, TimeSpan horarioFim)
        {
            if (horarioFim <= horarioInicio)
                throw new DomainException("O horário de fim deve ser posterior ao horário de início.", nameof(UsuarioHorario));

            HorarioInicio = horarioInicio;
            HorarioFim = horarioFim;

            IsTolerancia = false;
        }

        /// <summary>
        /// Verifica se um horário específico está dentro do expediente
        /// </summary>
        /// <param name="horario">Horário a ser verificado</param>
        /// <returns>True se está dentro do expediente, False caso contrário</returns>
        public bool EstaDentroDoExpediente(TimeSpan horario)
        {
            return horario >= HorarioInicio && horario <= HorarioFim;
        }

        /// <summary>
        /// Calcula a duração do expediente em horas
        /// </summary>
        /// <returns>Duração do expediente em horas</returns>
        public double CalcularDuracaoExpediente()
        {
            return (HorarioFim - HorarioInicio).TotalHours;
        }

        /// <summary>
        /// Valida as regras de domínio para o horário de trabalho
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="diaSemanaId">ID do dia da semana</param>
        /// <param name="horarioInicio">Horário de início</param>
        /// <param name="horarioFim">Horário de fim</param>
        private void ValidarDominio(int usuarioId, int diaSemanaId, TimeSpan horarioInicio, TimeSpan horarioFim)
        {
            if (usuarioId <= 0)
                throw new DomainException("O ID do usuário deve ser maior que zero.", nameof(UsuarioHorario));

            if (diaSemanaId <= 0 || diaSemanaId > 7)
                throw new DomainException("O ID do dia da semana deve estar entre 1 e 7.", nameof(UsuarioHorario));

            if (horarioFim <= horarioInicio)
                throw new DomainException("O horário de fim deve ser posterior ao horário de início.", nameof(UsuarioHorario));
        }

        /// <summary>
        /// Valida as regras de domínio comuns (sem horários)
        /// </summary>
        private void ValidarDominio(int usuarioId, int diaSemanaId)
        {
            if (usuarioId <= 0)
                throw new DomainException("O ID do usuário deve ser maior que zero.", nameof(UsuarioHorario));

            if (diaSemanaId <= 0 || diaSemanaId > 7)
                throw new DomainException("O ID do dia da semana deve estar entre 1 e 7.", nameof(UsuarioHorario));
        }

        public void DefinirTolerancia(bool ativo)
        {
            IsTolerancia = ativo;
        }
    }
}
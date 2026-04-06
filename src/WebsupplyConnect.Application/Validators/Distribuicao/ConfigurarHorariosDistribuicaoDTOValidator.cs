using FluentValidation;
using WebsupplyConnect.Application.DTOs.Distribuicao;

namespace WebsupplyConnect.Application.Validators.Distribuicao
{
    /// <summary>
    /// Validador para ConfigurarHorariosDistribuicaoDTO (SIMPLIFICADO)
    /// </summary>
    public class ConfigurarHorariosDistribuicaoDTOValidator : AbstractValidator<ConfigurarHorariosDistribuicaoDTO>
    {
        public ConfigurarHorariosDistribuicaoDTOValidator()
        {
            RuleFor(x => x.ConfiguracaoDistribuicaoId)
                .GreaterThan(0)
                .WithMessage("ID da configuração deve ser maior que zero");

            // Validação simplificada de horários
            RuleFor(x => x.HorarioInicioExpediente)
                .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
                .When(x => !string.IsNullOrEmpty(x.HorarioInicioExpediente))
                .WithMessage("Horário de início deve estar no formato HH:mm");

            RuleFor(x => x.HorarioFimExpediente)
                .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
                .When(x => !string.IsNullOrEmpty(x.HorarioFimExpediente))
                .WithMessage("Horário de fim deve estar no formato HH:mm");

            // Validação simplificada: horário de fim deve ser maior que início
            RuleFor(x => x)
                .Must(ValidarHorariosExpediente)
                .When(x => !string.IsNullOrEmpty(x.HorarioInicioExpediente) && !string.IsNullOrEmpty(x.HorarioFimExpediente))
                .WithMessage("Horário de fim deve ser maior que o horário de início");
        }

        private bool ValidarHorariosExpediente(ConfigurarHorariosDistribuicaoDTO configuracao)
        {
            if (string.IsNullOrEmpty(configuracao.HorarioInicioExpediente) || 
                string.IsNullOrEmpty(configuracao.HorarioFimExpediente))
                return true;

            if (TimeSpan.TryParse(configuracao.HorarioInicioExpediente, out var inicio) &&
                TimeSpan.TryParse(configuracao.HorarioFimExpediente, out var fim))
            {
                return inicio < fim;
            }

            return false;
        }
    }

}

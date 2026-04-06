using FluentValidation;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Validators.Usuario
{
    public class ConfigurarHorariosRequestDTOValidator : AbstractValidator<ConfigurarHorariosRequestDTO>
    {
        public ConfigurarHorariosRequestDTOValidator()
        {
            RuleFor(x => x.Horarios)
                .NotNull().WithMessage("A lista de horários é obrigatória.")
                .Must(h => h.Count == 7).WithMessage("Deve configurar exatamente os 7 dias da semana.");

            RuleFor(x => x.Horarios)
                .Must(NaoPossuiDiasDuplicados)
                .WithMessage("Não pode haver dias da semana duplicados.");

            RuleForEach(x => x.Horarios)
                .SetValidator(new HorarioTrabalhoDTOValidator());
        }

        private bool NaoPossuiDiasDuplicados(List<HorarioTrabalhoDTO> horarios)
        {
            if (horarios == null) return true;
            var duplicados = horarios.GroupBy(h => h.DiaSemanaId).Where(g => g.Count() > 1);
            return !duplicados.Any();
        }
    }
}

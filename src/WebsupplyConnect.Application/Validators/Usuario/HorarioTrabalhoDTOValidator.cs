using FluentValidation;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Validators.Usuario
{
    public class HorarioTrabalhoDTOValidator : AbstractValidator<HorarioTrabalhoDTO>
    {
        public HorarioTrabalhoDTOValidator()
        {
            RuleFor(x => x.DiaSemanaId)
                .InclusiveBetween(1, 7)
                .WithMessage("Dia da semana deve estar entre 1 (Domingo) e 7 (Sábado).");

            When(x => !x.SemExpediente, () =>
            {
                RuleFor(x => x.HorarioInicio)
                    .NotNull().WithMessage("Horário de início é obrigatório.")
                    .Must(h => h >= TimeSpan.Zero && h < TimeSpan.FromHours(24))
                    .WithMessage("Horário de início deve estar entre 00:00 e 23:59.");

                RuleFor(x => x.HorarioFim)
                    .NotNull().WithMessage("Horário de fim é obrigatório.")
                    .Must(h => h >= TimeSpan.Zero && h < TimeSpan.FromHours(24))
                    .WithMessage("Horário de fim deve estar entre 00:00 e 23:59.");

                RuleFor(x => x)
                    .Must(x => x.HorarioInicio < x.HorarioFim)
                    .WithMessage("Horário de fim deve ser posterior ao horário de início.");
            });
        }
    }
}

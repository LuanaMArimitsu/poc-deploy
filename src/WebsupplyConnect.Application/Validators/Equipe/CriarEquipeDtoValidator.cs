using FluentValidation;
using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Validators.Equipe
{
    public class CriarEquipeDtoValidator : AbstractValidator<CriarEquipeDto>
    {
        public CriarEquipeDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("Nome não pode ter mais que 100 caracteres");

            RuleFor(x => x.ResponsavelId)
                .GreaterThan(0)
                .WithMessage("O responsável deve ser informado.");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode ter mais que 500 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.Descricao));

            RuleFor(x => x.TipoEquipeId)
                .GreaterThan(0).WithMessage("Tipo de equipe é obrigatório");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("Empresa é obrigatória");

            RuleFor(x => x.TempoMaxSemAtendimento)
                .GreaterThanOrEqualTo(5)
                    .WithMessage("O tempo mínimo é de 5 minutos. Por favor, informe um tempo válido.");


            //When(x => x.NotificarSemAtendimentoLideres, () =>
            //{
            //    RuleFor(x => x.TempoSemAtendimentoHoras)
            //        .NotNull().WithMessage("Informe as horas para o tempo máximo sem atendimento")
            //        .InclusiveBetween(0, 24).WithMessage("Horas deve estar entre 0 e 24");

            //    RuleFor(x => x.TempoSemAtendimentoMinutos)
            //        .NotNull().WithMessage("Informe os minutos para o tempo máximo sem atendimento")
            //        .InclusiveBetween(0, 59).WithMessage("Minutos deve estar entre 0 e 59");

            //    RuleFor(x => x)
            //        .Must(x =>
            //        {
            //            if (x.TempoSemAtendimentoHoras is null || x.TempoSemAtendimentoMinutos is null)
            //                return false;

            //            var h = x.TempoSemAtendimentoHoras.Value;
            //            var m = x.TempoSemAtendimentoMinutos.Value;

            //            // não pode 00:00
            //            if (h == 0 && m == 0) return false;

            //            // limite 24:00
            //            if (h == 24 && m > 0) return false;

            //            return true;
            //        })
            //        .WithMessage("Tempo máximo sem atendimento deve ser > 00:00 e ≤ 24:00 (24:00 permite somente 24h e 0min).");
            //});
        }
    }
}
using FluentValidation;
using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Validators.Equipe
{
    public class AtualizarEquipeDtoValidator : AbstractValidator<AtualizarEquipeDto>
    {
        public AtualizarEquipeDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("Nome não pode ter mais que 100 caracteres");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode ter mais que 500 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
        }
    }
}
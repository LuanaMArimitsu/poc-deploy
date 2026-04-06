using FluentValidation;
using WebsupplyConnect.Application.DTOs.Lead;

namespace WebsupplyConnect.Application.Validators.Lead
{
    public class OrigemResquestDTOValidator : AbstractValidator<OrigemRequest>
    {
        public OrigemResquestDTOValidator() 
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome da origem é obrigatório.")
                .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");

            RuleFor(x => x.OrigemTipoId)
                .GreaterThan(0).WithMessage("Tipo de origem inválido.");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode exceder 500 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
        }
    }
}

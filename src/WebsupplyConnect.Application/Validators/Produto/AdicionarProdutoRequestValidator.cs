using FluentValidation;
using WebsupplyConnect.Application.DTOs.Produto;

namespace WebsupplyConnect.Application.Validators.Produto
{
    public class AdicionarProdutoRequestValidator : AbstractValidator<AdicionarProdutoRequestDTO>
    {
        public AdicionarProdutoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do produto é obrigatório.")
                .MaximumLength(200).WithMessage("O nome do produto não pode ter mais de 200 caracteres.");

            RuleFor(x => x.ValorReferencia)
                .GreaterThanOrEqualTo(0).When(x => x.ValorReferencia.HasValue)
                .WithMessage("O valor de referência não pode ser negativo.");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("O ID da empresa deve ser maior que zero.");
        }
    }
}

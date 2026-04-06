using FluentValidation;
using WebsupplyConnect.Application.DTOs.Produto;

namespace WebsupplyConnect.Application.Validators.Produto
{
    public class VincularEmpresaProdutoRequestValidator : AbstractValidator<VincularEmpresaProdutoRequestDTO>
    {
        public VincularEmpresaProdutoRequestValidator()
        {
            RuleFor(x => x.ProdutoId)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("O ID da empresa deve ser maior que zero.");

            RuleFor(x => x.ValorPersonalizado)
                .GreaterThanOrEqualTo(0).When(x => x.ValorPersonalizado.HasValue)
                .WithMessage("O valor personalizado não pode ser negativo.");
        }
    }
}

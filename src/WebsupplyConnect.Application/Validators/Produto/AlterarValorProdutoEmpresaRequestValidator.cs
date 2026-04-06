using FluentValidation;
using WebsupplyConnect.Application.DTOs.Produto;

namespace WebsupplyConnect.Application.Validators.Produto
{
    public class AlterarValorProdutoEmpresaRequestValidator : AbstractValidator<AlterarValorProdutoEmpresaRequestDTO>
    {
        public AlterarValorProdutoEmpresaRequestValidator()
        {
            RuleFor(x => x.NovoValor)
                .GreaterThanOrEqualTo(0).When(x => x.NovoValor.HasValue)
                .WithMessage("O valor personalizado não pode ser negativo.");
        }
    }
}

using FluentValidation;
using WebsupplyConnect.Application.DTOs.Produto;

namespace WebsupplyConnect.Application.Validators.Produto
{
    public class AtualizarProdutoRequestValidator : AbstractValidator<AtualizarProdutoRequestDTO>
    {
        public AtualizarProdutoRequestValidator()
        {
            RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do produto não pode ter mais de 200 caracteres.");

            RuleFor(x => x.Url)
                .MaximumLength(500).WithMessage("A URL não pode ter mais de 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Url));

            RuleFor(x => x.Descricao)
                .MaximumLength(1000).WithMessage("A Descrição não pode ter mais de 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descricao));
        }
    }
}

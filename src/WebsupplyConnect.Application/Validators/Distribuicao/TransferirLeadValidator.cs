using FluentValidation;
using WebsupplyConnect.Application.DTOs.Lead;

namespace WebsupplyConnect.Application.Validators.Distribuicao
{
    public class TransferirLeadValidator : AbstractValidator<LeadRedistribuicaoDTO>
    {
        public TransferirLeadValidator()
        {
            RuleFor(x => x.NovoResponsavelId)
                .GreaterThan(0)
                .WithMessage("O novo responsável deve ser informado.");

            RuleFor(x => x.EmpresaID)
                .GreaterThan(0)
                .WithMessage("A empresa deve ser informada.");
        }
    }
}

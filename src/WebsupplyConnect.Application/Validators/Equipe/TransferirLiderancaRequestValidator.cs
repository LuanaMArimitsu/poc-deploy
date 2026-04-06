using FluentValidation;
using WebsupplyConnect.Application.DTOs.Equipe;

public class TransferirLiderancaRequestValidator : AbstractValidator<TransferirLiderancaRequestDto>
{
    public TransferirLiderancaRequestValidator()
    {
        RuleFor(x => x.EquipeId).GreaterThan(0);
        RuleFor(x => x.NovoResponsavelMembroId).GreaterThan(0);
    }
}
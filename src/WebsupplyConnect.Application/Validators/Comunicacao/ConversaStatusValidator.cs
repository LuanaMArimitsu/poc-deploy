using FluentValidation;
using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Validators.Comunicacao
{

    public class ConversaStatusValidator : AbstractValidator<ConversaStatusDTO>
    {
        public ConversaStatusValidator()
        {
            RuleFor(x => x.ConversaID)
                .GreaterThan(0).WithMessage("Conversa id deve ser maior que zero.");

            RuleFor(x => x.StatusId)
                .GreaterThan(0).WithMessage("Status id deve ser maior que zero.");
        }
    }
}

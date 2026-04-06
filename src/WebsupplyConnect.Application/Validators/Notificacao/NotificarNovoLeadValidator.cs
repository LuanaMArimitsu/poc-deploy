using FluentValidation;
using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Validators.Notificacao
{
    public class NotificarNovoLeadValidator : AbstractValidator<NotificarNovoLeadDTO>
    {
        public NotificarNovoLeadValidator()
        {
            RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Usuário inválido.");
            RuleFor(x => x.LeadId).GreaterThan(0).WithMessage("Lead inválido.");
        }
    }
}

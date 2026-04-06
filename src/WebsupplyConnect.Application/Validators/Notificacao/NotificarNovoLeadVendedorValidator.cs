using FluentValidation;
using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Validators.Notificacao
{
    public class NotificarNovoLeadVendedorValidator : AbstractValidator<NotificarNovoLeadVendedorDTO>
    {
        public NotificarNovoLeadVendedorValidator()
        {
            RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Usuário inválido.");
            RuleFor(x => x.LeadId).GreaterThan(0).WithMessage("Lead inválido.");
            RuleFor(x => x.NomeVendedor).NotEmpty().WithMessage("Nome do vendedor é obrigatório.");
        }
    }
}

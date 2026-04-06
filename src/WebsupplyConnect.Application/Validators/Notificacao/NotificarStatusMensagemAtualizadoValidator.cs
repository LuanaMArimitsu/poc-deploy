using FluentValidation;
using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Validators.Notificacao
{
    public class NotificarStatusMensagemAtualizadoValidator : AbstractValidator<NotificarStatusMensagemAtualizadoDTO>
    {
        public NotificarStatusMensagemAtualizadoValidator()
        {
            RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Usuário inválido.");
            RuleFor(x => x.MensagemId).GreaterThan(0).WithMessage("Mensagem inválida.");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Status é obrigatório.")
                                  .MaximumLength(50).WithMessage("Status deve ter no máximo 50 caracteres.");
        }
    }
}

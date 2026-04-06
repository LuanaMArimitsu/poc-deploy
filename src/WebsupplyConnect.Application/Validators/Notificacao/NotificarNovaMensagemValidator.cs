using FluentValidation;
using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Validators.Notificacao
{
    public class NotificarNovaMensagemValidator : AbstractValidator<NotificarNovaMensagemDTO>
    {
        public NotificarNovaMensagemValidator()
        {
            RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Usuário inválido.");
            RuleFor(x => x.MensagemId).GreaterThan(0).WithMessage("Mensagem inválida.");
            RuleFor(x => x.Titulo).NotEmpty().WithMessage("Título obrigatório.")
                                  .MaximumLength(100).WithMessage("Título deve ter no máximo 100 caracteres.");
        }
    }
}

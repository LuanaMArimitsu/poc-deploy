using FluentValidation;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Validators.Usuario
{
    public class AdicionarDispositivoValidator : AbstractValidator<AdicionarDispositivoDTO>
    {
        public AdicionarDispositivoValidator()
        {

            RuleFor(x => x.Modelo)
                .MaximumLength(50)
                .WithMessage("Modelo deve ter no máximo 100 caracteres");

            RuleFor(x => x.DeviceId)
                .MaximumLength(300)
                .WithMessage("DeviceId deve ser informado");

            RuleFor(x => x.UsuarioId)
                .GreaterThan(0)
                .WithMessage("UsuarioId deve ser informada");
        }
    }
}

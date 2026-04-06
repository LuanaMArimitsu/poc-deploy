using FluentValidation;
using Microsoft.AspNetCore.Http;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Validators.Usuario
{
    public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequestDTO>
    {
        public AtualizarUsuarioRequestValidator()
        {
            RuleFor(x => x.Cargo)
            .MaximumLength(100)
            .WithMessage("Cargo deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Departamento)
            .MaximumLength(100)
            .WithMessage("Departamento deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Ativo)
            .NotNull()
            .WithMessage("Status ativo é obrigatório.");
        }
    }
}

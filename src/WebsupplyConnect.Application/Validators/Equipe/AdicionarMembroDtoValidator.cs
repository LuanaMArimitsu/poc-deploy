using FluentValidation;
using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Validators.Equipe
{
    public class AdicionarMembroDtoValidator : AbstractValidator<AdicionarMembroDto>
    {
        private static readonly HashSet<int> StatusValidos = new() { 94, 95, 96, 97, 98 };

        public AdicionarMembroDtoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0)
                .WithMessage("Usuário é obrigatório.");

            RuleFor(x => x.StatusMembroEquipeId)
                .Must(id => StatusValidos.Contains(id))
                .WithMessage("Status inválido. Utilize um dos IDs 94 a 98.");

            RuleFor(x => x.Observacoes)
                .MaximumLength(1000)
                .WithMessage("Observações não podem ter mais que 1000 caracteres.");
        }
    }
}

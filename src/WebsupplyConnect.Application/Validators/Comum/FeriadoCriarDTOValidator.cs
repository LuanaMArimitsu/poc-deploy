using FluentValidation;
using System;
using WebsupplyConnect.Application.DTOs.Comum;

namespace WebsupplyConnect.Application.Validators.Comum
{
    /// <summary>
    /// Validador para o DTO de criação de Feriado
    /// </summary>
    public class FeriadoCriarDTOValidator : AbstractValidator<FeriadoCriarDTO>
    {
        /// <summary>
        /// Construtor com regras de validação
        /// </summary>
        public FeriadoCriarDTOValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do feriado é obrigatório")
                .MaximumLength(100).WithMessage("O nome do feriado deve ter no máximo 100 caracteres");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("A data do feriado é obrigatória")
                .Must(BeAValidDate).WithMessage("A data do feriado é inválida");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("O tipo do feriado é obrigatório")
                .MaximumLength(20).WithMessage("O tipo do feriado deve ter no máximo 20 caracteres")
                .Must(BeAValidTipo).WithMessage("Tipo de feriado inválido. Os tipos válidos são: Nacional, Estadual, Municipal ou Empresa");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");

            RuleFor(x => x.UF)
                .Must((dto, uf) => string.IsNullOrEmpty(uf) || uf.Length == 2)
                .WithMessage("O código UF deve ter 2 caracteres")
                .When(x => x.Tipo?.Equals("Estadual", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.UF)
                .NotEmpty().WithMessage("Para feriados estaduais, o campo UF é obrigatório")
                .When(x => x.Tipo?.Equals("Estadual", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.CodigoMunicipio)
                .NotEmpty().WithMessage("Para feriados municipais, o campo Código do Município é obrigatório")
                .When(x => x.Tipo?.Equals("Municipal", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.EmpresaId)
                .NotNull().WithMessage("Para feriados de empresa, o campo EmpresaId é obrigatório")
                .When(x => x.Tipo?.Equals("Empresa", StringComparison.OrdinalIgnoreCase) == true);
        }

        private bool BeAValidDate(DateTime date)
        {
            return date != default;
        }

        private bool BeAValidTipo(string tipo)
        {
            if (string.IsNullOrEmpty(tipo))
                return false;

            return tipo.Equals("Nacional", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Estadual", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Municipal", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Empresa", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Validador para o DTO de atualização de Feriado
    /// </summary>
    public class FeriadoAtualizarDTOValidator : AbstractValidator<FeriadoAtualizarDTO>
    {
        /// <summary>
        /// Construtor com regras de validação
        /// </summary>
        public FeriadoAtualizarDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("O ID do feriado deve ser maior que zero");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do feriado é obrigatório")
                .MaximumLength(100).WithMessage("O nome do feriado deve ter no máximo 100 caracteres");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("A data do feriado é obrigatória")
                .Must(BeAValidDate).WithMessage("A data do feriado é inválida");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("O tipo do feriado é obrigatório")
                .MaximumLength(20).WithMessage("O tipo do feriado deve ter no máximo 20 caracteres")
                .Must(BeAValidTipo).WithMessage("Tipo de feriado inválido. Os tipos válidos são: Nacional, Estadual, Municipal ou Empresa");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");

            RuleFor(x => x.UF)
                .Must((dto, uf) => string.IsNullOrEmpty(uf) || uf.Length == 2)
                .WithMessage("O código UF deve ter 2 caracteres")
                .When(x => x.Tipo?.Equals("Estadual", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.UF)
                .NotEmpty().WithMessage("Para feriados estaduais, o campo UF é obrigatório")
                .When(x => x.Tipo?.Equals("Estadual", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.CodigoMunicipio)
                .NotEmpty().WithMessage("Para feriados municipais, o campo Código do Município é obrigatório")
                .When(x => x.Tipo?.Equals("Municipal", StringComparison.OrdinalIgnoreCase) == true);

            RuleFor(x => x.EmpresaId)
                .NotNull().WithMessage("Para feriados de empresa, o campo EmpresaId é obrigatório")
                .When(x => x.Tipo?.Equals("Empresa", StringComparison.OrdinalIgnoreCase) == true);
        }

        private bool BeAValidDate(DateTime date)
        {
            return date != default;
        }

        private bool BeAValidTipo(string tipo)
        {
            if (string.IsNullOrEmpty(tipo))
                return false;

            return tipo.Equals("Nacional", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Estadual", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Municipal", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Empresa", StringComparison.OrdinalIgnoreCase);
        }
    }
}
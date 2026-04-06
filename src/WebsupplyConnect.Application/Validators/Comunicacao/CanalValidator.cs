using FluentValidation;
using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Validators.Comunicacao
{
    /// <summary>
    /// Validators para todas as operações relacionadas ao Canal
    /// </summary>
    public class CanalValidator : AbstractValidator<CreateCanalDTO>
    {
        /// <summary>
        /// Validator para criação de Canal
        /// </summary>

        public CanalValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty()
                .WithMessage("Nome do canal é obrigatório")
                .MaximumLength(100)
                .WithMessage("Nome do canal deve ter no máximo 100 caracteres")
                .Matches(@"^[a-zA-Z\s\-_]+$")
                .WithMessage("Nome do canal deve conter apenas letras, espaços e hífens (sem números)");

            RuleFor(x => x.Descricao)
                .MaximumLength(500)
                .WithMessage("Descrição deve ter no máximo 500 caracteres");

            RuleFor(x => x.CanalTipoId)
                .GreaterThan(0)
                .WithMessage("Tipo do canal deve ser informado");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0)
                .WithMessage("Empresa deve ser informada");

            RuleFor(x => x.OrigemPadraoId)
                .GreaterThan(0)
                .WithMessage("Origem Padrão deve ser informada");

            RuleFor(x => x.LimiteDiario)
                .GreaterThanOrEqualTo(0)
                .When(x => x.LimiteDiario.HasValue)
                .WithMessage("Limite diário deve ser maior ou igual a zero")
                .LessThanOrEqualTo(10000)
                .When(x => x.LimiteDiario.HasValue)
                .WithMessage("Limite diário não pode exceder 10.000 mensagens");

            RuleFor(x => x.WhatsAppNumero)
                .Matches(@"^\d{10,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.WhatsAppNumero))
                .WithMessage("Número do WhatsApp deve conter apenas dígitos e ter entre 10 e 15 caracteres");

            RuleFor(x => x.ConfiguracaoIntegracao)
                .Must(BeValidJsonConfiguration)
                .When(x => !string.IsNullOrWhiteSpace(x.ConfiguracaoIntegracao))
                .WithMessage("Configuração de integração deve ser um JSON válido");
        }

        /// <summary>
        /// Valida se a configuração é um JSON válido
        /// </summary>
        private bool BeValidJsonConfiguration(string configuracao)
        {
            if (string.IsNullOrEmpty(configuracao))
                return true; // Será validado pela regra NotEmpty se obrigatório

            try
            {
                System.Text.Json.JsonDocument.Parse(configuracao);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}


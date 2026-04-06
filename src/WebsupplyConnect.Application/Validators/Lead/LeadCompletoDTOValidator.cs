using FluentValidation;
using WebsupplyConnect.Application.DTOs.Lead;
using System.Text.RegularExpressions;

namespace WebsupplyConnect.Application.Validators.Lead
{
    public class LeadCompletoValidator : AbstractValidator<LeadCompletoDTO>
    {
        public LeadCompletoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome do lead é obrigatório.")
                .MaximumLength(100).WithMessage("Nome do lead deve ter no máximo 100 caracteres.");
            
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("EmpresaId deve ser maior que zero.");

            RuleFor(x => x.OrigemId)
                .GreaterThan(0).WithMessage("OrigemId deve ser maior que zero.");

            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.Email) || !string.IsNullOrWhiteSpace(dto.WhatsappNumero))
                .WithMessage("E-mail ou Whatsapp deve ser informado");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Email inválido.");

            //RuleFor(x => x.WhatsappNumero)
            //  .MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.WhatsappNumero))
            //  .WithMessage("Número de WhatsApp deve ter no máximo 20 caracteres.")
            //  .Must(numero =>
            //  {
            //      if (string.IsNullOrWhiteSpace(numero))
            //          return true;

            //      var digits = new string(numero.Where(char.IsDigit).ToArray());

            //      // Brasil: DDI 55 + DDD (2) + número (8 ou 9 dígitos)
            //      return (digits.Length == 12 || digits.Length == 13) && digits.StartsWith("55");
            //  })
            //  .WithMessage("Número de WhatsApp deve conter o DDI do Brasil (55), DDD (2 dígitos) e número com 8 ou 9 dígitos.");

            RuleFor(x => x.Telefone)
                .MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Telefone))
                .WithMessage("Número de telefone fixo deve ter no máximo 10 dígitos (DDD + número).")
                .Must(numero =>
                {
                    if (string.IsNullOrWhiteSpace(numero))
                        return true;

                    var digits = new string(numero.Where(char.IsDigit).ToArray());

                    // Deve ter exatamente 10 dígitos
                    if (digits.Length != 10)
                        return false;

                    // Primeiro dígito após o DDD
                    char primeiraCasa = digits[2];

                    // Fixo válido: começa com 2, 3, 4, 5, 7 ou 8
                    return primeiraCasa is '2' or '3' or '4' or '5' or '7' or '8';
                })
                .WithMessage("Número de telefone fixo deve conter DDD (2 dígitos) e número fixo (8 dígitos iniciando em 2, 3, 4, 5, 7 ou 8).");

            RuleFor(x => x.Genero)
                .Must(g => string.IsNullOrWhiteSpace(g) || g == "F" || g == "M")
                .WithMessage("Gênero deve ser 'F' ou 'M'.");

            RuleFor(x => x.CPF)
                .Must(cpf => string.IsNullOrWhiteSpace(cpf) || Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$"))
                .WithMessage("CPF deve estar no formato 000.000.000-00 ou 11 dígitos.");

            RuleFor(x => x.CNPJEmpresa)
                .Must(cnpj => string.IsNullOrWhiteSpace(cnpj) || Regex.IsMatch(cnpj, @"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$|^\d{14}$"))
                .WithMessage("CNPJ deve estar no formato 00.000.000/0000-00 ou 14 dígitos.");
        }
    }
}

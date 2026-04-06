using FluentValidation;
using WebsupplyConnect.Application.DTOs.Oportunidade;

namespace WebsupplyConnect.Application.Validators.Oportunidade
{
    public class CreateOportunidadeDTOValidator : AbstractValidator<CreateOportunidadeDTO>
    {
        public CreateOportunidadeDTOValidator()
        {
            RuleFor(x => x.LeadId)
                .GreaterThan(0).WithMessage("ID do lead é obrigatório");

            RuleFor(x => x.ProdutoId)
                .GreaterThan(0).WithMessage("ID do produto é obrigatório");

            RuleFor(x => x.EtapaId)
                .GreaterThan(0).WithMessage("ID da etapa é obrigatório");

            RuleFor(x => x.OrigemId)
                .GreaterThan(0).WithMessage("ID da origem é obrigatório");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("ID da empresa é obrigatório");
        }
    }
}
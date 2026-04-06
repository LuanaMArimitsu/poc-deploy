using FluentValidation;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Validators.Usuario
{
    public class AtualizarEmpresaUsuarioValidator : AbstractValidator<List<EmpresaVinculoDTO>>
    {
        public AtualizarEmpresaUsuarioValidator()
        {
            RuleFor(vinculos => vinculos)
                .NotEmpty()
                .WithMessage("A lista de vínculos não pode estar vazia.");

            RuleFor(vinculos => vinculos)
                .Must(v => v.Select(x => x.EmpresaId).Distinct().Count() == v.Count)
                .WithMessage("A lista contém empresas duplicadas.");

            RuleFor(vinculos => vinculos)
                .Must(v => v.Count(x => x.EhPrincipal == true) == 1)
                .WithMessage("Deve haver exatamente uma empresa principal.");

            RuleForEach(vinculos => vinculos)
                .SetValidator(new EmpresaVinculoValidator());
        }
    }

    public class EmpresaVinculoValidator : AbstractValidator<EmpresaVinculoDTO>
    {
        public EmpresaVinculoValidator()
        {
            RuleFor(x => x.EmpresaId)
                .GreaterThan(0)
                .WithMessage("ID da empresa deve ser maior que zero.");

            RuleFor(x => x.EhPrincipal)
                .NotNull()
                .WithMessage("É necessário informar se a empresa é principal.");

            RuleFor(x => x.CanalPadraoId)
                .GreaterThan(0)
                .WithMessage("O ID do canal padrão deve ser maior que zero.");
        }
    }

    public class AtualizarVinculosRequestValidator : AbstractValidator<AtualizarVinculosRequestDTO>
    {
        public AtualizarVinculosRequestValidator()
        {
            RuleFor(x => x.EmpresasVinculos)
                .SetValidator(new AtualizarEmpresaUsuarioValidator());
        }
    }
}


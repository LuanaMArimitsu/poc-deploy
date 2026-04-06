using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.ControleSistemasExternos
{
    public interface ISistemaExternoRepository : IBaseRepository
    {
        Task<SistemaExterno> GetSistemaExterno(string nome);
        Task<SistemaExterno?> GetSistemaExternoPorCredenciais(string nome, string cnpj);
    }
}

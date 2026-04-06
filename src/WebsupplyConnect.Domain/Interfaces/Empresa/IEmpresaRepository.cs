using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Empresa
{
    public interface IEmpresaRepository : IBaseRepository
    {
        Task<bool> EmpresaExisteAsync(int empresaId);
        Task<bool> ExistemEmpresasAtivasAsync(List<int> empresasIds);
        IQueryable<WebsupplyConnect.Domain.Entities.Empresa.Empresa> Query();
        Task<Domain.Entities.Empresa.Empresa?> ObterPorIdAsync(int empresaId);
        Task<List<Domain.Entities.Empresa.Empresa>> ListarEmpresasAtivasAsync();
        Task<Domain.Entities.Empresa.Empresa> GetGrupoEmpresaByEmpresaId(int empresaId);
        Task<List<Domain.Entities.Empresa.Empresa>> GetFiliais(int emgrupoEmpresaIdpresaId);
        Task<Domain.Entities.Empresa.Empresa> GetEmpresaPorCnpjAsync(string cnpjEmpresa);
    }
}

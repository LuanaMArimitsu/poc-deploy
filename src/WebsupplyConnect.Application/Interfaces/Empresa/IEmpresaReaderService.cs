using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Empresa;

namespace WebsupplyConnect.Application.Interfaces.Empresa
{
    public interface IEmpresaReaderService
    {
        Task<EmpresaComCanaisResponseDTO?> ObterEmpresaComCanaisAsync(int empresaId);
        Task<Domain.Entities.Empresa.Empresa?> ObterPorCnpjAsync(string cnpj);
        Task<List<EmpresaComCanaisResponseDTO>> ObterEmpresasComCanaisAsync();
        Task<bool> EmpresaExistsAsync(int id);
        Task<string?> GetConfiguracaoIntegracao(int empresaId);
        Task<List<EmpresaListagemDTO>> ObterTodasEmpresasAsync();
        Task<Domain.Entities.Empresa.Empresa> ObterPorId(int empresaId);
        Task<bool> ExistemEmpresasAtivasAsync(List<int> empresaIds);
        Task<GrupoEmpresaDTO> GetGrupoEmpresaByEmpresaId(int empresaId);
        Task<List<BranchesDTO>> GetFiliasAsync(int grupoEmpresaId);
        Task<Domain.Entities.Empresa.Empresa?> GetEmpresaPorCnpjAsync(string cnpjEmpresa);
        /// <summary>Lista empresas não excluídas para uso em ETL (sincronização dimensões).</summary>
        Task<List<Domain.Entities.Empresa.Empresa>> ObterTodasNaoExcluidasParaETLAsync();
    }
}

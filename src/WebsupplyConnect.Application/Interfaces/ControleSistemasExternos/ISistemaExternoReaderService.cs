using WebsupplyConnect.Application.DTOs.ControleIntegracoes;

namespace WebsupplyConnect.Application.Interfaces.ControleSistemasExternos
{
    public interface ISistemaExternoReaderService
    {
        Task<SistemaExternoIntegradorDTO> GetSistemaExternoIntegradorData(string nome);
        Task<SistemaExternoIntegradorDTO> GetSistemaExternoOlxPorCredenciais(string nome, string cnpj);
    }
}

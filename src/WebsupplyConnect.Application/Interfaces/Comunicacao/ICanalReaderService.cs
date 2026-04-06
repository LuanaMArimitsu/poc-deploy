using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface ICanalReaderService
    {
        /// <summary>
        /// Método para listar os canais existentes
        /// </summary>
        Task<List<Canal>> List();
        /// <summary>
        /// Método para buscar o canal a partir do ID
        /// </summary>
        Task<Canal?> GetCanalByIdAsync(int canalId);
        CanalConfigDTO? ObterConfiguracaoMeta(Canal canal);
        Task<List<Canal>> GetListCanaisById(List<int> canalIds);
        Task<List<string>> GetlistaConfiguracaoIntegracao();
        Task<List<Canal>> GetListCanaisByWhatsAppNumber(string numeroWhatsApp);
        Task<Canal?> GetCanalByEmpresaId(int empresaId);
    }
}

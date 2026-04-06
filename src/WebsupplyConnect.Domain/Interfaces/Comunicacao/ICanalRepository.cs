using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface ICanalRepository : IBaseRepository
    {
        /// <summary>
        /// Obtém um canal existente a partir do número de WhatsApp informado.
        /// </summary>
        /// <param name="whatsAppNumber">Número de WhatsApp a ser consultado.</param>
        /// <returns>Retorna o canal correspondente ou null se não for encontrado.</returns>
        Task<Canal?> GetCanalByWhatsAppNumberAsync(string whatsAppNumber);

        /// <summary>
        /// Verifica se já existe um canal com o nome informado.
        /// </summary>
        /// <param name="channelName">Nome do canal a ser verificado.</param>
        /// <returns>Retorna true se já existir um canal com esse nome; caso contrário, false.</returns>
        Task<bool> CanalNameExistsAsync(string channelName);

        /// <summary>
        /// Lista canais do sistema com filtros opcionais de empresa e status ativo.
        /// </summary>
        /// <param name="empresaId">ID da empresa para filtrar os canais (opcional).</param>
        /// <param name="ativo">Status ativo do canal para filtrar (opcional).</param>
        /// <returns>Lista de canais ordenada por nome que atendem aos critérios especificados.</returns>
        Task<List<Canal>> ListCanaisAsync(int? empresaId = null, bool? ativo = null);

        /// <summary>
        /// Recupera o canal a partir do id.
        /// </summary>
        /// <param name="canalId">Id do canal</param>
        /// <returns>O canal.</returns>
        Task<Canal?> GetCanalAsync(int canalId);
        Task<bool> ExistemCanaisAsync(List<int> canalIds);
        Task<List<Canal>> ListarCanaisPorEmpresasAsync(List<int> empresaIds);
        Task<List<Canal>> ObterCanaisPorIdsAsync(List<int> canalIds);
        Task<List<string>> GetConfiguracaoIntegracao();
        Task<List<Canal>> GetListCanaisByWhatsAppNumber(string whatsAppNumber);
        Task<Canal?> GetCanalByEmpresaId(int empresaId);
    }
}

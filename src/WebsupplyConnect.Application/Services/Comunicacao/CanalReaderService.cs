using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class CanalReaderService(ICanalRepository canalRepository, ILogger<CanalReaderService> logger) : ICanalReaderService
    {
        private readonly ICanalRepository _canalRepository = canalRepository ?? throw new ArgumentNullException(nameof(canalRepository));
        private readonly ILogger<CanalReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Método para listar canais
        /// </summary>
        public async Task<List<Canal>> List()
        {
            try
            {
                var canais = await _canalRepository.ListCanaisAsync();
                return canais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar canais");
                throw;
            }

        }

        public Task<List<Canal>> GetListCanaisById(List<int> canalIds)
        {
            try
            {
                if (canalIds == null || canalIds.Count == 0)
                {
                    return Task.FromResult(new List<Canal>());
                }
                return _canalRepository.ObterCanaisPorIdsAsync(canalIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter canais por IDs");
                throw;
            }
        }

        /// <summary>
        /// Método para buscar o canal a partir do ID.
        /// </summary>
        public async Task<Canal?> GetCanalByIdAsync(int canalId)
        {
            try
            {
                return await _canalRepository.GetCanalAsync(canalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar canal com o id: {id}", canalId);
                throw;
            }
        }

        public CanalConfigDTO? ObterConfiguracaoMeta(Canal canal)
        {
            if (string.IsNullOrWhiteSpace(canal.ConfiguracaoIntegracao))
                return null;
            try
            {
                return JsonSerializer.Deserialize<CanalConfigDTO>(canal.ConfiguracaoIntegracao);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao desserializar configuração do canal {canalNome}.", canal.Nome);
                throw;
            }
        }

        public Task<List<string>> GetlistaConfiguracaoIntegracao()
        {
            return _canalRepository.GetConfiguracaoIntegracao();
        }

        public async Task<List<Canal>> GetListCanaisByWhatsAppNumber(string numeroWhatsApp)
        {
            try
            {
                return await _canalRepository.GetListCanaisByWhatsAppNumber(numeroWhatsApp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter empresa por canal com o número de telefone: {telefone}", numeroWhatsApp);
                throw;
            }
        }

        public async Task<Canal?> GetCanalByEmpresaId(int empresaId)
        {
            try
            {
                return await _canalRepository.GetCanalByEmpresaId(empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter canal por empresa com o id: {empresaId}", empresaId);
                throw;
            }

        }
    }
}

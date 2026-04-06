using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Interfaces.Empresa;

namespace WebsupplyConnect.Application.Services.Empresa
{
    public class PromptEmpresasReaderService(ILogger<PromptEmpresasReaderService> logger, IPromptEmpresaRepository promptEmpresaRepository)
        : IPromptEmpresasReaderService
    {
        private readonly ILogger<PromptEmpresasReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IPromptEmpresaRepository _promptEmpresaRepository = promptEmpresaRepository ?? throw new ArgumentNullException(nameof(promptEmpresaRepository));
        public async Task<PromptEmpresas?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            try
            {
                return await _promptEmpresaRepository.GetByIdAsync(id, includeDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar PromptEmpresa por ID: {Id}", id);
                throw;
            }
        }

        public async Task<string?> GetPromptAsync(int empresaId, bool sistema, string tipoPrompt, bool includeDeleted = false)
        {
            try
            {
                if (empresaId <= 0)
                    return null;

                if(string.IsNullOrWhiteSpace(tipoPrompt))
                {
                    _logger.LogError("TipoPrompt nulo ou vazio ao buscar PromptEmpresa para Empresa ID: {EmpresaId}", empresaId);
                    return null;
                }

                return await _promptEmpresaRepository.GetPromptAsync(empresaId, sistema, tipoPrompt, includeDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar PromptEmpresa por Empresa ID: {EmpresaId}", empresaId);
                throw;
            }
        }
    }
}

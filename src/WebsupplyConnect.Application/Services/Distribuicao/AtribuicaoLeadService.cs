using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    public class AtribuicaoLeadService(ILogger<AtribuicaoLeadService> logger, IAtribuicaoLeadRepository atribuicaoLeadRepository) : IAtribuicaoLeadService
    {
        private readonly ILogger<AtribuicaoLeadService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAtribuicaoLeadRepository _atribuicaoLeadRepository = atribuicaoLeadRepository ?? throw new ArgumentNullException(nameof(atribuicaoLeadRepository));

        public async Task<AtribuicaoLead> CriarAtribuicaoAsync(AtribuicaoLead atribuicao)
        {
            try
            {
                var atribuicaoCriada = await _atribuicaoLeadRepository.CriarAtribuicaoAsync(atribuicao);

                return atribuicaoCriada;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar atribuição de lead");
                throw;
            }
        }

        public async Task<AtribuicaoLead> UpdateAsync(AtribuicaoLead atribuicao)
        {
            try
            {
                var atribuicaoAtualizada = await _atribuicaoLeadRepository.UpdateAsync(atribuicao);
                return atribuicaoAtualizada;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar atribuição de lead");
                throw;
            }
        }

        public async Task<AtribuicaoLead?> ObterUltimaAtribuicaoLeadAsync(int leadId)
        {
            try
            {
                var atribuicao = await _atribuicaoLeadRepository.ObterUltimaAtribuicaoLeadAsync(leadId);

                if (atribuicao == null)
                {
                    _logger.LogWarning("Nenhuma atribuição encontrada para o lead ID {LeadId}", leadId);
                }
                return atribuicao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter última atribuição de lead");
                throw;
            }
        }

        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorLeadAsync(int leadId)
        {
            try
            {
                return await _atribuicaoLeadRepository.ListAtribuicoesPorLeadAsync(leadId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar atribuições por lead ID {LeadId}", leadId);
                throw;
            }
        }

        public async Task<bool> LeadPossuiResponsavelAsync(int leadId)
        {
            try
            {
                return await _atribuicaoLeadRepository.LeadPossuiResponsavelAsync(leadId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se lead possui responsável");
                throw;
            }
        }

        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorVendedorAsync(
            int vendedorId,
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            int pagina = 1,
            int tamanhoPagina = 20)
        {
            try
            {
                var atribuicoes = await _atribuicaoLeadRepository.ListAtribuicoesPorVendedorAsync(
                    vendedorId,
                    empresaId,
                    dataInicio,
                    dataFim,
                    pagina,
                    tamanhoPagina);
                return atribuicoes ?? [];

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar atribuições por vendedor ID {VendedorId}", vendedorId);
                throw;
            }
        }

        public async Task<int> CountAtribuicoesPorVendedorAsync(
            int vendedorId,
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            try
            {
                return await _atribuicaoLeadRepository.CountAtribuicoesPorVendedorAsync(
                    vendedorId,
                    empresaId,
                    dataInicio,
                    dataFim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar atribuições por vendedor ID {VendedorId}", vendedorId);
                throw;
            }
        }

        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorEmpresaAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            try
            {
                var atribuicoes = await _atribuicaoLeadRepository.ListAtribuicoesPorEmpresaAsync(
                    empresaId,
                    dataInicio,
                    dataFim);
                return atribuicoes ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar atribuições por empresa ID {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<object>> GetDistribuicoesPorVendedorAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            try
            {
                var distribuicoes = await _atribuicaoLeadRepository.GetDistribuicoesPorVendedorAsync(
                    empresaId,
                    dataInicio,
                    dataFim);
                if (distribuicoes == null || !distribuicoes.Any())
                {
                    _logger.LogWarning("Nenhuma distribuição encontrada para a empresa ID {EmpresaId}", empresaId);
                    return [];
                }
                return distribuicoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter distribuições por vendedor");
                throw;
            }
        }
    }
}

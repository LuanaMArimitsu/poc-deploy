using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class ConversaReaderService(IConversaRepository conversaRepository, ILogger<ConversaReaderService> logger) : IConversaReaderService
    {
        private readonly IConversaRepository _conversaRepository = conversaRepository ?? throw new ArgumentNullException(nameof(conversaRepository));
        private readonly ILogger<ConversaReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Conversa?> GetConversaByLead(int id, string statusEncerrado)
        {
            try
            {
                var statusId = await _conversaRepository.GetConversaStatusByCodeAsync(statusEncerrado);
                var conversa = await _conversaRepository.GetByPredicateAsync<Conversa>(e => e.LeadId == id && e.StatusId != statusId && !e.Excluido);
                return conversa;
            }
            catch
            {
                _logger.LogError("Erro ao procurar uma conversa com o lead: {lead}", id);
                throw;
            }
        }

        public async Task<List<Conversa>> GetAllConversasAtivaByLeadAsync(int id, string statusEncerrado)
        {
            try
            {
                var statusId = await _conversaRepository.GetConversaStatusByCodeAsync(statusEncerrado);
                var conversas = await _conversaRepository.GetListByPredicateAsync<Conversa>(
                    e => e.LeadId == id && e.StatusId != statusId
                );
                return conversas;
            }
            catch
            {
                _logger.LogError("Erro ao procurar todas as conversas ativas para o lead: {lead}", id);
                throw;
            }
        }

        // < Summary>
        // Verifica se o lead já foi atendido em outra conversa
        //</Summary>
        public async Task<Conversa?> GetUltimaConversaLead(int leadId, int equipeId)
        {
            try
            {
                var conversa = await _conversaRepository.GetUltimaConversaLead(leadId, equipeId);
                return conversa;
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se o lead {leadId} está sendo atendido.", leadId);
                throw;
            }
        }

        public async Task<List<ConversaStatus>> GetListConversaStatus()
        {
            try
            {
                return await _conversaRepository.GetListByPredicateAsync<ConversaStatus>(e => e.Codigo != "ENCERRADA" && e.Codigo != "AGUARDANDO_RESPOSTA");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retornar lista de status da conversa.");
                throw;
            }
        }

        public async Task<Conversa> GetConversaByIdAsync(int conversaId)
        {
            try
            {
                return await _conversaRepository.GetByIdAsync<Conversa>(conversaId, false) ?? throw new AppException($"Conversa com o ID {conversaId} não foi encontrada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em encontrar conversa com ID {conversaId}.", conversaId);
                throw;
            }
        }

        public async Task<ConversaStatus> GetConversaStatusAsync(int? id = null, string? codigo = null)
        {
            if (id == null && string.IsNullOrWhiteSpace(codigo))
                throw new AppException("É necessário informar pelo menos o ID ou o código da conversa.");

            try
            {
                var resultado = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(
                    e => e.Id == id || e.Codigo == codigo);

                return resultado ?? throw new AppException($"Conversa status com id {id} não foi encontrada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status da conversa. Status id: {id}, Status código: {codigo}", id, codigo);
                throw;
            }
        }

        public async Task<List<Conversa>> GetConversasByUsuarioAsync(int usuarioId, string codigoStatus)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new AppException("UsuárioId inválido.");

                if (string.IsNullOrWhiteSpace(codigoStatus))
                    throw new AppException("Código do status é obrigatório.");

                var statusId = await _conversaRepository
                    .GetConversaStatusByCodeAsync(codigoStatus);

                return await _conversaRepository
                    .GetConversasByUsuarioAsync(usuarioId, statusId) ?? throw new AppException($"Conversa não encontrado pelo o Usuário Id {usuarioId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retornar lista de conversas por usuário. Usuário: {usuarioId}, Status: {codigoStatus}", usuarioId, codigoStatus);
                throw;
            }
        }

        public async Task<bool> IsPrimeiraMensagemCliente(int conversaId)
        {
            try
            {
                return await _conversaRepository.IsPrimeiraMensagemClienteAsync(conversaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conferir se é a primeira mensagem do cliente. Conversa: {id}", conversaId);
                throw;
            }
        }

        public async Task<bool> ExisteConversaNoCanalAsync(int usuarioId, int canalId)
        {
            var statusEncerrado = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
            return await _conversaRepository.ExisteConversaNoCanalAsync(usuarioId, canalId, statusEncerrado);
        }

        public async Task<ConversasEncerradasResultDTO> ListConversasEncerradaAsync(int usuarioId, ConversaPagParam param)
        {
            try
            {

                if (usuarioId <= 0)
                    throw new AppException("ID do usuário deve ser maior que zero.");

                if ((param.quantidadeInicial.HasValue && !param.quantidadeFinal.HasValue) || (!param.quantidadeInicial.HasValue && param.quantidadeFinal.HasValue))
                    throw new AppException("Quantidade início e fim devem ser informados juntos.");

                if (param.quantidadeInicial.HasValue && param.quantidadeFinal.HasValue && param.quantidadeFinal < param.quantidadeInicial)
                    throw new AppException("'quantidadeFim' não pode ser menor que 'quantidadeInicio'.");

                var statusEncerrado = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");

                var conversas = await _conversaRepository.GetConversasEncerradasByUsuarioAsync(usuarioId, statusEncerrado, param.quantidadeInicial, param.quantidadeFinal, param.empresaId);

                var HistoricoEncerradas = await _conversaRepository.GetTotalConversasEncerradasByUsuarioAsync(
                    usuarioId, statusEncerrado);

                var listaDto = conversas.Select(c => new ListConversasEncerradaDTO
                {
                    ConversaId = c.Id,
                    LeadId = c.LeadId,
                    LeadNome = c.Lead?.Nome ?? string.Empty,
                    Status = "ENCERRADA",
                    DataCriacao = c.DataCriacao
                }).ToList();

                return new ConversasEncerradasResultDTO
                {
                    TotalEncerradas = HistoricoEncerradas,
                    Conversas = listaDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar conversas encerradas do usuário {usuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<List<Conversa>> GetConversasComInatividade(int responsavelId, int pagina, int tamanhoPagina)
        {
            return await _conversaRepository.GetConversasComInatividade(responsavelId, pagina, tamanhoPagina);
        }

        public async Task<List<Conversa>> GetConversasComAviso(int responsavelId, int pagina, int tamanhoPagina)
        {
            return await _conversaRepository.GetConversasComAviso(responsavelId, pagina, tamanhoPagina);
        }

        public async Task<List<Conversa>> GetConversasSemAtendimento(int pagina, int tamanhoPagina)
        {
            return await _conversaRepository.GetConversasSemAtendimento(pagina, tamanhoPagina);
        }

        public async Task<List<Conversa>> ObterConversasPorLeadIdParaETLAsync(int leadId)
        {
            return await _conversaRepository.GetListByPredicateAsync<Conversa>(c => c.LeadId == leadId, includeDeleted: true);
        }

        public async Task<List<Conversa>> ObterConversasPorPeriodoModificacaoParaETLAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _conversaRepository.GetListByPredicateAsync<Conversa>(
                c => c.DataModificacao >= dataInicio && c.DataModificacao <= dataFim, includeDeleted: true);
        }

        public async Task<List<Conversa>> GetAllConversasByLeadAsync(int leadId)
        {
            try
            {
                var conversas = await _conversaRepository.GetListByPredicateAsync<Conversa>(c => c.LeadId == leadId, includeDeleted: true);
                return conversas;
            }
            catch
            {
                _logger.LogError("Erro ao procurar todas as conversas ativas para o lead: {lead}", leadId);
                throw;
            }
        }

        public async Task<Dictionary<int, int>> ObterConversaAtivaIdsPorLeadIdsAsync(List<int> leadIds)
        {
            if (leadIds == null || leadIds.Count == 0)
                return new Dictionary<int, int>();

            try
            {
                var statusEncerradoId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");

                var conversas = await _conversaRepository.GetListByPredicateAsync<Conversa>(
                    c => leadIds.Contains(c.LeadId) && c.StatusId != statusEncerradoId);

                return conversas
                    .GroupBy(c => c.LeadId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.Id).First().Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar conversas ativas para {count} leads.", leadIds.Count);
                throw;
            }
        }

        public async Task<bool> ExisteConversaEncerradaPorLeadAsync(int leadId)
        {
            return await _conversaRepository.ExisteConversaEncerradaPorLeadAsync(leadId);
        }

        public async Task<Dictionary<int, (string? Contexto, DateTime? DataAtualizacaoContexto, bool TrocaDeContato, string? ClassificacaoIA)>> GetContextosByIdsAsync(IReadOnlyCollection<int> conversaIds)
        {
            if (conversaIds == null || conversaIds.Count == 0)
                return new Dictionary<int, (string? Contexto, DateTime? DataAtualizacaoContexto, bool TrocaDeContato, string? ClassificacaoIA)>();

            return await _conversaRepository.GetContextosByIdsAsync(conversaIds);
        }

    }
}

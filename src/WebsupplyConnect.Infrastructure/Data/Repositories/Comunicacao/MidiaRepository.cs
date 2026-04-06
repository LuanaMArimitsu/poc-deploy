using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    public class MidiaRepository(ILogger<MidiaRepository> logger, WebsupplyConnectDbContext context, IUnitOfWork unitOfWork) : BaseRepository(context, unitOfWork), IMidiaRepository
    {
        private readonly ILogger<MidiaRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<Midia> GetMidiaByMensagemId(int mensagemId)
        {
            try
            {
                if (mensagemId <= 0)
                    throw new InfraException("O ID da mensagem deve ser maior que zero para buscar a mídia.");

                var midia = await GetByPredicateAsync<Midia>(w => w.MensagemId == mensagemId);

                return midia ?? throw new InfraException($"Nenhuma mídia encontrada para o mensagemId: {mensagemId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mídia para mensagemId {mensagemId}", mensagemId);
                throw new InfraException($"Erro ao buscar mídia para mensagemId: {mensagemId}.", ex);
            }
        }


        public async Task<MidiaStatusProcessamento> GetMidiaStatusProcessamentoAsync(string codigo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigo))
                    throw new InfraException("O código do status processamento não pode ser vazio.");

                return await GetByPredicateAsync<MidiaStatusProcessamento>(
                    w => w.Codigo == codigo
                ) ?? throw new InfraException($"Nenhum status de processamento encontrado para o código: {codigo}") ;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro em buscar status de processamento da Mídia pelo codigo: {codigo}. Erro: {erro}", codigo, ex.Message);
                throw new InfraException($"Erro em buscar status de processamento da Mídia pelo codigo: {codigo}. Erro: {ex.Message}");
            }
        }
    }
}

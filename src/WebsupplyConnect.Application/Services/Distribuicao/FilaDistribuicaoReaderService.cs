using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    public class FilaDistribuicaoReaderService(ILogger<FilaDistribuicaoReaderService> logger, IFilaDistribuicaoRepository filaDistribuicaoRepository) : IFilaDistribuicaoReaderService
    {
        private readonly ILogger<FilaDistribuicaoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFilaDistribuicaoRepository _filaDistribuicaoRepository = filaDistribuicaoRepository ?? throw new ArgumentNullException(nameof(filaDistribuicaoRepository));

        public async Task<FilaDistribuicao> GetMembroEquipeFilaDistribuicaoById(int membroEquipeId)
        {
            if (membroEquipeId <= 0)
            {
                throw new AppException("O ID do membro deve ser maior que zero.");
            }

            var fila = await _filaDistribuicaoRepository.GetByIdAsync<FilaDistribuicao>(membroEquipeId);
            if (fila == null)
            {
                _logger.LogError("Nenhuma fila de distribuição encontrada para o membro equipe com ID {UsuarioId}.", membroEquipeId);
                throw new AppException($"Nenhuma fila de distribuição encontrada para o membro equipe com ID {membroEquipeId}.");
            }
            return fila;
        }
    }
}

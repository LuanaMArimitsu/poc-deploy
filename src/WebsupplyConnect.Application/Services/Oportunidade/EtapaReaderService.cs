using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;

namespace WebsupplyConnect.Application.Services.Oportunidade
{
    public class EtapaReaderService(ILogger<EtapaReaderService> logger, IEtapaRepository etapaRepository) : IEtapaReaderService
    {
        private readonly ILogger<EtapaReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEtapaRepository _etapaRepository = etapaRepository ?? throw new ArgumentNullException(nameof(etapaRepository));

        public async Task<List<Etapa>> GetListEtapaByFunil(int funilId)
        {
            try
            {
                return await _etapaRepository.GetListByPredicateAsync<Etapa>(e => e.FunilId == funilId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar etapas do funil com ID {FunilId}", funilId);
                throw;
            }
        }

        public async Task<Etapa?> GetEtapaById(int etapaId)
        {
            try
            {
                return await _etapaRepository.GetByIdAsync<Etapa>(etapaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar etapa com ID {EtapaId}", etapaId);
                throw;
            }
        }

        public async Task<List<EtapaHistoricoListDTO>> GetListEtapaHistorico(int oportunidadeId)
        {
            try
            {
                var list = await _etapaRepository.GetListEtapaHistorico(oportunidadeId);


                var listDto = list.Select(e => new EtapaHistoricoListDTO
                {
                    NomeEtapa = e.EtapaNova.Nome,
                    DataMudanca = e.DataMudanca,
                    Observacao = e.Observacao ?? "Esta etapa não possui observação.",
                    Cor = e.EtapaNova.Cor ?? "#000000"

                })
               .OrderByDescending(e => e.DataMudanca)
               .ToList();

                return listDto;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar histórico de etapas da oportunidade com ID {id}", oportunidadeId);
                throw;
            }
        }

        public async Task<List<Etapa>> ListarEtapasParaETLAsync(CancellationToken cancellationToken = default)
        {
            return await _etapaRepository.GetListByPredicateAsync<Etapa>(e => !e.Excluido);
        }
    }
}

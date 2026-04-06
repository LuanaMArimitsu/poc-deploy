using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;

namespace WebsupplyConnect.Application.Services.Oportunidade
{
    public class FunilReaderService(ILogger<FunilReaderService> logger, IFunilRepository funilRepository, IEtapaReaderService etapaReaderService) : IFunilReaderService
    {
        private readonly ILogger<FunilReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFunilRepository _funilRepository = funilRepository ?? throw new ArgumentNullException(nameof(funilRepository));
        private readonly IEtapaReaderService _etapaReaderService = etapaReaderService ?? throw new ArgumentNullException(nameof(etapaReaderService));
        public async Task<List<GetEtapasDTO>> GetFunilByEmpresa(int empresaId)
        {
            try
            {
                var funil = await _funilRepository.GetByPredicateAsync<Funil>(e => e.EmpresaId == empresaId) ?? throw new AppException($"Funil não existe com a empresa id {empresaId}.");
                var etapas = await _etapaReaderService.GetListEtapaByFunil(funil.Id);
                List<GetEtapasDTO> listEtapas = [];
                foreach (var etapa in etapas)
                {
                    var item = new GetEtapasDTO()
                    {
                        Id = etapa.Id,
                        Nome = etapa.Nome,
                        Descricao = etapa.Descricao,
                        Ordem = etapa.Ordem,
                        Cor = etapa.Cor,
                        ProbabilidadePadrao = etapa.ProbabilidadePadrao,
                        EhAtiva = etapa.EhAtiva,
                        EhFinal = etapa.EhFinal,
                        EhVitoria = etapa.EhVitoria,
                        EhPerdida = etapa.EhPerdida,
                        EhExibida = etapa.EhExibida,
                        FunilId = etapa.FunilId,
                        Ativo = etapa.Ativo
                    };
                    listEtapas.Add(item);
                }

                return listEtapas;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar funis da empresa com ID {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Funil>> ListarFunisParaETLAsync(CancellationToken cancellationToken = default)
        {
            return await _funilRepository.GetListByPredicateAsync<Funil>(f => !f.Excluido);
        }
    }
}

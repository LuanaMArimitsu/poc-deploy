using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Equipe;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class StatusMembroEquipeReadService : IStatusMembroEquipeReadService
    {
        private static readonly HashSet<string> CodigosValidos = new()
        {
            "ATIVO",
            "TREINAMENTO",
            "LICENCA",
            "INATIVO",
            "TRANSFERENCIA"
        };

        private readonly IStatusMembroEquipeRepository _repo;
         private readonly ILogger<StatusMembroEquipe> _logger;
        public StatusMembroEquipeReadService(IStatusMembroEquipeRepository repo, ILogger<StatusMembroEquipe> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public Task<bool> StatusExisteAsync(int statusId)
        {
            return _repo.ExistsInDatabaseAsync<StatusMembroEquipe>(statusId);
        }

        public async Task<StatusMembroEquipe> GetStatusMembro(int? statusId = null, string? codigoStatus = null)
        {
            if (statusId <= 0 && statusId == null && codigoStatus == null)
            {
                throw new ApplicationException("É necessário informar o statusId ou o codigoStatus.");
            }

            StatusMembroEquipe? status = null;

            if (statusId != null)
            {
                status = await _repo.GetByIdAsync<StatusMembroEquipe>(statusId.Value);
            }
            else if (codigoStatus != null)
            {
                codigoStatus = codigoStatus.ToUpperInvariant();
                status = await _repo.GetByPredicateAsync<StatusMembroEquipe>(e => e.Codigo == codigoStatus);
            }

            if (status == null)
            {
                _logger.LogError("Status de membro de equipe não encontrado. statusId: {StatusId}, codigoStatus: {CodigoStatus}", statusId, codigoStatus);
                throw new ApplicationException("Status de membro de equipe não encontrado.");
            }

            return status;
        }
        public async Task<List<StatusMembroEquipeDto>> ListarStatusFixoAsync()
        {
            var itens = await _repo.ListarStatusFixosAsync();

            var filtrados = itens.Where(s =>
                !string.IsNullOrWhiteSpace(s.Codigo) &&
                CodigosValidos.Contains(s.Codigo.ToUpper())
            );

            return filtrados.Select(s => new StatusMembroEquipeDto
            {
                Id = s.Id,
                Codigo = s.Codigo ?? string.Empty,
                Nome = s.Nome ?? string.Empty
            }).ToList();
        }
    }
}
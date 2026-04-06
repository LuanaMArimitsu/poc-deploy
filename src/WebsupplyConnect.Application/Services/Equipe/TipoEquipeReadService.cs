using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Equipe;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class TipoEquipeReadService : ITipoEquipeReadService
    {
        private readonly ITipoEquipeRepository _repo;
        private readonly ILogger<TipoEquipe> _logger;

        public TipoEquipeReadService(ITipoEquipeRepository repo, ILogger<TipoEquipe> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<bool> TipoExisteAsync(int tipoEquipeId)
        {
            return await _repo.ExistsInDatabaseAsync<TipoEquipe>(tipoEquipeId);
        }

        public async Task<List<TipoEquipeDto>> GetTiposFixosAsync()
        {
            var itens = await _repo.ListarAsync();

            return itens.Select(t => new TipoEquipeDto
            {
                Id = t.Id,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Ordem = t.Ordem,
                Icone = t.Icone
            }).ToList();
        }
    }
}

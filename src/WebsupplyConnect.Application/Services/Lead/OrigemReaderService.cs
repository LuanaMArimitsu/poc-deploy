using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class OrigemReaderService : IOrigemReaderService
    {
        private readonly IOrigemRepository _origemRepository;
        private readonly ITipoOrigemRepository _tipoOrigemRepository;

        public OrigemReaderService(IOrigemRepository origemRepository, ITipoOrigemRepository tipoOrigemRepository)
        {
            _origemRepository = origemRepository ?? throw new ArgumentNullException(nameof(origemRepository));
            _tipoOrigemRepository = tipoOrigemRepository ?? throw new ArgumentNullException(nameof(tipoOrigemRepository));
        }

        public async Task<List<OrigemDTO>> ListarOrigensAsync()
        {
            var origens = await _origemRepository.ListarOrigensAsync();
            return origens.Select(o => new OrigemDTO
            {
                Id = o.Id,
                Nome = o.Nome,
                Descricao = o.Descricao,
                OrigemTipoId = o.OrigemTipoId,
                OrigemTipoNome = o.OrigemTipo?.Nome ?? string.Empty
            }).ToList();
        }

        public async Task<List<OrigemSimplesDTO>> ListarOrigensSimplesAsync()
        {
            var origens = await _origemRepository.ListarOrigensAsync();
            return origens.Select(o => new OrigemSimplesDTO
            {
                Id = o.Id,
                Nome = o.Nome
            }).ToList();
        }


        public async Task<OrigemDTO> GetOrigemByIdAsync(int id)
        {
            try
            {
                var origem = await _origemRepository.GetOrigemByIdAsync(id) ?? throw new ApplicationException($"Erro ao encontrar origem pelo id: {id}");

                return new OrigemDTO
                {
                    Id = origem.Id,
                    Nome = origem.Nome,
                    Descricao = origem.Descricao,
                    OrigemTipoId = origem.OrigemTipoId,
                    OrigemTipoNome = origem.OrigemTipo?.Nome ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter a origem por ID.", ex);
            }
        }

        public async Task<List<TipoOrigemDTO>> GetAllOrigemTiposAsync()
        {
            try
            {
                var origemTipos = await _tipoOrigemRepository.GetListByPredicateAsync<OrigemTipo>(
                    predicate: ot => !ot.Excluido,
                    orderBy: query => query.OrderBy(ot => ot.Ordem)
                );

                return origemTipos.Select(ot => new TipoOrigemDTO
                {
                    Id = ot.Id,
                    Codigo = ot.Codigo,
                    Nome = ot.Nome,
                    Descricao = ot.Descricao,
                    Ordem = ot.Ordem
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter os tipos de origem.", ex);
            }
        }

        public async Task<Origem> GetOrigemByName(string name)
        {
            try
            {
                var origem = await _origemRepository.GetOrigemByName(name) ?? throw new ApplicationException($"Erro ao encontrar origem pelo nome: {name}");
                return origem;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter a origem por nome.", ex);
            }
        }

        public async Task<List<Origem>> ListarOrigensNaoExcluidasParaETLAsync()
        {
            return await _origemRepository.GetListByPredicateAsync<Origem>(o => !o.Excluido, includeDeleted: true);
        }

        public Task<List<Origem>> ListarTodasOrigensParaETLAsync() =>
            _origemRepository.GetListByPredicateAsync<Origem>(o => true, includeDeleted: true);
    }
}
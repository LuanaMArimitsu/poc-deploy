using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Equipe;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class EquipeReaderService : IEquipeReaderService
    {
        private readonly IEquipeRepository _equipeRepo;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService;
        private readonly IConversaReaderService _conversaReaderService;
        private readonly ILogger<EquipeReaderService> _logger;

        public EquipeReaderService(IEquipeRepository equipeRepo, IMembroEquipeReaderService membroEquipeReaderService, IConversaReaderService conversaReaderService, ILogger<EquipeReaderService> logger)
        {
            _equipeRepo = equipeRepo;
            _membroEquipeReaderService = membroEquipeReaderService;
            _conversaReaderService = conversaReaderService;
            _logger = logger;
        }

        public async Task<EquipePaginadoDto> ListarPorEmpresaAsync(EquipeFiltroRequestDto filtro)
        {
            if (filtro.EmpresaId <= 0)
                throw new AppException("EmpresaId deve ser maior que zero.");

            //var pagina = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
            //var tamanho = filtro.TamanhoPagina <= 0 ? 10 : filtro.TamanhoPagina;

            bool paginar = filtro.Pagina > 0 && filtro.TamanhoPagina > 0;

            int? pagina = paginar ? filtro.Pagina : null;
            int? tamanho = paginar ? filtro.TamanhoPagina : null;

            var (equipes, totalItens) = await _equipeRepo.ListarPorEmpresaFiltradoAsync(
                empresaId: filtro.EmpresaId,
                tipoEquipeId: filtro.TipoEquipeId,
                ativas: filtro.Ativa,
                responsavelMembroId: filtro.ResponsavelMembroId,
                busca: filtro.Busca,
                pagina: pagina,
                tamanhoPagina: tamanho
            );

            //var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanho);

            var itens = equipes.Select(e => new ListEquipeDto
            {
                Id = e.Id,
                Nome = e.Nome,
                Descricao = e.Descricao,
                EhPadrao = e.EhPadrao,

                TipoEquipeId = e.TipoEquipeId,
                TipoEquipeNome = e.TipoEquipe?.Nome ?? string.Empty,

                EmpresaId = e.EmpresaId,
                EmpresaNome = e.Empresa?.Nome ?? string.Empty,

                Ativa = e.Ativa,

                ResponsavelMembroId = e.ResponsavelMembroId ?? 0,
                ResponsavelNome = e.ResponsavelMembro?.Usuario?.Nome ?? string.Empty,

                TotalMembros = e.Membros?.Count(m => !m.Excluido) ?? 0,
                MembrosAtivos = e.Membros?.Count(m => !m.Excluido && m.DataSaida == null) ?? 0,

                TempoMaxSemAtendimento = e.TempoMaxSemAtendimento.HasValue
                ? (int)e.TempoMaxSemAtendimento.Value.TotalMinutes
                : 0
            }).ToList();

            var totalPaginas = paginar
                ? (int)Math.Ceiling(totalItens / (double)tamanho!.Value)
                : 1;

            return new EquipePaginadoDto
            {
                TotalItens = totalItens,
                PaginaAtual = paginar ? pagina!.Value : 1,
                TotalPaginas = totalPaginas,
                Itens = itens
            };
        }

        public async Task<List<EquipeSimplesDto>> ListaSimplesPorEmpresaAsync(int empresaId)
        {
            if (empresaId <= 0)
                throw new AppException("EmpresaId deve ser maior que zero.");

            var equipes = await _equipeRepo.GetByEmpresaWithMembersAsync(empresaId);

            var itens = equipes.Select(e => new EquipeSimplesDto
            {
                Id = e.Id,
                Nome = e.Nome,
                Membros = e.Membros?
                    .Where(m => !m.Excluido)
                    .Select(m => new MembroSimplesDTO
                    {
                        Id = m.Id,
                        Nome = m.Usuario?.Nome ?? string.Empty,
                    }).ToList() ?? []
            }).ToList();

            return itens;
        }

        public async Task<ListDetalheEquipeDto> GetByIdAsync(int id)
        {
            var entidade = await _equipeRepo.GetByIdEquipeDetailsAsync(id);
            if (entidade is null)
                throw new AppException("Equipe não encontrada.");

            var listDetalheEquipe = new ListDetalheEquipeDto
            {
                Id = entidade.Id,
                Nome = entidade.Nome,
                Descricao = entidade.Descricao,
                EhPadrao = entidade.EhPadrao,
                EmpresaId = entidade.EmpresaId,
                EmpresaNome = entidade.Empresa?.Nome ?? string.Empty,
                TipoEquipeId = entidade.TipoEquipeId,
                TipoEquipeNome = entidade.TipoEquipe?.Nome ?? string.Empty,
                Ativa = entidade.Ativa,
                ResponsavelMembroId = entidade.ResponsavelMembroId ?? 0,
                ResponsavelNome = entidade.ResponsavelMembro?.Usuario?.Nome ?? string.Empty,
                TempoMaxSemAtendimento = entidade.TempoMaxSemAtendimento.HasValue
                    ? (int)entidade.TempoMaxSemAtendimento.Value.TotalMinutes
                    : 0


                //NotificarAtribuicaoAoDestinatario = entidade.NotificarAtribuicaoAoDestinatario,
                //NotificarAtribuicaoAosLideres = entidade.NotificarAtribuicaoAosLideres,
                //NotificarSemAtendimentoLideres = entidade.NotificarSemAtendimentoLideres,
                //TempoSemAtendimentoHoras = entidade.TempoMaxSemAtendimento.HasValue ? (int?)entidade.TempoMaxSemAtendimento.Value.Hours : null, //separar horas e minutos se valor, se não retorna nulo para os dois campos
                //TempoSemAtendimentoMinutos = entidade.TempoMaxSemAtendimento.HasValue ? (int?)entidade.TempoMaxSemAtendimento.Value.Minutes : null,
            };

            return listDetalheEquipe;
        }

        public async Task<Domain.Entities.Equipe.Equipe?> GetEquipePadraoAsync(int empresaId)
        {
            try
            {
                var equipe = await _equipeRepo.GetByPredicateAsync<Domain.Entities.Equipe.Equipe>(e => e.EmpresaId == empresaId && e.Ativa == true && e.EhPadrao == true);
                return equipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter equipe padrão para a empresa {EmpresaId}", empresaId);
                throw new AppException("Erro ao buscar equipe padrão de uma empresa especifica.");
            }
        }

        public async Task<List<ResponsaveisPorEmpresaDto>> ListarResponsaveisPorEmpresaAsync(int empresaId)
        {
            if (empresaId <= 0)
                throw new DomainException("O parâmetro EmpresaId deve ser maior que zero.");

            var equipes = await _equipeRepo.ListEquipesComResponsavelAsync(empresaId);

            if (equipes is null || equipes.Count == 0)
                throw new DomainException("Nenhuma equipe com responsável encontrada para a empresa informada.");

            var result = equipes
                .Where(e => e.ResponsavelMembro != null)
                .GroupBy(e => new { e.EmpresaId, e.Empresa!.Nome })
                .Select(g => new ResponsaveisPorEmpresaDto
                {
                    EmpresaId = g.Key.EmpresaId,
                    EmpresaNome = g.Key.Nome,
                    Responsaveis = g
                        .GroupBy(e => e.ResponsavelMembro!.UsuarioId)
                        .Select(e => new ResponsavelEquipeDto
                        {
                            UsuarioId = e.First().ResponsavelMembro!.UsuarioId,
                            UsuarioNome = e.First().ResponsavelMembro!.Usuario!.Nome,
                            MembroId = e.First().ResponsavelMembroId ?? 0
                        })
                        .OrderBy(r => r.UsuarioNome)
                        .ToList()
                })
                .ToList();

            return result;
        }

        public async Task<Domain.Entities.Equipe.Equipe?> GetEquipeByIdAsync(int id)
        {
            if (id <= 0)
                throw new AppException("O ID da equipe deve ser maior que zero.");

            try
            {
                var equipe = await _equipeRepo.GetByPredicateAsync<Domain.Entities.Equipe.Equipe>(
                    e => e.Id == id && !e.Excluido
                );

                if (equipe is null)
                {
                    _logger.LogWarning("Equipe {EquipeId} não encontrada.", id);
                    return null;
                }

                return equipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipe {EquipeId}.", id);
                throw new AppException("Erro ao buscar dados da equipe.");
            }
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> GetEquipesByEmpresaId(int empresaId)
        {
            return await _equipeRepo.GetListByPredicateAsync<Domain.Entities.Equipe.Equipe>(e => e.EmpresaId == empresaId && e.Ativa && !e.Excluido);
        }

        public async Task<Domain.Entities.Equipe.Equipe?> GetEquipeIntegracaoPorEmpresaIdAsync(int empresaId)
        {
            return await _equipeRepo.GetEquipeIntegracaoPorEmpresaIdAsync(empresaId);
        }

        public async Task<Domain.Entities.Equipe.Equipe?> ObterEquipeOlxIdAsync(int empresaId)
        {
            return await _equipeRepo.ObterEquipeOlxIdAsync(empresaId);
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> ObterEquipesNaoExcluidasParaETLAsync()
        {
            return await _equipeRepo.GetListByPredicateAsync<Domain.Entities.Equipe.Equipe>(e => !e.Excluido, includeDeleted: true);
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> ObterEquipesComMembrosPorEmpresaParaETLAsync(int empresaId)
        {
            return await _equipeRepo.GetByEmpresaWithMembersAsync(empresaId);
        }

        public async Task<List<ListMembroEEquipeDTO>> ListarMembrosEEquipesByResponsavelAsync(int usuarioId)
        {
            try
            {
                var membroResponsavel = await _membroEquipeReaderService.ObterMembrosPorUsuarioIsLiderAsync(usuarioId);
                if (membroResponsavel == null)
                    throw new AppException("Responsável não encontrado.");

                //pega todos os membro equipes ID do responsavel
                var membroId = membroResponsavel.Select(m => m.Id).ToList();

                //com a lista de membro equipes ID do responsavel, busca as equipes que ele é responsável e os membros dessas equipes
                var equipes = await _equipeRepo.GetEquipeComMembrosByResponsavelAsync(membroId);

                var membrosDict = new Dictionary<int, ListMembroEEquipeDTO>();

                foreach (var equipe in equipes)
                {
                    foreach (var membro in equipe.Membros)
                    {
                        if (membro.UsuarioId == usuarioId)
                            continue;

                        if (!membrosDict.TryGetValue(membro.UsuarioId, out var membroDto))
                        {
                            membroDto = new ListMembroEEquipeDTO
                            {
                                UsuarioId = membro.UsuarioId,
                                UsuarioNome = membro.Usuario?.Nome ?? string.Empty,
                                Equipes = new List<ListEquipeEConversasDTO>()
                            };

                            membrosDict.Add(membro.UsuarioId, membroDto);
                        }

                        var conversas = await _conversaReaderService.GetConversasByUsuarioAsync(membro.UsuarioId, "ENCERRADA");

                        // contar conversas da equipe
                        var quantidadeConversas = conversas
                            .Count(c => c.EquipeId == equipe.Id);

                        membroDto.Equipes.Add(new ListEquipeEConversasDTO
                        {
                            EmpresaId = equipe.EmpresaId,
                            EmpresaNome = equipe.Empresa?.Nome ?? string.Empty,
                            EquipeId = equipe.Id,
                            Nome = equipe.Nome,
                            QuantidadeConversasAtivas = quantidadeConversas
                        });
                    }
                }
                return membrosDict.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar membros e equipes pelo responsável. UsuarioId: {UsuarioId}", usuarioId);
                throw new AppException("Erro ao listar membros e equipes do responsável.");
            }

        }

        
        //método para obter equipes de uma empresa através de uma lista de EmpresaId
        public async Task<List<Domain.Entities.Equipe.Equipe>> GetListEquipesByEmpresasIds(List<int> empresasIds)
        {
            if (empresasIds == null || empresasIds.Count == 0)
                return new List<Domain.Entities.Equipe.Equipe>();

            return await _equipeRepo.GetListByPredicateAsync<Domain.Entities.Equipe.Equipe>(
                e => empresasIds.Contains(e.EmpresaId) && e.Ativa && !e.Excluido
            );
        }

        public async Task<List<EquipeListagemSimplesDto>> ListarSimplesPorEmpresaIdAsync(int empresaId)
        {
            if (empresaId <= 0)
                throw new AppException("EmpresaId deve ser maior que zero.");

            var equipes = await _equipeRepo.GetListByPredicateAsync<Domain.Entities.Equipe.Equipe>(
                e => e.EmpresaId == empresaId && e.Ativa && !e.Excluido
            );

            return equipes.Select(e => new EquipeListagemSimplesDto
            {
                Id = e.Id,
                Nome = e.Nome
            }).ToList();
        }
    }
}

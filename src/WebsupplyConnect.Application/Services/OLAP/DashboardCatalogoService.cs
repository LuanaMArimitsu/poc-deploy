using WebsupplyConnect.Application.DTOs.Dashboard;
using WebsupplyConnect.Application.Helpers;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.OLAP;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

namespace WebsupplyConnect.Application.Services.OLAP;

/// <summary>
/// Serviço para obter os catálogos (filtros) do Dashboard OLAP.
/// Retorna dados exclusivamente das dimensões OLAP para consistência com os dados exibidos no dashboard.
/// </summary>
public class DashboardCatalogoService : IDashboardCatalogoService
{
    private readonly IDimensaoRepository _dimensaoRepository;
    private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService;
    private readonly ITipoEquipeReadService _tipoEquipeReadService;
    private readonly IMembroEquipeReaderService _membroEquipeReaderService;

    public DashboardCatalogoService(
        IDimensaoRepository dimensaoRepository,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService,
        ITipoEquipeReadService tipoEquipeReadService,
        IMembroEquipeReaderService membroEquipeReaderService)
    {
        _dimensaoRepository = dimensaoRepository;
        _usuarioEmpresaReaderService = usuarioEmpresaReaderService;
        _tipoEquipeReadService = tipoEquipeReadService;
        _membroEquipeReaderService = membroEquipeReaderService;
    }

    public async Task<DashboardCataloguesDTO> ObterCataloguesAsync(IReadOnlyList<int>? empresaIds = null, int? usuarioId = null)
    {
        var result = new DashboardCataloguesDTO();

        List<int> empresaIdsPermitidas = [];
        if (usuarioId.HasValue)
        {
            var vinculos = await _usuarioEmpresaReaderService.GetVinculosPorUsuarioIdAsync(usuarioId.Value);
            empresaIdsPermitidas = vinculos.Select(v => v.EmpresaId).ToList();
        }

        var dimensoesEmpresa = await _dimensaoRepository.GetListByPredicateAsync<DimensaoEmpresa>(d => true, null, true);
        if (usuarioId.HasValue)
        {
            dimensoesEmpresa = dimensoesEmpresa.Where(d => empresaIdsPermitidas.Contains(d.EmpresaOrigemId)).ToList();
        }

        var empresaIdsFiltroExplicito = empresaIds?
            .Where(id => id > 0)
            .Distinct()
            .ToList() ?? [];

        if (empresaIdsFiltroExplicito.Count > 0)
        {
            dimensoesEmpresa = dimensoesEmpresa
                .Where(d => empresaIdsFiltroExplicito.Contains(d.EmpresaOrigemId))
                .ToList();
        }

        var empresasParaEquipes = dimensoesEmpresa
            .Select(d => d.EmpresaOrigemId)
            .ToList();

        var dimensoesEquipe = await _dimensaoRepository.GetListByPredicateAsync<DimensaoEquipe>(d => true, null, true);
        if (empresasParaEquipes.Count > 0)
        {
            dimensoesEquipe = dimensoesEquipe.Where(d => empresasParaEquipes.Contains(d.EmpresaId)).ToList();
        }

        var tiposEquipe = await _tipoEquipeReadService.GetTiposFixosAsync();
        var tipoEquipeLookup = tiposEquipe.ToDictionary(t => t.Id, t => t.Nome);

        var dimensoesVendedor = await _dimensaoRepository.GetListByPredicateAsync<DimensaoVendedor>(d => d.Ativo && !d.Excluido, null, true);
        if (empresasParaEquipes.Count > 0)
        {
            dimensoesVendedor = dimensoesVendedor
                .Where(d => d.EmpresaId.HasValue && empresasParaEquipes.Contains(d.EmpresaId.Value))
                .ToList();
        }

        foreach (var dimEmpresa in dimensoesEmpresa.OrderBy(e => e.Nome))
        {
            var empresaId = dimEmpresa.EmpresaOrigemId;
            var equipesDaEmpresa = dimensoesEquipe
                .Where(e => e.EmpresaId == empresaId)
                .OrderBy(e => e.Nome)
                .ToList();

            var equipeOrigemIds = equipesDaEmpresa.Select(e => e.EquipeOrigemId).ToList();
            var membrosAtivos = await _membroEquipeReaderService.ObterVendedoresAtivosPorEquipeIdsAsync(equipeOrigemIds);
            var membrosPorEquipeOrigemId = membrosAtivos
                .GroupBy(m => m.EquipeId)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.Usuario?.Nome ?? string.Empty).ToList());

            var usuariosEmAlgumaEquipeDaEmpresa = membrosAtivos.Select(m => m.UsuarioId).ToHashSet();

            var nodeEmpresa = new DashboardCatalogoEmpresaHierarquiaDTO
            {
                Id = empresaId,
                Nome = dimEmpresa.Nome,
                Equipes = equipesDaEmpresa
                    .Select(eq =>
                    {
                        var equipeOrigemId = eq.EquipeOrigemId;
                        membrosPorEquipeOrigemId.TryGetValue(equipeOrigemId, out var membros);
                        membros ??= [];

                        return new DashboardCatalogoEquipeHierarquiaDTO
                        {
                            Id = equipeOrigemId,
                            Nome = eq.Nome,
                            Tipo = eq.TipoEquipeId.HasValue && tipoEquipeLookup.TryGetValue(eq.TipoEquipeId.Value, out var tipo)
                                ? tipo
                                : string.Empty,
                            EmpresaId = empresaId,
                            Vendedores = membros
                                .Select(m => MapearMembroEquipe(m, equipeOrigemId, empresaId))
                                .ToList()
                        };
                    })
                    .ToList()
            };

            nodeEmpresa.VendedoresSemEquipe = dimensoesVendedor
                .Where(d => d.EmpresaId == empresaId && !usuariosEmAlgumaEquipeDaEmpresa.Contains(d.UsuarioOrigemId))
                .OrderBy(v => v.Nome)
                .Select(v => MapearDimensaoVendedorSemEquipe(v, empresaId))
                .ToList();

            result.Empresas.Add(nodeEmpresa);
        }

        var dimensoesOrigem = await _dimensaoRepository.GetListByPredicateAsync<DimensaoOrigem>(d => true, null, true);
        var origemIdsComFatos = await _dimensaoRepository.ObterIdsDimensaoOrigemReferenciadosEmFatosAsync();
        result.Origens = dimensoesOrigem
            .Where(d => !d.Excluido || origemIdsComFatos.Contains(d.Id))
            .Select(d => new DashboardCatalogoOrigemDTO { Id = d.OrigemOrigemId, Nome = d.Nome })
            .OrderBy(o => o.Nome)
            .ToList();

        var dimensoesCampanha = await _dimensaoRepository.GetListByPredicateAsync<DimensaoCampanha>(d => true, null, true);
        if (empresasParaEquipes.Count > 0)
        {
            var grupoIds = dimensoesEmpresa
                .Where(e => empresasParaEquipes.Contains(e.EmpresaOrigemId))
                .Select(e => e.GrupoEmpresaId)
                .Distinct()
                .ToList();

            dimensoesCampanha = dimensoesCampanha
                .Where(d => grupoIds.Contains(d.GrupoEmpresaId))
                .ToList();
        }

        result.Campanhas = dimensoesCampanha
            .Select(d => new DashboardCatalogoCampanhaDTO
            {
                Id = d.CampanhaOrigemId,
                Nome = d.Nome,
                EmpresaId = d.EmpresaId
            })
            .OrderBy(c => c.Nome)
            .ToList();

        var dimensoesStatusLead = await _dimensaoRepository.GetListByPredicateAsync<DimensaoStatusLead>(d => true, null, true);
        var statusIdsComFatos = await _dimensaoRepository.ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync();
        result.StatusLeads = dimensoesStatusLead
            .Where(d => !d.Excluido || statusIdsComFatos.Contains(d.Id))
            .Select(d => new DashboardCatalogoStatusLeadDTO
            {
                Id = d.StatusOrigemId,
                Nome = d.Nome,
                Cor = d.Cor ?? string.Empty
            })
            .OrderBy(s => s.Nome)
            .ToList();

        return result;
    }

    private static DashboardCatalogoVendedorDTO MapearMembroEquipe(MembroEquipe m, int equipeOrigemId, int empresaId)
    {
        var nome = m.Usuario?.Nome ?? string.Empty;
        return new DashboardCatalogoVendedorDTO
        {
            Id = m.UsuarioId,
            Nome = nome,
            NomeResponsavelResumido = NomeVendedorHelper.AbreviarNome(nome),
            EquipeId = equipeOrigemId,
            EmpresaId = empresaId
        };
    }

    private static DashboardCatalogoVendedorDTO MapearDimensaoVendedorSemEquipe(DimensaoVendedor d, int empresaId) =>
        new()
        {
            Id = d.UsuarioOrigemId,
            Nome = d.Nome,
            NomeResponsavelResumido = NomeVendedorHelper.AbreviarNome(d.Nome),
            EquipeId = 0,
            EmpresaId = empresaId
        };
}

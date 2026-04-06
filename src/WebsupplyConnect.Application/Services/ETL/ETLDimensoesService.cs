using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.ETL;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

namespace WebsupplyConnect.Application.Services.ETL;

public class ETLDimensoesService : IETLDimensoesService
{
    private readonly IDimensaoRepository _dimensaoRepository;
    private readonly IEmpresaReaderService _empresaReaderService;
    private readonly IEquipeReaderService _equipeReaderService;
    private readonly IUsuarioReaderService _usuarioReaderService;
    private readonly ILeadReaderService _leadReaderService;
    private readonly IOrigemReaderService _origemReaderService;
    private readonly ICampanhaReaderService _campanhaReaderService;
    private readonly IFunilReaderService _funilReaderService;
    private readonly IEtapaReaderService _etapaReaderService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ETLDimensoesService> _logger;

    public ETLDimensoesService(
        IDimensaoRepository dimensaoRepository,
        IEmpresaReaderService empresaReaderService,
        IEquipeReaderService equipeReaderService,
        IUsuarioReaderService usuarioReaderService,
        ILeadReaderService leadReaderService,
        IOrigemReaderService origemReaderService,
        ICampanhaReaderService campanhaReaderService,
        IFunilReaderService funilReaderService,
        IEtapaReaderService etapaReaderService,
        IUnitOfWork unitOfWork,
        ILogger<ETLDimensoesService> logger)
    {
        _dimensaoRepository = dimensaoRepository;
        _empresaReaderService = empresaReaderService;
        _equipeReaderService = equipeReaderService;
        _usuarioReaderService = usuarioReaderService;
        _leadReaderService = leadReaderService;
        _origemReaderService = origemReaderService;
        _campanhaReaderService = campanhaReaderService;
        _funilReaderService = funilReaderService;
        _etapaReaderService = etapaReaderService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SincronizarDimensaoTempoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        var dataAtual = new DateTime(dataInicio.Year, dataInicio.Month, dataInicio.Day, dataInicio.Hour, 0, 0);
        var fim = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, dataFim.Hour, 0, 0);

        while (dataAtual <= fim)
        {
            var existente = await _dimensaoRepository.ObterDimensaoTempoPorDataAsync(dataAtual, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoTempo(dataAtual);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
            dataAtual = dataAtual.AddHours(1);
        }
    }

    public async Task SincronizarDimensaoTempoAsync(IEnumerable<DateTime> datas,
        CancellationToken cancellationToken = default)
    {
        var datasTruncadas = datas
            .Select(d => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0))
            .Distinct()
            .ToList();

        foreach (var dataAtual in datasTruncadas)
        {
            var existente = await _dimensaoRepository.ObterDimensaoTempoPorDataAsync(dataAtual, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoTempo(dataAtual);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
        }
    }

    public async Task SincronizarDimensaoEmpresaAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresaReaderService.ObterTodasNaoExcluidasParaETLAsync();
        _logger.LogDebug("Sincronizando dimensão Empresa: {Count} empresas na fonte", empresas.Count);
        foreach (var emp in empresas)
        {
            var existente = await _dimensaoRepository.ObterDimensaoEmpresaPorOrigemIdAsync(emp.Id, cancellationToken);
            var nome = emp.Nome ?? "";
            if (existente == null)
            {
                var dimensao = new DimensaoEmpresa(emp.Id, nome, emp.Ativo, emp.GrupoEmpresaId);
                await _dimensaoRepository.CreateAsync(dimensao);
                continue;
            }

            existente.Atualizar(nome, emp.Ativo, emp.GrupoEmpresaId);
            _dimensaoRepository.Update<DimensaoEmpresa>(existente);
        }
    }

    public async Task SincronizarDimensaoEquipeAsync(CancellationToken cancellationToken = default)
    {
        var equipes = await _equipeReaderService.ObterEquipesNaoExcluidasParaETLAsync();
        foreach (var eq in equipes)
        {
            var existente = await _dimensaoRepository.ObterDimensaoEquipePorOrigemIdAsync(eq.Id, cancellationToken);
            var nome = eq.Nome ?? "";
            if (existente == null)
            {
                var dimensao = new DimensaoEquipe(eq.Id, nome, eq.TipoEquipeId, eq.EmpresaId, eq.Ativa);
                await _dimensaoRepository.CreateAsync(dimensao);
                continue;
            }

            existente.Atualizar(nome, eq.TipoEquipeId, eq.EmpresaId, eq.Ativa);
            _dimensaoRepository.Update<DimensaoEquipe>(existente);
        }
    }

    public async Task SincronizarDimensaoVendedorAsync(DateTime? ultimaDataProcessada, CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioReaderService.ObterUsuariosAtivosNaoBotParaETLAsync();

        // Montar lookup de equipe/empresa por usuário a partir das equipes transacionais
        var empresas = await _empresaReaderService.ObterTodasNaoExcluidasParaETLAsync();
        var membroLookup = new Dictionary<int, (int? EquipeOrigemId, int? EmpresaId)>();
        foreach (var emp in empresas)
        {
            var equipesComMembros = await _equipeReaderService.ObterEquipesComMembrosPorEmpresaParaETLAsync(emp.Id);
            foreach (var eq in equipesComMembros)
            {
                if (eq.Membros == null) continue;
                foreach (var membro in eq.Membros)
                {
                    // Se o usuário já está em outra equipe, manter a primeira (estável)
                    if (!membroLookup.ContainsKey(membro.UsuarioId))
                    {
                        membroLookup[membro.UsuarioId] = (eq.Id, emp.Id);
                    }
                }
            }
        }

        foreach (var u in usuarios)
        {
            var (equipeOrigemId, empresaId) = membroLookup.GetValueOrDefault(u.Id, (null, null));

            // Resolver o ID da DimensaoEquipe a partir do EquipeOrigemId transacional
            int? equipeDimensaoId = null;
            if (equipeOrigemId.HasValue)
            {
                var equipeDim = await _dimensaoRepository.ObterDimensaoEquipePorOrigemIdAsync(equipeOrigemId.Value, cancellationToken);
                equipeDimensaoId = equipeDim?.Id;
            }

            var existente = await _dimensaoRepository.ObterDimensaoVendedorPorOrigemIdAsync(u.Id, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoVendedor(u.Id, u.Nome, u.Email ?? "", equipeDimensaoId, empresaId, u.Ativo);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
            else
            {
                existente.Atualizar(u.Nome, u.Email ?? "", equipeDimensaoId, empresaId, u.Ativo);
                _dimensaoRepository.Update<DimensaoVendedor>(existente);
            }
        }
    }

    public async Task SincronizarDimensaoStatusLeadAsync(CancellationToken cancellationToken = default)
    {
        var statusList = await _leadReaderService.ListarStatusLeadEntidadesAsync();
        foreach (var s in statusList)
        {
            var existente = await _dimensaoRepository.ObterDimensaoStatusLeadPorOrigemIdAsync(s.Id, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoStatusLead(s.Id, s.Codigo, s.Nome ?? "", s.Cor, s.Ordem);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
        }
    }

    public async Task SincronizarDimensaoOrigemAsync(CancellationToken cancellationToken = default)
    {
        var origens = await _origemReaderService.ListarTodasOrigensParaETLAsync();
        _logger.LogDebug("Sincronizando dimensão Origem: {Count} origens na fonte (incl. excluídas)", origens.Count);
        foreach (var o in origens)
        {
            var existente = await _dimensaoRepository.ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(o.Id, cancellationToken);
            var nome = o.Nome ?? "";

            if (o.Excluido)
            {
                if (existente == null)
                    continue;

                existente.Atualizar(nome, o.OrigemTipoId, o.Descricao);
                if (!existente.Excluido)
                    existente.ExcluirLogicamente();
                _dimensaoRepository.Update<DimensaoOrigem>(existente);
                continue;
            }

            if (existente == null)
            {
                var dimensao = new DimensaoOrigem(o.Id, nome, o.OrigemTipoId, o.Descricao);
                await _dimensaoRepository.CreateAsync(dimensao);
                continue;
            }

            existente.Atualizar(nome, o.OrigemTipoId, o.Descricao);
            if (existente.Excluido)
                existente.RestaurarExclusaoLogica();
            _dimensaoRepository.Update<DimensaoOrigem>(existente);
        }
    }

    /// <summary>
    /// Sincroniza a dimensão OLAP a partir das campanhas transacionais.
    /// O atributo <c>Nome</c> da entidade é o rótulo analítico e a chave de agrupamento entre empresas no dashboard (modo PorNomeCampanha).
    /// A unicidade por empresa permanece em <c>CampanhaOrigemId</c> (ID transacional).
    /// </summary>
    public async Task SincronizarDimensaoCampanhaAsync(CancellationToken cancellationToken = default)
    {
        var campanhas = await _campanhaReaderService.ListarCampanhasNaoExcluidasParaETLAsync();
        var empresas = await _empresaReaderService.ObterTodasNaoExcluidasParaETLAsync();
        var empresaGrupoLookup = empresas.ToDictionary(e => e.Id, e => e.GrupoEmpresaId);

        foreach (var c in campanhas)
        {
            if (!empresaGrupoLookup.TryGetValue(c.EmpresaId, out var grupoEmpresaId))
                continue;

            var existente = await _dimensaoRepository.ObterDimensaoCampanhaPorOrigemIdAsync(c.Id, cancellationToken);
            if (existente == null)
            {
                var novaDimensao = new DimensaoCampanha(c.Id, c.Nome ?? "", c.Codigo,
                    c.Ativo, c.Temporaria, c.EmpresaId, grupoEmpresaId);
                await _dimensaoRepository.CreateAsync(novaDimensao);
                continue;
            }

            existente.Atualizar(c.Nome ?? "", c.Codigo, c.Ativo, c.Temporaria, c.EmpresaId, grupoEmpresaId);
            _dimensaoRepository.Update<DimensaoCampanha>(existente);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SincronizarDimensaoFunilAsync(CancellationToken cancellationToken = default)
    {
        var funis = await _funilReaderService.ListarFunisParaETLAsync(cancellationToken);
        _logger.LogDebug("Sincronizando dimensão Funil: {Count} funis na fonte", funis.Count);
        foreach (var f in funis)
        {
            var existente = await _dimensaoRepository.ObterDimensaoFunilPorOrigemIdAsync(f.Id, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoFunil(f.Id, f.EmpresaId, f.Nome, f.Ativo, f.EhPadrao, f.Cor);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
            else
            {
                existente.Atualizar(f.Nome, f.Ativo, f.EhPadrao, f.Cor);
                _dimensaoRepository.Update<DimensaoFunil>(existente);
            }
        }
    }

    public async Task SincronizarDimensaoEtapaFunilAsync(CancellationToken cancellationToken = default)
    {
        var etapas = await _etapaReaderService.ListarEtapasParaETLAsync(cancellationToken);
        _logger.LogDebug("Sincronizando dimensão EtapaFunil: {Count} etapas na fonte", etapas.Count);
        foreach (var e in etapas)
        {
            var funilDim = await _dimensaoRepository.ObterDimensaoFunilPorOrigemIdAsync(e.FunilId, cancellationToken);
            if (funilDim == null)
            {
                _logger.LogWarning("Etapa {EtapaId} ignorada na dimensão OLAP: funil {FunilId} não encontrado.", e.Id, e.FunilId);
                continue;
            }

            var existente = await _dimensaoRepository.ObterDimensaoEtapaFunilPorOrigemIdAsync(e.Id, cancellationToken);
            if (existente == null)
            {
                var dimensao = new DimensaoEtapaFunil(
                    e.Id,
                    funilDim.Id,
                    e.FunilId,
                    e.Nome,
                    e.Ordem,
                    e.Cor,
                    e.ProbabilidadePadrao,
                    e.EhAtiva,
                    e.EhFinal,
                    e.EhVitoria,
                    e.EhPerdida,
                    e.EhExibida,
                    e.Ativo);
                await _dimensaoRepository.CreateAsync(dimensao);
            }
            else
            {
                existente.Atualizar(
                    funilDim.Id,
                    e.FunilId,
                    e.Nome,
                    e.Ordem,
                    e.Cor,
                    e.ProbabilidadePadrao,
                    e.EhAtiva,
                    e.EhFinal,
                    e.EhVitoria,
                    e.EhPerdida,
                    e.EhExibida,
                    e.Ativo);
                _dimensaoRepository.Update<DimensaoEtapaFunil>(existente);
            }
        }
    }

    public async Task<DimensaoTempo?> ObterDimensaoTempoPorDataAsync(DateTime data, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoTempoPorDataAsync(data, cancellationToken);

    public async Task<DimensaoEmpresa?> ObterDimensaoEmpresaPorOrigemIdAsync(int empresaOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoEmpresaPorOrigemIdAsync(empresaOrigemId, cancellationToken);

    public async Task<DimensaoEquipe?> ObterDimensaoEquipePorOrigemIdAsync(int equipeOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoEquipePorOrigemIdAsync(equipeOrigemId, cancellationToken);

    public async Task<DimensaoEquipe?> ObterDimensaoEquipePorIdAsync(int dimensaoEquipeId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetByIdAsync<DimensaoEquipe>(dimensaoEquipeId, includeDeleted: false);

    public async Task<DimensaoVendedor?> ObterDimensaoVendedorPorOrigemIdAsync(int usuarioOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoVendedorPorOrigemIdAsync(usuarioOrigemId, cancellationToken);

    public async Task<DimensaoStatusLead?> ObterDimensaoStatusLeadPorOrigemIdAsync(int statusOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoStatusLeadPorOrigemIdAsync(statusOrigemId, cancellationToken);

    public async Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdAsync(int origemOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoOrigemPorOrigemIdAsync(origemOrigemId, cancellationToken);

    public async Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(int origemOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(origemOrigemId, cancellationToken);

    public async Task<DimensaoCampanha?> ObterDimensaoCampanhaPorOrigemIdAsync(int campanhaOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoCampanhaPorOrigemIdAsync(campanhaOrigemId, cancellationToken);

    public async Task<DimensaoFunil?> ObterDimensaoFunilPorOrigemIdAsync(int funilOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoFunilPorOrigemIdAsync(funilOrigemId, cancellationToken);

    public async Task<DimensaoEtapaFunil?> ObterDimensaoEtapaFunilPorOrigemIdAsync(int etapaOrigemId, CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensaoEtapaFunilPorOrigemIdAsync(etapaOrigemId, cancellationToken);

    public async Task<List<DimensaoEtapaFunil>> ObterDimensoesEtapaFunilNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.ObterDimensoesEtapaFunilNaoExcluidasAsync(cancellationToken);

    public Task<Dictionary<int, string>> ObterNomesFunilPorDimensaoFunilIdsAsync(
        IEnumerable<int> dimensaoFunilIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterNomesFunilPorDimensaoFunilIdsAsync(dimensaoFunilIds, cancellationToken);

    public async Task<List<DimensaoStatusLead>> ObterDimensoesStatusNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoStatusLead>(d => !d.Excluido);

    public async Task<List<DimensaoOrigem>> ObterDimensoesOrigemNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoOrigem>(d => !d.Excluido);

    public async Task<List<DimensaoCampanha>> ObterDimensoesCampanhaNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoCampanha>(d => !d.Excluido);

    public async Task<List<DimensaoEmpresa>> ObterDimensoesEmpresaNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoEmpresa>(d => !d.Excluido);

    public async Task<List<DimensaoEquipe>> ObterDimensoesEquipeNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoEquipe>(d => !d.Excluido);

    public async Task<List<DimensaoVendedor>> ObterDimensoesVendedorNaoExcluidasAsync(CancellationToken cancellationToken = default) =>
        await _dimensaoRepository.GetListByPredicateAsync<DimensaoVendedor>(d => !d.Excluido);

    public Task<HashSet<int>> ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(cancellationToken);

    public Task<HashSet<int>> ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(cancellationToken);

    public async Task<(bool ok, string? detalhe)> ValidarFiltroOrigemOrigemIdsParaDashboardAsync(
        IReadOnlyList<int> origemOrigemIds, CancellationToken cancellationToken = default)
    {
        if (origemOrigemIds.Count == 0)
            return (true, null);

        var ids = origemOrigemIds.Distinct().ToList();
        var refs = await _dimensaoRepository.ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(cancellationToken);
        var candidatos = await _dimensaoRepository.GetListByPredicateAsync<DimensaoOrigem>(
            d => ids.Contains(d.OrigemOrigemId), null, includeDeleted: true);
        var permitidos = candidatos
            .Where(d => !d.Excluido || refs.Contains(d.Id))
            .Select(d => d.OrigemOrigemId)
            .ToHashSet();
        if (permitidos.Count == ids.Count)
            return (true, null);

        var naoEncontrados = ids.Where(id => !permitidos.Contains(id)).Order().ToList();
        var encontradasStr = candidatos.Count > 0
            ? string.Join("; ", candidatos.OrderBy(o => o.OrigemOrigemId).Select(o =>
                $"origemOrigemId={o.OrigemOrigemId}, dimensaoOrigemId={o.Id}, excluido={o.Excluido}"))
            : "(nenhuma)";
        return (false,
            $"Origem(ns) de lead inexistente(s) no OLAP, excluída(s) sem fatos, ou fora do escopo. origemOrigemId(s) rejeitado(s): [{string.Join(", ", naoEncontrados)}]; encontrada(s) no OLAP: {encontradasStr}.");
    }

    public async Task<(bool ok, string? detalhe)> ValidarFiltroStatusLeadOrigemIdsParaDashboardAsync(
        IReadOnlyList<int> statusOrigemIds, CancellationToken cancellationToken = default)
    {
        if (statusOrigemIds.Count == 0)
            return (true, null);

        var ids = statusOrigemIds.Distinct().ToList();
        var refs = await _dimensaoRepository.ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(cancellationToken);
        var candidatos = await _dimensaoRepository.GetListByPredicateAsync<DimensaoStatusLead>(
            d => ids.Contains(d.StatusOrigemId), null, includeDeleted: true);
        var permitidos = candidatos
            .Where(d => !d.Excluido || refs.Contains(d.Id))
            .Select(d => d.StatusOrigemId)
            .ToHashSet();
        if (permitidos.Count == ids.Count)
            return (true, null);

        var naoEncontrados = ids.Where(id => !permitidos.Contains(id)).Order().ToList();
        return (false,
            $"Status de lead inexistente(s) no OLAP, excluído(s) sem fatos, ou fora do escopo. statusOrigemId(s) rejeitado(s): [{string.Join(", ", naoEncontrados)}].");
    }

    public async Task<List<DimensaoOrigem>> ObterDimensoesOrigemParaFiltroDashboardPorOrigemIdsAsync(
        IReadOnlyList<int> origemOrigemIds, CancellationToken cancellationToken = default)
    {
        if (origemOrigemIds.Count == 0)
            return [];

        var ids = origemOrigemIds.Distinct().ToList();
        var refs = await _dimensaoRepository.ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(cancellationToken);
        var candidatos = await _dimensaoRepository.GetListByPredicateAsync<DimensaoOrigem>(
            d => ids.Contains(d.OrigemOrigemId), null, includeDeleted: true);
        return candidatos.Where(d => !d.Excluido || refs.Contains(d.Id)).ToList();
    }

    public async Task<List<DimensaoStatusLead>> ObterDimensoesStatusLeadParaFiltroDashboardPorStatusOrigemIdsAsync(
        IReadOnlyList<int> statusOrigemIds, CancellationToken cancellationToken = default)
    {
        if (statusOrigemIds.Count == 0)
            return [];

        var ids = statusOrigemIds.Distinct().ToList();
        var refs = await _dimensaoRepository.ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(cancellationToken);
        var candidatos = await _dimensaoRepository.GetListByPredicateAsync<DimensaoStatusLead>(
            d => ids.Contains(d.StatusOrigemId), null, includeDeleted: true);
        return candidatos.Where(d => !d.Excluido || refs.Contains(d.Id)).ToList();
    }

    public async Task<List<DimensaoOrigem>> ObterDimensoesOrigemPorDimensaoIdsIncluindoExcluidasAsync(
        IReadOnlyList<int> dimensaoIds, CancellationToken cancellationToken = default)
    {
        if (dimensaoIds.Count == 0)
            return [];

        var ids = dimensaoIds.Distinct().ToList();
        return await _dimensaoRepository.GetListByPredicateAsync<DimensaoOrigem>(
            d => ids.Contains(d.Id), null, includeDeleted: true);
    }

    public async Task<List<DimensaoStatusLead>> ObterDimensoesStatusLeadPorDimensaoIdsIncluindoExcluidasAsync(
        IReadOnlyList<int> dimensaoIds, CancellationToken cancellationToken = default)
    {
        if (dimensaoIds.Count == 0)
            return [];

        var ids = dimensaoIds.Distinct().ToList();
        return await _dimensaoRepository.GetListByPredicateAsync<DimensaoStatusLead>(
            d => ids.Contains(d.Id), null, includeDeleted: true);
    }

    public Task<IReadOnlyDictionary<DateTime, DimensaoTempo>> ObterDimensoesTempoPorDatasAsync(
        IReadOnlyCollection<DateTime> datas, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesTempoPorDatasAsync(datas, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoEmpresa>> ObterDimensoesEmpresaPorOrigemIdsAsync(
        IReadOnlyCollection<int> empresaOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesEmpresaPorOrigemIdsAsync(empresaOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesOrigemPorOrigemIdsAsync(origemOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsIncluindoExcluidasAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesOrigemPorOrigemIdsIncluindoExcluidasAsync(origemOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoEquipe>> ObterDimensoesEquipePorOrigemIdsAsync(
        IReadOnlyCollection<int> equipeOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesEquipePorOrigemIdsAsync(equipeOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoVendedor>> ObterDimensoesVendedorPorOrigemIdsAsync(
        IReadOnlyCollection<int> usuarioOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesVendedorPorOrigemIdsAsync(usuarioOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoStatusLead>> ObterDimensoesStatusLeadPorOrigemIdsAsync(
        IReadOnlyCollection<int> statusOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesStatusLeadPorOrigemIdsAsync(statusOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoCampanha>> ObterDimensoesCampanhaPorOrigemIdsAsync(
        IReadOnlyCollection<int> campanhaOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesCampanhaPorOrigemIdsAsync(campanhaOrigemIds, cancellationToken);

    public Task<IReadOnlyDictionary<int, DimensaoEtapaFunil>> ObterDimensoesEtapaFunilPorOrigemIdsAsync(
        IReadOnlyCollection<int> etapaOrigemIds, CancellationToken cancellationToken = default) =>
        _dimensaoRepository.ObterDimensoesEtapaFunilPorOrigemIdsAsync(etapaOrigemIds, cancellationToken);
}

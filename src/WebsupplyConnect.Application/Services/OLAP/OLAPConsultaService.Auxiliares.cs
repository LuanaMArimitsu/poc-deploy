using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Dashboard;
using WebsupplyConnect.Application.Helpers;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.OLAP;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.OLAP.Controle;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Produto;


namespace WebsupplyConnect.Application.Services.OLAP;

public partial class OLAPConsultaService
{
    #region Auxiliares

    private async Task<int?> ResolverEmpresaDimensaoIdAsync(int? empresaOrigemId)
    {
        if (!empresaOrigemId.HasValue) return null;
        var dim = await _dimensoesService.ObterDimensaoEmpresaPorOrigemIdAsync(empresaOrigemId.Value);
        return dim?.Id;
    }

    /// <summary>
    /// Obtém fatos de lead filtrados por período considerando a data do último evento.
    /// Usado em todos os endpoints do dashboard que filtram leads e indicadores relacionados.
    /// </summary>
    private async Task<List<FatoLeadAgregado>> ObterFatosLeadFiltradosPorDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId, FiltrosDashboardDTO filtros)
    {
        var fatos = await _fatoLeadRepository.ObterPorPeriodoDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId);
        return await AplicarFiltrosAdicionaisLeadAsync(fatos, filtros);
    }

    private async Task<List<FatoOportunidadeMetrica>> ObterFatosOportunidadeFiltradosAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId, FiltrosDashboardDTO filtros)
    {
        var fatos = await _fatoOportunidadeRepository.ObterPorPeriodoAsync(dataInicio, dataFim, empresaDimensaoId);
        return await AplicarFiltrosAdicionaisOportunidadeAsync(fatos, filtros);
    }

    private async Task<List<FatoEventoAgregado>> ObterFatosEventoFiltradosAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId, FiltrosDashboardDTO filtros)
    {
        var fatos = await _fatoEventoRepository.ObterPorPeriodoAsync(dataInicio, dataFim, empresaDimensaoId);
        return await AplicarFiltrosAdicionaisEventoAsync(fatos, filtros);
    }

    /// <summary>
    /// Obtém fatos de evento filtrados por período considerando a data do último evento do lead.
    /// Usado nos indicadores de campanha.
    /// </summary>
    private async Task<List<FatoEventoAgregado>> ObterFatosEventoFiltradosPorDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId, FiltrosDashboardDTO filtros)
    {
        var fatos = await _fatoEventoRepository.ObterPorPeriodoDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId);
        return await AplicarFiltrosAdicionaisEventoAsync(fatos, filtros);
    }

    private async Task<List<FatoOportunidadeMetrica>> ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId, FiltrosDashboardDTO filtros)
    {
        var fatos = await _fatoOportunidadeRepository.ObterPorPeriodoDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId);
        return await AplicarFiltrosAdicionaisOportunidadeAsync(fatos, filtros);
    }

    /// <summary>
    /// Leads cujo responsável é usuário bot não entram em totalizadores/retornos OLAP de leads.
    /// </summary>
    private async Task<HashSet<int>> ObterLeadIdsAtribuidosABotAsync(IEnumerable<int> leadIds)
    {
        var ids = leadIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var leads = await _leadReaderService.ObterLeadsComResponsavelUsuarioPorIdsAsync(ids, includeDeleted: true);
        return leads.Where(l => _leadReaderService.LeadPertenceAoBot(l)).Select(l => l.Id).ToHashSet();
    }

    private async Task<List<FatoLeadAgregado>> AplicarFiltrosAdicionaisLeadAsync(
        List<FatoLeadAgregado> fatos, FiltrosDashboardDTO filtros)
    {
        var filtrosDimensao = await ObterFiltrosDimensaoIdsAsync(filtros);
        if (filtrosDimensao.SemResultados)
            return [];

        var filtrados = AplicarFiltrosComuns(
                fatos,
                filtrosDimensao,
                f => f.EmpresaId,
                f => f.EquipeId,
                f => f.VendedorId,
                f => f.OrigemId,
                f => f.CampanhaId,
                f => f.StatusAtualId,
                obterEtapaFunilDimId: null)
            .ToList();

        if (filtrados.Count == 0)
            return filtrados;

        var botIds = await ObterLeadIdsAtribuidosABotAsync(filtrados.Select(f => f.LeadId));
        if (botIds.Count == 0)
            return filtrados;

        return filtrados.Where(f => !botIds.Contains(f.LeadId)).ToList();
    }

    private async Task<List<FatoOportunidadeMetrica>> AplicarFiltrosAdicionaisOportunidadeAsync(
        List<FatoOportunidadeMetrica> fatos, FiltrosDashboardDTO filtros)
    {
        var filtrosDimensao = await ObterFiltrosDimensaoIdsAsync(filtros);
        if (filtrosDimensao.SemResultados)
            return [];

        return AplicarFiltrosComuns(
                fatos,
                filtrosDimensao,
                f => f.EmpresaId,
                f => f.EquipeId,
                f => f.VendedorId,
                f => f.OrigemId,
                f => f.CampanhaId,
                f => f.StatusLeadId,
                f => f.DimensaoEtapaFunilId)
            .ToList();
    }

    private async Task<List<FatoEventoAgregado>> AplicarFiltrosAdicionaisEventoAsync(
        List<FatoEventoAgregado> fatos, FiltrosDashboardDTO filtros)
    {
        var filtrosDimensao = await ObterFiltrosDimensaoIdsAsync(filtros);
        if (filtrosDimensao.SemResultados)
            return [];

        var filtrados = AplicarFiltrosComuns(
                fatos,
                filtrosDimensao,
                f => f.EmpresaId,
                f => f.EquipeId,
                f => f.VendedorId,
                f => f.OrigemId,
                f => f.CampanhaId,
                f => f.StatusAtualId,
                obterEtapaFunilDimId: null)
            .ToList();

        if (filtrados.Count == 0)
            return filtrados;

        var botIds = await ObterLeadIdsAtribuidosABotAsync(filtrados.Select(f => f.LeadId));
        if (botIds.Count == 0)
            return filtrados;

        return filtrados.Where(f => !botIds.Contains(f.LeadId)).ToList();
    }

    private async Task<FiltrosDimensaoIds> ObterFiltrosDimensaoIdsAsync(FiltrosDashboardDTO filtros)
    {
        var resultado = new FiltrosDimensaoIds();

        var empresaOrigemIds = filtros.ObterEmpresaIds();
        if (empresaOrigemIds.Count > 0)
        {
            resultado.FiltrarEmpresa = true;
            foreach (var empresaOrigemId in empresaOrigemIds)
            {
                var dim = await _dimensoesService.ObterDimensaoEmpresaPorOrigemIdAsync(empresaOrigemId);
                if (dim != null)
                    resultado.EmpresaDimIds.Add(dim.Id);
            }
        }

        var equipeOrigemIds = filtros.ObterEquipeIds();
        if (equipeOrigemIds.Count > 0)
        {
            resultado.FiltrarEquipe = true;
            var dimensoesEquipe = await _dimensoesService.ObterDimensoesEquipeNaoExcluidasAsync();
            foreach (var id in dimensoesEquipe
                         .Where(d => equipeOrigemIds.Contains(d.EquipeOrigemId))
                         .Select(d => d.Id)
                         .Distinct())
            {
                resultado.EquipeDimIds.Add(id);
            }
        }

        var vendedorOrigemIds = filtros.ObterVendedorIds();
        if (vendedorOrigemIds.Count > 0)
        {
            resultado.FiltrarVendedor = true;
            var dimensoesVendedor = await _dimensoesService.ObterDimensoesVendedorNaoExcluidasAsync();
            foreach (var id in dimensoesVendedor
                         .Where(d => vendedorOrigemIds.Contains(d.UsuarioOrigemId))
                         .Select(d => d.Id)
                         .Distinct())
            {
                resultado.VendedorDimIds.Add(id);
            }
        }

        var origemOrigemIds = filtros.ObterOrigemIds();
        if (origemOrigemIds.Count > 0)
        {
            resultado.FiltrarOrigem = true;
            var dimensoesOrigem = await _dimensoesService.ObterDimensoesOrigemParaFiltroDashboardPorOrigemIdsAsync(origemOrigemIds);
            foreach (var id in dimensoesOrigem.Select(d => d.Id).Distinct())
                resultado.OrigemDimIds.Add(id);
        }

        var nomesCampanhaFiltro = filtros.ObterCampanhaNomes();
        if (nomesCampanhaFiltro.Count > 0)
        {
            resultado.FiltrarCampanha = true;
            var dimensoesCampanha = await _dimensoesService.ObterDimensoesCampanhaNaoExcluidasAsync();
            var setNomes = new HashSet<string>(nomesCampanhaFiltro, StringComparer.OrdinalIgnoreCase);
            var empresasFiltro = filtros.ObterEmpresaIds();
            IEnumerable<DimensaoCampanha> candidatosCampanha = dimensoesCampanha;
            if (empresasFiltro.Count > 0)
                candidatosCampanha = candidatosCampanha.Where(d => empresasFiltro.Contains(d.EmpresaId));
            foreach (var id in candidatosCampanha
                         .Where(d => setNomes.Contains(d.Nome.Trim()))
                         .Select(d => d.Id)
                         .Distinct())
            {
                resultado.CampanhaDimIds.Add(id);
            }
        }

        var statusLeadOrigemIds = filtros.ObterStatusLeadIds();
        if (statusLeadOrigemIds.Count > 0)
        {
            resultado.FiltrarStatus = true;
            var dimensoesStatusLead =
                await _dimensoesService.ObterDimensoesStatusLeadParaFiltroDashboardPorStatusOrigemIdsAsync(statusLeadOrigemIds);
            foreach (var id in dimensoesStatusLead.Select(d => d.Id).Distinct())
                resultado.StatusDimIds.Add(id);
        }

        var funilOrigemIds = filtros.ObterFunilIds();
        var etapaOrigemIds = filtros.ObterEtapaIds();
        if (funilOrigemIds.Count > 0 || etapaOrigemIds.Count > 0)
        {
            resultado.FiltrarEtapaFunil = true;
            var todasEtapas = await _dimensoesService.ObterDimensoesEtapaFunilNaoExcluidasAsync();
            IEnumerable<DimensaoEtapaFunil> candidatos = todasEtapas;
            if (funilOrigemIds.Count > 0)
                candidatos = candidatos.Where(e => funilOrigemIds.Contains(e.FunilOrigemId));
            if (etapaOrigemIds.Count > 0)
                candidatos = candidatos.Where(e => etapaOrigemIds.Contains(e.EtapaOrigemId));
            foreach (var id in candidatos.Select(e => e.Id).Distinct())
                resultado.EtapaFunilDimIds.Add(id);
        }

        return resultado;
    }

    private static IEnumerable<TFato> AplicarFiltrosComuns<TFato>(
        IEnumerable<TFato> fatos,
        FiltrosDimensaoIds filtros,
        Func<TFato, int> obterEmpresaId,
        Func<TFato, int?> obterEquipeId,
        Func<TFato, int?> obterVendedorId,
        Func<TFato, int> obterOrigemId,
        Func<TFato, int?> obterCampanhaId,
        Func<TFato, int?> obterStatusId,
        Func<TFato, int?>? obterEtapaFunilDimId = null)
    {
        if ((filtros.FiltrarEmpresa && filtros.EmpresaDimIds.Count == 0) ||
            (filtros.FiltrarEquipe && filtros.EquipeDimIds.Count == 0) ||
            (filtros.FiltrarVendedor && filtros.VendedorDimIds.Count == 0) ||
            (filtros.FiltrarOrigem && filtros.OrigemDimIds.Count == 0) ||
            (filtros.FiltrarCampanha && filtros.CampanhaDimIds.Count == 0) ||
            (filtros.FiltrarStatus && filtros.StatusDimIds.Count == 0) ||
            (filtros.FiltrarEtapaFunil && filtros.EtapaFunilDimIds.Count == 0))
        {
            return [];
        }

        var resultado = fatos;

        if (filtros.FiltrarEmpresa)
            resultado = resultado.Where(f => filtros.EmpresaDimIds.Contains(obterEmpresaId(f)));

        if (filtros.FiltrarEquipe)
            resultado = resultado.Where(f => obterEquipeId(f).HasValue && filtros.EquipeDimIds.Contains(obterEquipeId(f)!.Value));

        if (filtros.FiltrarVendedor)
            resultado = resultado.Where(f => obterVendedorId(f).HasValue && filtros.VendedorDimIds.Contains(obterVendedorId(f)!.Value));

        if (filtros.FiltrarOrigem)
            resultado = resultado.Where(f => filtros.OrigemDimIds.Contains(obterOrigemId(f)));

        if (filtros.FiltrarCampanha)
            resultado = resultado.Where(f => obterCampanhaId(f).HasValue && filtros.CampanhaDimIds.Contains(obterCampanhaId(f)!.Value));

        if (filtros.FiltrarStatus)
            resultado = resultado.Where(f => obterStatusId(f).HasValue && filtros.StatusDimIds.Contains(obterStatusId(f)!.Value));

        if (filtros.FiltrarEtapaFunil && obterEtapaFunilDimId != null)
            resultado = resultado.Where(f =>
            {
                var idEtapa = obterEtapaFunilDimId(f);
                return idEtapa.HasValue && filtros.EtapaFunilDimIds.Contains(idEtapa.Value);
            });

        return resultado;
    }

    private sealed class FiltrosDimensaoIds
    {
        public bool FiltrarEmpresa { get; set; }
        public bool FiltrarEquipe { get; set; }
        public bool FiltrarVendedor { get; set; }
        public bool FiltrarOrigem { get; set; }
        public bool FiltrarCampanha { get; set; }
        public bool FiltrarStatus { get; set; }
        public bool FiltrarEtapaFunil { get; set; }

        public HashSet<int> EmpresaDimIds { get; } = [];
        public HashSet<int> EquipeDimIds { get; } = [];
        public HashSet<int> VendedorDimIds { get; } = [];
        public HashSet<int> OrigemDimIds { get; } = [];
        public HashSet<int> CampanhaDimIds { get; } = [];
        public HashSet<int> StatusDimIds { get; } = [];
        public HashSet<int> EtapaFunilDimIds { get; } = [];

        public bool SemResultados =>
            (FiltrarEmpresa && EmpresaDimIds.Count == 0) ||
            (FiltrarEquipe && EquipeDimIds.Count == 0) ||
            (FiltrarVendedor && VendedorDimIds.Count == 0) ||
            (FiltrarOrigem && OrigemDimIds.Count == 0) ||
            (FiltrarCampanha && CampanhaDimIds.Count == 0) ||
            (FiltrarStatus && StatusDimIds.Count == 0) ||
            (FiltrarEtapaFunil && EtapaFunilDimIds.Count == 0);

        public FatoLeadAgregadoConsultaFiltro ToFatoLeadConsultaFiltro(
            DateTime dataInicio, DateTime dataFim, int? empresaDimensaoId)
        {
            return new FatoLeadAgregadoConsultaFiltro
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                EmpresaDimensaoId = empresaDimensaoId,
                FiltrarEmpresa = FiltrarEmpresa,
                FiltrarEquipe = FiltrarEquipe,
                FiltrarVendedor = FiltrarVendedor,
                FiltrarOrigem = FiltrarOrigem,
                FiltrarCampanha = FiltrarCampanha,
                FiltrarStatus = FiltrarStatus,
                FiltrarEtapaFunil = FiltrarEtapaFunil,
                EmpresaDimIds = new HashSet<int>(EmpresaDimIds),
                EquipeDimIds = new HashSet<int>(EquipeDimIds),
                VendedorDimIds = new HashSet<int>(VendedorDimIds),
                OrigemDimIds = new HashSet<int>(OrigemDimIds),
                CampanhaDimIds = new HashSet<int>(CampanhaDimIds),
                StatusDimIds = new HashSet<int>(StatusDimIds),
                EtapaFunilDimIds = new HashSet<int>(EtapaFunilDimIds)
            };
        }
    }

    private async Task<Dictionary<int, DimensaoStatusLead?>> ObterDimensoesStatusLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoStatusLead?>();
        var todos = await _dimensoesService.ObterDimensoesStatusLeadPorDimensaoIdsIncluindoExcluidasAsync(ids);
        return todos.ToDictionary(d => d.Id, d => (DimensaoStatusLead?)d);
    }

    private async Task<Dictionary<int, DimensaoOrigem?>> ObterDimensoesOrigemLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoOrigem?>();
        var todos = await _dimensoesService.ObterDimensoesOrigemPorDimensaoIdsIncluindoExcluidasAsync(ids);
        return todos.ToDictionary(d => d.Id, d => (DimensaoOrigem?)d);
    }

    private async Task<Dictionary<int, DimensaoCampanha?>> ObterDimensoesCampanhaLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoCampanha?>();
        var todos = await _dimensoesService.ObterDimensoesCampanhaNaoExcluidasAsync();
        return todos.Where(d => ids.Contains(d.Id)).ToDictionary(d => d.Id, d => (DimensaoCampanha?)d);
    }

    private static string ResolverNomeCampanhaParaAgrupamento(DimensaoCampanha? dim, int dimensaoCampanhaId)
    {
        var n = dim?.Nome?.Trim();
        if (!string.IsNullOrEmpty(n))
            return n;
        return dim != null ? $"Campanha {dim.CampanhaOrigemId}" : $"Campanha {dimensaoCampanhaId}";
    }

    /// <summary>Chave estável para agrupar fatos por empresa (dimensão OLAP) e nome de campanha com comparador case-insensitive.</summary>
    private static string ChaveEmpresaDimensaoENomeCampanha(int empresaDimensaoId, string nomeCampanha)
        => empresaDimensaoId.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\u001f" + nomeCampanha;

    private static CampanhaHorarioDTO MesclarCampanhasHorarioPorNome(string nomeCampanha, IEnumerable<CampanhaHorarioDTO> itens)
    {
        var list = itens.ToList();
        var total = list.Sum(x => x.TotalEventos);
        var mergedHoras = new int[24];
        foreach (var c in list)
        {
            foreach (var h in c.EventosPorHora)
                mergedHoras[h.Hora] += h.TotalEventos;
        }

        var eventosPorHora = new List<HorarioResumoDTO>();
        for (var h = 0; h < 24; h++)
        {
            var qtd = mergedHoras[h];
            eventosPorHora.Add(new HorarioResumoDTO
            {
                Hora = h,
                TotalEventos = qtd,
                Percentual = total > 0 ? (decimal)qtd / total * 100 : 0
            });
        }

        var horaPico = eventosPorHora.OrderByDescending(e => e.TotalEventos).FirstOrDefault();
        return new CampanhaHorarioDTO
        {
            NomeCampanha = nomeCampanha,
            TotalEventos = total,
            HoraPico = horaPico?.TotalEventos > 0 ? horaPico.Hora : null,
            EventosPorHora = eventosPorHora
        };
    }

    private async Task<Dictionary<int, DimensaoEmpresa?>> ObterDimensoesEmpresaLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoEmpresa?>();
        var todos = await _dimensoesService.ObterDimensoesEmpresaNaoExcluidasAsync();
        return todos.Where(d => ids.Contains(d.Id)).ToDictionary(d => d.Id, d => (DimensaoEmpresa?)d);
    }

    private async Task<Dictionary<int, string>> ObterProdutosLookupAsync(List<int> produtoIds)
    {
        if (produtoIds.Count == 0)
            return [];

        var lookup = new Dictionary<int, string>();
        foreach (var produtoId in produtoIds)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(produtoId);
            if (produto != null)
                lookup[produtoId] = produto.Nome;
        }

        return lookup;
    }

    private async Task<Dictionary<int, DimensaoEquipe?>> ObterDimensoesEquipeLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoEquipe?>();
        var todos = await _dimensoesService.ObterDimensoesEquipeNaoExcluidasAsync();
        return todos.Where(d => ids.Contains(d.Id)).ToDictionary(d => d.Id, d => (DimensaoEquipe?)d);
    }

    private async Task<Dictionary<int, DimensaoVendedor?>> ObterDimensoesVendedorLookupAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, DimensaoVendedor?>();
        var todos = await _dimensoesService.ObterDimensoesVendedorNaoExcluidasAsync();
        return todos.Where(d => ids.Contains(d.Id)).ToDictionary(d => d.Id, d => (DimensaoVendedor?)d);
    }

    private async Task<int?> ObterDimensaoStatusInativoIdAsync()
    {
        var statusList = await _dimensoesService.ObterDimensoesStatusNaoExcluidasAsync();
        return statusList.FirstOrDefault(s => s.Codigo == "INATIVO")?.Id;
    }

    #endregion

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoPrimeiroAtendimentoAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var fatosAguardando = await ObterFatosLeadsPendentesCanonicosAsync(
            filtros,
            AcompanhamentoDashboardTipoPendenciaConstantes.PrimeiroContato);
        var leadsAtivos = await _leadReaderService.ObterLeadsPorIdsAsync(
            fatosAguardando.Select(f => f.LeadId).Distinct(),
            includeDeleted: false);

        var totalItens = fatosAguardando.Count;
        var totalPaginas = tamanhoPagina > 0 ? (int)Math.Ceiling((double)totalItens / tamanhoPagina) : 0;
        var fatosPaginados = fatosAguardando
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        if (fatosPaginados.Count == 0)
        {
            return new PagedResultDTO<DashboardListagemLeadsDTO>
            {
                Itens = new List<DashboardListagemLeadsDTO>(),
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalPaginas
            };
        }

        var vendedorIds = fatosPaginados.Where(f => f.VendedorId.HasValue).Select(f => f.VendedorId!.Value).Distinct().ToList();
        var campanhaIds = fatosPaginados.Where(f => f.CampanhaId.HasValue).Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var statusIds = fatosPaginados.Where(f => f.StatusAtualId.HasValue).Select(f => f.StatusAtualId!.Value).Distinct().ToList();
        var origemIds = fatosPaginados.Select(f => f.OrigemId).Distinct().ToList();
        var equipeIds = fatosPaginados.Where(f => f.EquipeId.HasValue).Select(f => f.EquipeId!.Value).Distinct().ToList();

        var vendedorLookup = await ObterDimensoesVendedorLookupAsync(vendedorIds);
        var campanhaLookup = await ObterDimensoesCampanhaLookupAsync(campanhaIds);
        var statusLookup = await ObterDimensoesStatusLookupAsync(statusIds);
        var origemLookup = await ObterDimensoesOrigemLookupAsync(origemIds);
        var equipeLookup = await ObterDimensoesEquipeLookupAsync(equipeIds);

        foreach (var v in vendedorLookup.Values.Where(e => e?.EquipeId != null))
        {
            if (v!.EquipeId.HasValue && !equipeLookup.ContainsKey(v.EquipeId.Value))
            {
                var eq = await _dimensoesService.ObterDimensaoEquipePorIdAsync(v.EquipeId.Value)
                    ?? await _dimensoesService.ObterDimensaoEquipePorOrigemIdAsync(v.EquipeId.Value);
                if (eq != null) equipeLookup[v.EquipeId.Value] = eq;
            }
        }

        var empresaDimIdsAguardando = fatosPaginados.Select(f => f.EmpresaId).Distinct().ToList();
        var empresaLookupAguardando = await ObterDimensoesEmpresaLookupAsync(empresaDimIdsAguardando);

        var leadLookup = leadsAtivos.ToDictionary(l => l.Id);

        var conversaAtivaLookup = await _conversaReaderService.ObterConversaAtivaIdsPorLeadIdsAsync(
            fatosPaginados.Select(f => f.LeadId).Distinct().ToList());
        var conversaIds = fatosPaginados
            .Select(f => conversaAtivaLookup.TryGetValue(f.LeadId, out var cid) ? cid : (int?)null)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .Distinct()
            .ToList();
        var contextosPorConversa = await _conversaReaderService.GetContextosByIdsAsync(conversaIds);

        var dtos = new List<DashboardListagemLeadsDTO>();
        foreach (var fato in fatosPaginados)
        {
            var lead = leadLookup.GetValueOrDefault(fato.LeadId);

            var nomeStatus = fato.StatusAtualId.HasValue && statusLookup.TryGetValue(fato.StatusAtualId.Value, out var statusDim)
                ? statusDim?.Nome ?? "" : "";
            var nomeOrigem = origemLookup.TryGetValue(fato.OrigemId, out var origemDim)
                ? origemDim?.Nome ?? "" : "";
            var nomeResponsavel = fato.VendedorId.HasValue && vendedorLookup.TryGetValue(fato.VendedorId.Value, out var vendedorDim)
                ? vendedorDim?.Nome : null;
            var nomeCampanha = fato.CampanhaId.HasValue && campanhaLookup.TryGetValue(fato.CampanhaId.Value, out var campanhaDim)
                ? ResolverNomeCampanhaParaAgrupamento(campanhaDim, fato.CampanhaId.Value)
                : null;
            var equipeId = fato.EquipeId ?? (fato.VendedorId.HasValue && vendedorLookup.TryGetValue(fato.VendedorId.Value, out var vd) && vd?.EquipeId.HasValue == true ? vd.EquipeId : null);
            var nomeEquipe = equipeId.HasValue && equipeLookup.TryGetValue(equipeId.Value, out var equipeDim) ? equipeDim?.Nome : null;
            var empresaIdItem = lead?.EmpresaId
                ?? empresaLookupAguardando.GetValueOrDefault(fato.EmpresaId)?.EmpresaOrigemId
                ?? filtros.EmpresaId
                ?? 0;

            dtos.Add(new DashboardListagemLeadsDTO
            {
                LeadId = fato.LeadId,
                EmpresaId = empresaIdItem,
                Nome = lead?.Nome ?? "",
                Email = lead?.Email,
                Telefone = lead?.Telefone,
                NomeStatus = nomeStatus,
                NomeOrigem = nomeOrigem,
                NomeEquipe = nomeEquipe,
                NomeResponsavel = nomeResponsavel,
                NomeResponsavelResumido = nomeResponsavel != null ? NomeVendedorHelper.AbreviarNome(nomeResponsavel) : null,
                NomeCampanha = nomeCampanha,
                ProdutoInteresse = fato.ProdutoInteresse,
                TotalOportunidades = fato.TotalOportunidades,
                MensagensNaoLidas = fato.ConversasNaoLidas,
                TempoMedioRespostaMinutos = fato.TempoMedioRespostaMinutos > 0 ? fato.TempoMedioRespostaMinutos : null,
                DataUltimoEvento = fato.DataUltimoEvento,
                DataCriacao = lead?.DataCriacao ?? fato.DataReferencia,
                EhConvertido = fato.EhConvertido,
                ConversaAtivaId = conversaAtivaLookup.TryGetValue(fato.LeadId, out var conversaId) ? conversaId : null
            });
        }

        return new PagedResultDTO<DashboardListagemLeadsDTO>
        {
            Itens = dtos,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoRespostaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var fatosAguardando = await ObterFatosLeadsPendentesCanonicosAsync(
            filtros,
            AcompanhamentoDashboardTipoPendenciaConstantes.AguardandoResposta);
        var leadsAtivos = await _leadReaderService.ObterLeadsPorIdsAsync(
            fatosAguardando.Select(f => f.LeadId).Distinct(),
            includeDeleted: false);

        var totalItens = fatosAguardando.Count;
        var totalPaginas = tamanhoPagina > 0 ? (int)Math.Ceiling((double)totalItens / tamanhoPagina) : 0;
        var fatosPaginados = fatosAguardando
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        if (fatosPaginados.Count == 0)
        {
            return new PagedResultDTO<DashboardListagemLeadsDTO>
            {
                Itens = new List<DashboardListagemLeadsDTO>(),
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalPaginas
            };
        }

        var vendedorIds = fatosPaginados.Where(f => f.VendedorId.HasValue).Select(f => f.VendedorId!.Value).Distinct().ToList();
        var campanhaIds = fatosPaginados.Where(f => f.CampanhaId.HasValue).Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var statusIds = fatosPaginados.Where(f => f.StatusAtualId.HasValue).Select(f => f.StatusAtualId!.Value).Distinct().ToList();
        var origemIds = fatosPaginados.Select(f => f.OrigemId).Distinct().ToList();
        var equipeIds = fatosPaginados.Where(f => f.EquipeId.HasValue).Select(f => f.EquipeId!.Value).Distinct().ToList();

        var vendedorLookup = await ObterDimensoesVendedorLookupAsync(vendedorIds);
        var campanhaLookup = await ObterDimensoesCampanhaLookupAsync(campanhaIds);
        var statusLookup = await ObterDimensoesStatusLookupAsync(statusIds);
        var origemLookup = await ObterDimensoesOrigemLookupAsync(origemIds);
        var equipeLookup = await ObterDimensoesEquipeLookupAsync(equipeIds);

        foreach (var v in vendedorLookup.Values.Where(e => e?.EquipeId != null))
        {
            if (v!.EquipeId.HasValue && !equipeLookup.ContainsKey(v.EquipeId.Value))
            {
                var eq = await _dimensoesService.ObterDimensaoEquipePorIdAsync(v.EquipeId.Value)
                    ?? await _dimensoesService.ObterDimensaoEquipePorOrigemIdAsync(v.EquipeId.Value);
                if (eq != null) equipeLookup[v.EquipeId.Value] = eq;
            }
        }

        var empresaDimIdsResposta = fatosPaginados.Select(f => f.EmpresaId).Distinct().ToList();
        var empresaLookupResposta = await ObterDimensoesEmpresaLookupAsync(empresaDimIdsResposta);

        var leadLookup = leadsAtivos.ToDictionary(l => l.Id);

        var conversaAtivaLookup = await _conversaReaderService.ObterConversaAtivaIdsPorLeadIdsAsync(
            fatosPaginados.Select(f => f.LeadId).Distinct().ToList());
        var conversaIds = fatosPaginados
            .Select(f => conversaAtivaLookup.TryGetValue(f.LeadId, out var cid) ? cid : (int?)null)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .Distinct()
            .ToList();
        var contextosPorConversa = await _conversaReaderService.GetContextosByIdsAsync(conversaIds);

        var dtos = new List<DashboardListagemLeadsDTO>();
        foreach (var fato in fatosPaginados)
        {
            var lead = leadLookup.GetValueOrDefault(fato.LeadId);

            var nomeStatus = fato.StatusAtualId.HasValue && statusLookup.TryGetValue(fato.StatusAtualId.Value, out var statusDim)
                ? statusDim?.Nome ?? "" : "";
            var nomeOrigem = origemLookup.TryGetValue(fato.OrigemId, out var origemDim)
                ? origemDim?.Nome ?? "" : "";
            var nomeResponsavel = fato.VendedorId.HasValue && vendedorLookup.TryGetValue(fato.VendedorId.Value, out var vendedorDim)
                ? vendedorDim?.Nome : null;
            var nomeCampanha = fato.CampanhaId.HasValue && campanhaLookup.TryGetValue(fato.CampanhaId.Value, out var campanhaDim)
                ? ResolverNomeCampanhaParaAgrupamento(campanhaDim, fato.CampanhaId.Value)
                : null;
            var equipeId = fato.EquipeId ?? (fato.VendedorId.HasValue && vendedorLookup.TryGetValue(fato.VendedorId.Value, out var vd) && vd?.EquipeId.HasValue == true ? vd.EquipeId : null);
            var nomeEquipe = equipeId.HasValue && equipeLookup.TryGetValue(equipeId.Value, out var equipeDim) ? equipeDim?.Nome : null;
            var empresaIdRespostaItem = lead?.EmpresaId
                ?? empresaLookupResposta.GetValueOrDefault(fato.EmpresaId)?.EmpresaOrigemId
                ?? filtros.EmpresaId
                ?? 0;

            var conversaId = conversaAtivaLookup.TryGetValue(fato.LeadId, out var conversaId2) ? conversaId2 : (int?)null;
            var trocaDeContato = conversaId.HasValue && contextosPorConversa.TryGetValue(conversaId.Value, out var ctx)
                ? ctx.TrocaDeContato
                : false;

            dtos.Add(new DashboardListagemLeadsDTO
            {
                LeadId = fato.LeadId,
                EmpresaId = empresaIdRespostaItem,
                Nome = lead?.Nome ?? "",
                Email = lead?.Email,
                Telefone = lead?.Telefone,
                NomeStatus = nomeStatus,
                NomeOrigem = nomeOrigem,
                NomeEquipe = nomeEquipe,
                NomeResponsavel = nomeResponsavel,
                NomeResponsavelResumido = nomeResponsavel != null ? NomeVendedorHelper.AbreviarNome(nomeResponsavel) : null,
                NomeCampanha = nomeCampanha,
                ProdutoInteresse = fato.ProdutoInteresse,
                TotalOportunidades = fato.TotalOportunidades,
                MensagensNaoLidas = fato.ConversasNaoLidas,
                TempoMedioRespostaMinutos = fato.TempoMedioRespostaMinutos > 0 ? fato.TempoMedioRespostaMinutos : null,
                DataUltimoEvento = fato.DataUltimoEvento,
                DataCriacao = lead?.DataCriacao ?? fato.DataReferencia,
                EhConvertido = fato.EhConvertido,
                ConversaAtivaId = conversaId,
                TrocaDeContato = trocaDeContato
            });
        }

        return new PagedResultDTO<DashboardListagemLeadsDTO>
        {
            Itens = dtos,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<List<DashboardLeadPendenteCanonicoDTO>> ObterLeadsPendentesCanonicosAsync(FiltrosDashboardDTO filtros)
    {
        var fatosPrimeiroContato = await ObterFatosLeadsPendentesCanonicosAsync(
            filtros,
            AcompanhamentoDashboardTipoPendenciaConstantes.PrimeiroContato);
        var fatosAguardandoResposta = await ObterFatosLeadsPendentesCanonicosAsync(
            filtros,
            AcompanhamentoDashboardTipoPendenciaConstantes.AguardandoResposta);

        var primeiroContato = await MapearFatosPendentesCanonicosAsync(
            fatosPrimeiroContato,
            AcompanhamentoDashboardTipoPendenciaConstantes.PrimeiroContato);
        var aguardandoResposta = await MapearFatosPendentesCanonicosAsync(
            fatosAguardandoResposta,
            AcompanhamentoDashboardTipoPendenciaConstantes.AguardandoResposta);

        return primeiroContato
            .Concat(aguardandoResposta)
            .OrderByDescending(i => i.DataUltimoEvento ?? DateTime.MinValue)
            .ToList();
    }

    public async Task<List<DashboardConversaAtivaCanonicaDTO>> ObterConversasAtivasCanonicasAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);
        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(
            dataInicio,
            dataFim,
            empresaDimensaoId,
            filtros);
        var inativoId = await ObterDimensaoStatusInativoIdAsync();

        var fatosUnicos = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .Where(f => f.StatusAtualId != inativoId)
            .OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia)
            .ToList();

        var leadsAtivos = await _leadReaderService.ObterLeadsPorIdsAsync(
            fatosUnicos.Select(f => f.LeadId).Distinct(),
            includeDeleted: false);
        var leadIdsAtivos = leadsAtivos
            .Select(l => l.Id)
            .ToHashSet();

        var fatosAtivos = fatosUnicos
            .Where(f => leadIdsAtivos.Contains(f.LeadId))
            .ToList();
        var conversaAtivaLookup = await _conversaReaderService.ObterConversaAtivaIdsPorLeadIdsAsync(
            fatosAtivos.Select(f => f.LeadId).Distinct().ToList());
        var fatosComConversaAtiva = fatosAtivos
            .Where(f => conversaAtivaLookup.ContainsKey(f.LeadId))
            .OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia)
            .ToList();

        var statusIds = fatosComConversaAtiva
            .Where(f => f.StatusAtualId.HasValue)
            .Select(f => f.StatusAtualId!.Value)
            .Distinct()
            .ToList();
        var statusLookup = await ObterDimensoesStatusLookupAsync(statusIds);
        var leadLookup = leadsAtivos.ToDictionary(l => l.Id);

        return fatosComConversaAtiva
            .Select(fato =>
            {
                var tempoMedio = fato.TempoMedioRespostaMinutos.GetValueOrDefault();
                return new DashboardConversaAtivaCanonicaDTO
                {
                    ConversaAtivaId = conversaAtivaLookup[fato.LeadId],
                    LeadId = fato.LeadId,
                    NomeLead = leadLookup.TryGetValue(fato.LeadId, out var lead) ? lead.Nome : string.Empty,
                    ProdutoInteresse = fato.ProdutoInteresse,
                    StatusNome = fato.StatusAtualId.HasValue && statusLookup.TryGetValue(fato.StatusAtualId.Value, out var statusDim)
                        ? statusDim?.Nome ?? string.Empty
                        : string.Empty,
                    MensagensNaoLidas = fato.ConversasNaoLidas,
                    TempoMedioAtendimentoMinutos = tempoMedio > 0
                        ? (int)Math.Round(tempoMedio, MidpointRounding.AwayFromZero)
                        : 0,
                    DataUltimaMensagem = fato.DataUltimoEvento ?? fato.DataReferencia
                };
            })
            .ToList();
    }

    private async Task<List<FatoLeadAgregado>> ObterFatosLeadsPendentesCanonicosAsync(
        FiltrosDashboardDTO filtros,
        string tipoPendencia)
    {
        DateTime dataInicio;
        DateTime dataFim;
        if (filtros.IgnorarFiltroPeriodo)
        {
            // Endpoints leads-aguardando-atendimento e leads-aguardando-resposta: sem filtro de período
            dataInicio = new DateTime(2000, 1, 1);
            dataFim = TimeHelper.GetBrasiliaTime().AddYears(10);
        }
        else
        {
            (dataInicio, dataFim) = filtros.ResolverPeriodo();
        }

        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);
        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(
            dataInicio,
            dataFim,
            empresaDimensaoId,
            filtros);

        var inativoId = await ObterDimensaoStatusInativoIdAsync();
        var fatosPendentes = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .Where(f => EhFatoPendentePorTipo(f, tipoPendencia, inativoId))
            .OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia)
            .ToList();

        // Evita exibir itens de leads excluídos, mesmo que existam no fato OLAP.
        var leadsAtivos = await _leadReaderService.ObterLeadsPorIdsAsync(
            fatosPendentes.Select(f => f.LeadId).Distinct(),
            includeDeleted: false);
        var leadIdsAtivos = leadsAtivos
            .Select(l => l.Id)
            .ToHashSet();

        return fatosPendentes
            .Where(f => leadIdsAtivos.Contains(f.LeadId))
            .ToList();
    }

    private static bool EhFatoPendentePorTipo(FatoLeadAgregado fato, string tipoPendencia, int? inativoId)
    {
        if (fato.StatusAtualId == inativoId)
            return false;

        return tipoPendencia switch
        {
            AcompanhamentoDashboardTipoPendenciaConstantes.PrimeiroContato => fato.AguardandoRespostaVendedor,
            AcompanhamentoDashboardTipoPendenciaConstantes.AguardandoResposta => fato.AguardandoRespostaAtendimento,
            _ => false
        };
    }

    private async Task<List<DashboardLeadPendenteCanonicoDTO>> MapearFatosPendentesCanonicosAsync(
        List<FatoLeadAgregado> fatosPendentes,
        string tipoPendencia)
    {
        if (fatosPendentes.Count == 0)
            return [];

        var origemIds = fatosPendentes
            .Select(f => f.OrigemId)
            .Distinct()
            .ToList();
        var campanhaIds = fatosPendentes
            .Where(f => f.CampanhaId.HasValue)
            .Select(f => f.CampanhaId!.Value)
            .Distinct()
            .ToList();

        var origemLookup = await ObterDimensoesOrigemLookupAsync(origemIds);
        var campanhaLookup = await ObterDimensoesCampanhaLookupAsync(campanhaIds);
        var conversaAtivaLookup = await _conversaReaderService.ObterConversaAtivaIdsPorLeadIdsAsync(
            fatosPendentes.Select(f => f.LeadId).Distinct().ToList());
        var leads = await _leadReaderService.ObterLeadsPorIdsAsync(
            fatosPendentes.Select(f => f.LeadId).Distinct(),
            includeDeleted: false);
        var leadLookup = leads.ToDictionary(l => l.Id);

        return fatosPendentes
            .Select(fato =>
            {
                var nomeOrigem = origemLookup.TryGetValue(fato.OrigemId, out var origemDim)
                    ? origemDim?.Nome ?? string.Empty
                    : string.Empty;
                var nomeCampanha = fato.CampanhaId.HasValue && campanhaLookup.TryGetValue(fato.CampanhaId.Value, out var campanhaDim)
                    ? ResolverNomeCampanhaParaAgrupamento(campanhaDim, fato.CampanhaId.Value)
                    : null;

                return new DashboardLeadPendenteCanonicoDTO
                {
                    LeadId = fato.LeadId,
                    NomeLead = leadLookup.TryGetValue(fato.LeadId, out var lead) ? lead.Nome : string.Empty,
                    DataUltimoEvento = fato.DataUltimoEvento ?? fato.DataReferencia,
                    TipoPendencia = tipoPendencia,
                    NomeOrigem = nomeOrigem,
                    NomeCampanha = nomeCampanha,
                    MensagensNaoLidas = fato.ConversasNaoLidas,
                    ConversaAtivaId = conversaAtivaLookup.TryGetValue(fato.LeadId, out var conversaId)
                        ? conversaId
                        : null
                };
            })
            .ToList();
    }

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsPorVendedorAsync(
        int vendedorId, FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var filtrosComVendedor = new FiltrosDashboardDTO
        {
            TipoPeriodo = filtros.TipoPeriodo,
            DataInicio = filtros.DataInicio,
            DataFim = filtros.DataFim,
            AnoReferencia = filtros.AnoReferencia,
            MesReferencia = filtros.MesReferencia,
            AnoInicioHistorico = filtros.AnoInicioHistorico,
            MesInicioHistorico = filtros.MesInicioHistorico,
            EmpresaId = filtros.EmpresaId,
            EmpresaIds = filtros.EmpresaIds,
            EquipeId = filtros.EquipeId,
            EquipeIds = filtros.EquipeIds,
            VendedorId = vendedorId,
            VendedorIds = [vendedorId],
            OrigemId = filtros.OrigemId,
            OrigemIds = filtros.OrigemIds,
            CampanhaNome = filtros.CampanhaNome,
            CampanhaNomes = filtros.CampanhaNomes,
            StatusLeadId = filtros.StatusLeadId,
            StatusLeadIds = filtros.StatusLeadIds,
            FunilId = filtros.FunilId,
            FunilIds = filtros.FunilIds,
            EtapaId = filtros.EtapaId,
            EtapaIds = filtros.EtapaIds,
            OrdenarPor = filtros.OrdenarPor,
            DirecaoOrdenacao = filtros.DirecaoOrdenacao
        };

        return await ObterListagemLeadsAsync(filtrosComVendedor, pagina, tamanhoPagina);
    }

    public async Task<DashboardUltimaAtualizacaoDTO> ObterUltimaAtualizacaoAsync(CancellationToken cancellationToken = default)
    {
        var controle = await _controleRepository.ObterPorTipoAsync("Fatos", cancellationToken);
        if (controle == null)
            return new DashboardUltimaAtualizacaoDTO();

        return new DashboardUltimaAtualizacaoDTO
        {
            DataUltimaAtualizacao = controle.DataUltimaExecucao,
            StatusUltimaExecucao = controle.StatusUltimaExecucao,
            RegistrosProcessados = controle.RegistrosProcessados,
            TempoExecucaoSegundos = controle.TempoExecucaoSegundos
        };
    }
}

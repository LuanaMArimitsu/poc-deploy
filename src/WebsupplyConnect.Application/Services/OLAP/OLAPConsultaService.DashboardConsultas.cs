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
    public async Task<List<DashboardPerformanceEquipeDTO>> ObterPerformanceEquipesAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var comEquipe = fatosLead.Where(f => f.EquipeId.HasValue).GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.EquipeId!.Value)
            .ToList();

        if (comEquipe.Count == 0) return new List<DashboardPerformanceEquipeDTO>();

        var equipeDimensaoIds = comEquipe.Select(g => g.Key).Distinct().ToList();
        var dimensoesEquipe = await ObterDimensoesEquipeLookupAsync(equipeDimensaoIds);

        return comEquipe.Select(g =>
        {
            var dim = dimensoesEquipe.GetValueOrDefault(g.Key);
            var total = g.Count();
            var convertidos = g.Count(f => f.EhConvertido);
            var valorTotal = g.Sum(f => f.ValorTotalOportunidadesGanhas);
            return new DashboardPerformanceEquipeDTO
            {
                EquipeId = dim?.EquipeOrigemId ?? g.Key,
                EmpresaId = dim?.EmpresaId ?? filtros.EmpresaId ?? 0,
                NomeEquipe = dim?.Nome ?? $"Equipe {g.Key}",
                TotalLeads = total,
                LeadsConvertidos = convertidos,
                TaxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0,
                ValorTotal = valorTotal,
                TotalConversas = g.Sum(f => f.TotalConversas),
                TotalMensagens = g.Sum(f => f.TotalMensagens),
                MensagensNaoLidas = g.Sum(f => f.ConversasNaoLidas)
            };
        }).OrderByDescending(x => x.TotalLeads).ToList();
    }

    public async Task<List<DashboardPerformanceVendedorDTO>> ObterPerformanceVendedoresAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var comVendedor = fatosLead.Where(f => f.VendedorId.HasValue).GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.VendedorId!.Value)
            .ToList();

        if (comVendedor.Count == 0) return new List<DashboardPerformanceVendedorDTO>();

        var vendedorDimensaoIds = comVendedor.Select(g => g.Key).Distinct().ToList();
        var dimensoesVendedor = await ObterDimensoesVendedorLookupAsync(vendedorDimensaoIds);
        var dimensoesEquipe = new Dictionary<int, DimensaoEquipe?>();
        foreach (var v in dimensoesVendedor.Values.Where(e => e?.EquipeId != null))
        {
            if (v!.EquipeId.HasValue && !dimensoesEquipe.ContainsKey(v.EquipeId.Value))
            {
                var eq = await _dimensoesService.ObterDimensaoEquipePorIdAsync(v.EquipeId.Value)
                    ?? await _dimensoesService.ObterDimensaoEquipePorOrigemIdAsync(v.EquipeId.Value);
                dimensoesEquipe[v.EquipeId.Value] = eq;
            }
        }

        return comVendedor.Select(g =>
        {
            var dim = dimensoesVendedor.GetValueOrDefault(g.Key);
            var total = g.Count();
            var convertidos = g.Count(f => f.EhConvertido);
            var tempoMedio = g.Where(f => f.TempoMedioRespostaMinutos.HasValue && f.TempoMedioRespostaMinutos > 0)
                .Select(f => f.TempoMedioRespostaMinutos!.Value).DefaultIfEmpty(0).Average();
            DimensaoEquipe? equipeDim = null;
            if (dim?.EquipeId.HasValue == true)
                dimensoesEquipe.TryGetValue(dim.EquipeId!.Value, out equipeDim);
            var equipeNome = equipeDim?.Nome;
            var empresaIdVendedor = dim?.EmpresaId ?? equipeDim?.EmpresaId ?? filtros.EmpresaId ?? 0;
            var nomeVendedor = dim?.Nome ?? $"Vendedor {g.Key}";

            var taxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0;
            var scoreTempoResposta = tempoMedio > 0 ? (decimal)(100.0 / (1.0 + (double)tempoMedio / 60.0)) : 0m;
            var indicadorPerformance = taxaConversao * 0.7m + scoreTempoResposta * 0.3m;

            return new DashboardPerformanceVendedorDTO
            {
                VendedorId = dim?.UsuarioOrigemId ?? g.Key,
                EmpresaId = empresaIdVendedor,
                NomeResponsavel = nomeVendedor,
                NomeResponsavelResumido = NomeVendedorHelper.AbreviarNome(nomeVendedor),
                NomeEquipe = equipeNome,
                TotalLeads = total,
                LeadsConvertidos = convertidos,
                TaxaConversao = taxaConversao,
                TempoMedioRespostaMinutos = (decimal)tempoMedio,
                IndicadorPerformance = indicadorPerformance,
                TotalConversas = g.Sum(f => f.TotalConversas),
                TotalMensagens = g.Sum(f => f.TotalMensagens),
                MensagensNaoLidas = g.Sum(f => f.ConversasNaoLidas)
            };
        }).OrderByDescending(x => x.TotalLeads).ToList();
    }

    public async Task<DashboardRankingVendedoresResponseDTO> ObterRankingVendedoresAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosLeadNoPeriodo = fatosLead
            .Where(f =>
            {
                var dataBase = f.DataUltimoEvento ?? f.DataReferencia;
                return dataBase >= dataInicio && dataBase <= dataFim;
            })
            .ToList();

        var fatosUnicosPorLead = fatosLeadNoPeriodo
            .Where(f => f.VendedorId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        if (fatosUnicosPorLead.Count == 0)
        {
            return new DashboardRankingVendedoresResponseDTO
            {
                Itens = [],
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = 0,
                TotalPaginas = 0,
                Totalizadores = new DashboardRankingVendedoresTotalizadoresDTO()
            };
        }

        var vendedorDimensaoIds = fatosUnicosPorLead
            .Where(f => f.VendedorId.HasValue)
            .Select(f => f.VendedorId!.Value)
            .Distinct()
            .ToList();

        var dimensoesVendedor = await ObterDimensoesVendedorLookupAsync(vendedorDimensaoIds);

        var equipeDimensaoIds = dimensoesVendedor.Values
            .Where(v => v?.EquipeId.HasValue == true)
            .Select(v => v!.EquipeId!.Value)
            .Distinct()
            .ToList();
        var dimensoesEquipe = await ObterDimensoesEquipeLookupAsync(equipeDimensaoIds);

        var empresaDimensaoIds = fatosUnicosPorLead
            .Select(f => f.EmpresaId)
            .Distinct()
            .ToList();
        var dimensoesEmpresa = await ObterDimensoesEmpresaLookupAsync(empresaDimensaoIds);

        var ranking = fatosUnicosPorLead
            .Where(f => f.VendedorId.HasValue)
            .GroupBy(f => f.VendedorId!.Value)
            .Select(g =>
            {
                var fatosVendedor = g.ToList();
                var vendedorDim = dimensoesVendedor.GetValueOrDefault(g.Key);
                var nomeVendedor = vendedorDim?.Nome ?? $"Vendedor {g.Key}";
                var empresaDim = dimensoesEmpresa.GetValueOrDefault(fatosVendedor.First().EmpresaId);
                var equipeNome = vendedorDim?.EquipeId.HasValue == true &&
                                 dimensoesEquipe.TryGetValue(vendedorDim.EquipeId!.Value, out var equipeDim)
                    ? equipeDim?.Nome
                    : null;

                var leadsRecebidos = fatosVendedor.Count;
                var leadsComOportunidade = fatosVendedor.Count(f => f.TotalOportunidades > 0);
                var leadsConvertidos = fatosVendedor.Count(f => f.EhConvertido);
                var leadsPerdidos = fatosVendedor.Count(f => f.OportunidadesPerdidas > 0);
                var oportunidadesGanhas = fatosVendedor.Sum(f => f.OportunidadesGanhas);
                var oportunidadesPerdidas = fatosVendedor.Sum(f => f.OportunidadesPerdidas);
                var oportunidadesAbertas = fatosVendedor.Sum(f =>
                    Math.Max(f.TotalOportunidades - f.OportunidadesGanhas - f.OportunidadesPerdidas, 0));

                var taxaConversao = leadsRecebidos > 0
                    ? (decimal)leadsConvertidos / leadsRecebidos * 100
                    : 0m;

                var tempoMedioResposta = fatosVendedor
                    .Where(f => f.TempoMedioRespostaMinutos.HasValue && f.TempoMedioRespostaMinutos > 0)
                    .Select(f => f.TempoMedioRespostaMinutos!.Value)
                    .DefaultIfEmpty(0m)
                    .Average();

                var scoreTempoResposta = tempoMedioResposta > 0
                    ? (decimal)(100.0 / (1.0 + (double)tempoMedioResposta / 60.0))
                    : 0m;
                var indicadorPerformance = taxaConversao * 0.7m + scoreTempoResposta * 0.3m;

                return new DashboardRankingVendedorDTO
                {
                    VendedorId = vendedorDim?.UsuarioOrigemId ?? g.Key,
                    NomeResponsavel = nomeVendedor,
                    NomeResponsavelResumido = NomeVendedorHelper.AbreviarNome(nomeVendedor),
                    NomeEquipe = equipeNome,
                    EmpresaId = empresaDim?.EmpresaOrigemId,
                    NomeEmpresa = empresaDim?.Nome,
                    GrupoEmpresaId = empresaDim?.GrupoEmpresaId,
                    LeadsRecebidos = leadsRecebidos,
                    LeadsComOportunidade = leadsComOportunidade,
                    LeadsConvertidos = leadsConvertidos,
                    LeadsPerdidos = leadsPerdidos,
                    OportunidadesAbertas = oportunidadesAbertas,
                    OportunidadesGanhas = oportunidadesGanhas,
                    OportunidadesPerdidas = oportunidadesPerdidas,
                    TaxaConversaoLeads = taxaConversao,
                    TempoMedioRespostaMinutos = tempoMedioResposta,
                    IndicadorPerformance = indicadorPerformance
                };
            })
            .ToList();

        var criteriosOrdenacao = ObterCriteriosOrdenacaoRankingVendedores(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao);
        var rankingOrdenado = AplicarOrdenacaoRankingVendedores(ranking, criteriosOrdenacao);

        var totalItens = rankingOrdenado.Count;
        var totalPaginas = tamanhoPagina > 0 ? (int)Math.Ceiling((double)totalItens / tamanhoPagina) : 0;
        var itensPagina = rankingOrdenado
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        var totalLeadsRecebidos = rankingOrdenado.Sum(r => r.LeadsRecebidos);
        var totalOportunidadesAbertas = rankingOrdenado.Sum(r => r.OportunidadesAbertas);
        var totalOportunidadesGanhas = rankingOrdenado.Sum(r => r.OportunidadesGanhas);
        var totalOportunidadesPerdidas = rankingOrdenado.Sum(r => r.OportunidadesPerdidas);
        var totalLeadsConvertidos = rankingOrdenado.Sum(r => r.LeadsConvertidos);
        var taxaConversaoPercentual = totalLeadsRecebidos > 0
            ? (decimal)totalLeadsConvertidos / totalLeadsRecebidos * 100
            : 0m;

        return new DashboardRankingVendedoresResponseDTO
        {
            Itens = itensPagina,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas,
            Totalizadores = new DashboardRankingVendedoresTotalizadoresDTO
            {
                TotalVendedores = totalItens,
                TotalLeadsRecebidos = totalLeadsRecebidos,
                TotalOportunidadesAbertas = totalOportunidadesAbertas,
                TotalOportunidadesGanhas = totalOportunidadesGanhas,
                TotalOportunidadesPerdidas = totalOportunidadesPerdidas,
                TaxaConversaoPercentual = taxaConversaoPercentual
            }
        };
    }

    public async Task<List<DashboardAtividadePorHorarioDTO>> ObterAtividadePorHorarioAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsUnicosPorHora = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => (f.DataUltimoEvento ?? f.DataReferencia).Hour)
            .ToDictionary(g => g.Key, g => g.ToList());

        var resultado = new List<DashboardAtividadePorHorarioDTO>();
        for (var h = 0; h < 24; h++)
        {
            var fatosHora = leadsUnicosPorHora.GetValueOrDefault(h, new List<FatoLeadAgregado>());
            resultado.Add(new DashboardAtividadePorHorarioDTO
            {
                Hora = h,
                QuantidadeLeads = fatosHora.Count,
                QuantidadeMensagens = fatosHora.Sum(f => f.TotalMensagens),
                QuantidadeConversas = fatosHora.Sum(f => f.TotalConversas)
            });
        }
        return resultado;
    }

    public async Task<PagedResultDTO<DashboardRankingLeadsEmpresaDTO>> ObterRankingLeadsPorEmpresaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosUnicosPorLead = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        var empresaDimensaoIds = fatosUnicosPorLead.Select(f => f.EmpresaId).Distinct().ToList();
        var dimensoesEmpresa = await ObterDimensoesEmpresaLookupAsync(empresaDimensaoIds);

        var ranking = fatosUnicosPorLead
            .GroupBy(f => f.EmpresaId)
            .Select(g =>
            {
                var dim = dimensoesEmpresa.GetValueOrDefault(g.Key);
                return new DashboardRankingLeadsEmpresaDTO
                {
                    EmpresaId = dim?.EmpresaOrigemId ?? g.Key,
                    NomeEmpresa = dim?.Nome ?? $"Empresa {g.Key}",
                    GrupoEmpresaId = dim?.GrupoEmpresaId ?? 0,
                    LeadsRecebidos = g.Count()
                };
            })
            .ToList();

        var criterios = ObterCriteriosOrdenacaoSimples(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao,
            CamposOrdenaveisRankingLeadsEmpresa,
            "leadsRecebidos",
            direcaoPadraoAsc: false);
        var rankingOrdenado = AplicarOrdenacaoRankingLeadsEmpresa(ranking, criterios);

        return PaginarResultado(rankingOrdenado, pagina, tamanhoPagina);
    }

    public async Task<PagedResultDTO<DashboardRankingLeadsNomeCampanhaDTO>> ObterRankingLeadsPorNomeCampanhaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosUnicosPorLead = fatosLead
            .Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        if (fatosUnicosPorLead.Count == 0)
            return PaginarResultado(new List<DashboardRankingLeadsNomeCampanhaDTO>(), pagina, tamanhoPagina);

        var campanhaDimensaoIds = fatosUnicosPorLead.Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaDimensaoIds);

        var ranking = fatosUnicosPorLead
            .GroupBy(
                f => ResolverNomeCampanhaParaAgrupamento(
                    dimensoesCampanha.GetValueOrDefault(f.CampanhaId!.Value),
                    f.CampanhaId!.Value),
                StringComparer.OrdinalIgnoreCase)
            .Select(g => new DashboardRankingLeadsNomeCampanhaDTO
            {
                NomeCampanha = g.Key,
                LeadsRecebidos = g.Count()
            })
            .ToList();

        var criterios = ObterCriteriosOrdenacaoSimples(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao,
            CamposOrdenaveisRankingLeadsNomeCampanha,
            "leadsrecebidos",
            direcaoPadraoAsc: false);
        var rankingOrdenado = AplicarOrdenacaoRankingLeadsNomeCampanha(ranking, criterios);

        return PaginarResultado(rankingOrdenado, pagina, tamanhoPagina);
    }

    public async Task<PagedResultDTO<DashboardRankingLeadsNomeCampanhaEmpresaDTO>> ObterRankingLeadsPorNomeCampanhaEEmpresaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosUnicosPorLead = fatosLead
            .Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        if (fatosUnicosPorLead.Count == 0)
            return PaginarResultado(new List<DashboardRankingLeadsNomeCampanhaEmpresaDTO>(), pagina, tamanhoPagina);

        var campanhaDimensaoIds = fatosUnicosPorLead.Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaDimensaoIds);

        var empresaDimensaoIds = fatosUnicosPorLead.Select(f => f.EmpresaId).Distinct().ToList();
        var dimensoesEmpresa = await ObterDimensoesEmpresaLookupAsync(empresaDimensaoIds);

        var ranking = fatosUnicosPorLead
            .GroupBy(
                f => ChaveEmpresaDimensaoENomeCampanha(
                    f.EmpresaId,
                    ResolverNomeCampanhaParaAgrupamento(
                        dimensoesCampanha.GetValueOrDefault(f.CampanhaId!.Value),
                        f.CampanhaId!.Value)),
                StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var primeiro = g.First();
                var dimEmp = dimensoesEmpresa.GetValueOrDefault(primeiro.EmpresaId);
                var nomeCampanha = ResolverNomeCampanhaParaAgrupamento(
                    dimensoesCampanha.GetValueOrDefault(primeiro.CampanhaId!.Value),
                    primeiro.CampanhaId!.Value);
                return new DashboardRankingLeadsNomeCampanhaEmpresaDTO
                {
                    NomeCampanha = nomeCampanha,
                    EmpresaId = dimEmp?.EmpresaOrigemId ?? primeiro.EmpresaId,
                    NomeEmpresa = dimEmp?.Nome ?? $"Empresa {primeiro.EmpresaId}",
                    GrupoEmpresaId = dimEmp?.GrupoEmpresaId ?? 0,
                    LeadsRecebidos = g.Count()
                };
            })
            .ToList();

        var criterios = ObterCriteriosOrdenacaoSimples(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao,
            CamposOrdenaveisRankingLeadsNomeCampanhaEmpresa,
            "leadsrecebidos",
            direcaoPadraoAsc: false);
        var rankingOrdenado = AplicarOrdenacaoRankingLeadsNomeCampanhaEmpresa(ranking, criterios);

        return PaginarResultado(rankingOrdenado, pagina, tamanhoPagina);
    }

    public async Task<PagedResultDTO<DashboardRankingOportunidadesEmpresaDTO>> ObterRankingOportunidadesPorEmpresaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosUnicosPorOportunidade = fatosOportunidade
            .GroupBy(f => f.OportunidadeId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        var empresaDimensaoIds = fatosUnicosPorOportunidade.Select(f => f.EmpresaId).Distinct().ToList();
        var dimensoesEmpresa = await ObterDimensoesEmpresaLookupAsync(empresaDimensaoIds);

        var ranking = fatosUnicosPorOportunidade
            .GroupBy(f => f.EmpresaId)
            .Select(g =>
            {
                var dim = dimensoesEmpresa.GetValueOrDefault(g.Key);
                var itens = g.ToList();
                var total = itens.Count;
                var ganhas = itens.Count(f => f.EhGanha);
                var perdidas = itens.Count(f => f.EhPerdida);
                var abertas = itens.Count(f => !f.EhGanha && !f.EhPerdida);

                return new DashboardRankingOportunidadesEmpresaDTO
                {
                    EmpresaId = dim?.EmpresaOrigemId ?? g.Key,
                    NomeEmpresa = dim?.Nome ?? $"Empresa {g.Key}",
                    GrupoEmpresaId = dim?.GrupoEmpresaId ?? 0,
                    OportunidadesTotal = total,
                    OportunidadesAbertas = abertas,
                    OportunidadesGanhas = ganhas,
                    OportunidadesPerdidas = perdidas
                };
            })
            .ToList();

        var criterios = ObterCriteriosOrdenacaoSimples(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao,
            CamposOrdenaveisRankingOportunidadesEmpresa,
            "oportunidadesTotal",
            direcaoPadraoAsc: false);
        var rankingOrdenado = AplicarOrdenacaoRankingOportunidadesEmpresa(ranking, criterios);

        return PaginarResultado(rankingOrdenado, pagina, tamanhoPagina);
    }

    public async Task<PagedResultDTO<DashboardRankingOportunidadesTipoInteresseProdutoDTO>> ObterRankingOportunidadesPorTipoInteresseEProdutoAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosUnicosPorOportunidade = fatosOportunidade
            .GroupBy(f => f.OportunidadeId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();

        if (fatosUnicosPorOportunidade.Count == 0)
            return PaginarResultado(new List<DashboardRankingOportunidadesTipoInteresseProdutoDTO>(), pagina, tamanhoPagina);

        var oportunidades = await _oportunidadeReaderService.ObterOportunidadesPorIdsParaETLAsync(
            fatosUnicosPorOportunidade.Select(f => f.OportunidadeId).Distinct());
        var oportunidadesLookup = oportunidades.ToDictionary(o => o.Id);

        var tiposInteresseLookup = (await _oportunidadeReaderService.ListarTiposInteresseAsync())
            .ToDictionary(t => t.Id, t => t.Titulo);

        var produtoIds = oportunidades
            .Where(o => o.ProdutoId > 0)
            .Select(o => o.ProdutoId)
            .Distinct()
            .ToList();
        var produtoLookup = await ObterProdutosLookupAsync(produtoIds);

        var fatosComDetalhe = fatosUnicosPorOportunidade
            .Where(f => oportunidadesLookup.ContainsKey(f.OportunidadeId))
            .Select(f => new
            {
                Fato = f,
                Oportunidade = oportunidadesLookup[f.OportunidadeId]
            })
            .Where(x => x.Oportunidade.ProdutoId > 0)
            .ToList();

        var ranking = fatosComDetalhe
            .GroupBy(x => new { x.Oportunidade.TipoInteresseId, x.Oportunidade.ProdutoId })
            .Select(g =>
            {
                var tipoInteresseId = g.Key.TipoInteresseId;
                var produtoId = g.Key.ProdutoId;
                var nomeTipoInteresse = tipoInteresseId.HasValue && tiposInteresseLookup.TryGetValue(tipoInteresseId.Value, out var tipoNome)
                    ? tipoNome
                    : "Sem tipo de interesse";
                var nomeProduto = produtoLookup.GetValueOrDefault(produtoId) ?? $"Produto {produtoId}";

                var itens = g.Select(x => x.Fato).ToList();
                var total = itens.Count;
                var ganhas = itens.Count(f => f.EhGanha);
                var perdidas = itens.Count(f => f.EhPerdida);
                var abertas = itens.Count(f => !f.EhGanha && !f.EhPerdida);

                return new DashboardRankingOportunidadesTipoInteresseProdutoDTO
                {
                    TipoInteresseId = tipoInteresseId,
                    NomeTipoInteresse = nomeTipoInteresse,
                    ProdutoId = produtoId,
                    NomeProduto = nomeProduto,
                    OportunidadesTotal = total,
                    OportunidadesAbertas = abertas,
                    OportunidadesGanhas = ganhas,
                    OportunidadesPerdidas = perdidas
                };
            })
            .ToList();

        var criterios = ObterCriteriosOrdenacaoSimples(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao,
            CamposOrdenaveisRankingOportunidadesTipoProduto,
            "oportunidadesTotal",
            direcaoPadraoAsc: false);
        var rankingOrdenado = AplicarOrdenacaoRankingTipoInteresseProduto(ranking, criterios);

        return PaginarResultado(rankingOrdenado, pagina, tamanhoPagina);
    }

    public async Task<DashboardTempoRespostaDTO> ObterDistribuicaoTempoRespostaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var tempos = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .Where(f => f.TempoMedioRespostaMinutos.HasValue && f.TempoMedioRespostaMinutos > 0)
            .Select(f => f.TempoMedioRespostaMinutos!.Value)
            .ToList();

        var media = tempos.Any() ? (decimal)tempos.Average() : 0;
        var ordenados = tempos.OrderBy(t => t).ToList();
        var mediana = ordenados.Count > 0
            ? ordenados.Count % 2 == 1
                ? ordenados[ordenados.Count / 2]
                : (ordenados[ordenados.Count / 2 - 1] + ordenados[ordenados.Count / 2]) / 2
            : 0;

        var faixas = new[] { (0m, 5m, "0-5 min"), (5m, 15m, "5-15 min"), (15m, 30m, "15-30 min"), (30m, 60m, "30-60 min"), (60m, 999999m, "> 60 min") };
        var distribuicao = faixas.Select(f =>
        {
            var qtd = tempos.Count(t => t >= f.Item1 && t < f.Item2);
            return new DashboardTempoRespostaItemDTO
            {
                Faixa = f.Item3,
                Quantidade = qtd,
                Percentual = tempos.Count > 0 ? (decimal)qtd / tempos.Count * 100 : 0
            };
        }).ToList();

        return new DashboardTempoRespostaDTO
        {
            DistribuicaoPorFaixa = distribuicao,
            TempoMedioMinutos = media,
            MedianaMinutos = mediana
        };
    }

    public async Task<List<DashboardConversoesSemanaDTO>> ObterConversoesSemanaAsync(FiltrosDashboardDTO filtros)
    {
        var metaConversaoPercentual = _config.MetaConversaoPercentual;

        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsUnicos = fatosLead
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .ToList();
        var cultura = System.Globalization.CultureInfo.CurrentCulture;

        var totalLeads = leadsUnicos.Count;

        var porData = leadsUnicos
            .Where(f => f.EhConvertido && f.DataConversao.HasValue)
            .GroupBy(f => f.DataConversao!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var datas = Enumerable.Range(0, (int)(dataFim - dataInicio).TotalDays + 1)
            .Select(d => dataInicio.Date.AddDays(d))
            .ToList();

        // Meta diária = (totalLeads * metaConversaoPercentual) / dias do período
        var metaDiaria = datas.Count > 0
            ? (int)Math.Ceiling(totalLeads * metaConversaoPercentual / datas.Count)
            : 0;

        return datas.Select(d => new DashboardConversoesSemanaDTO
        {
            Data = d,
            DiaSemana = cultura.DateTimeFormat.GetDayName(d.DayOfWeek),
            Conversoes = porData.GetValueOrDefault(d, 0),
            Meta = metaDiaria
        }).ToList();
    }

    public async Task<DashboardInsightsDTO> ObterInsightsAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        // --- Taxa de conversão comparativa (semana atual vs anterior) - via OLAP ---
        var agora = TimeHelper.GetBrasiliaTime();
        var inicioSemanaAtual = agora.Date.AddDays(-(int)agora.DayOfWeek + (int)DayOfWeek.Monday);
        if (agora.DayOfWeek == DayOfWeek.Sunday) inicioSemanaAtual = inicioSemanaAtual.AddDays(-7);
        var fimSemanaAtual = inicioSemanaAtual.AddDays(7).AddTicks(-1);
        var inicioSemanaAnterior = inicioSemanaAtual.AddDays(-7);
        var fimSemanaAnterior = inicioSemanaAtual.AddTicks(-1);

        // Aplica filtros avançados (equipe, vendedor, origem, campanha, status) na taxa de conversão
        var fatosLeadSemanaAtual = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(
            inicioSemanaAtual, fimSemanaAtual, empresaDimensaoId, filtros);
        var leadsUnicosSemanaAtual = fatosLeadSemanaAtual.GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();
        var totalSemanaAtual = leadsUnicosSemanaAtual.Count;
        var convertidosSemanaAtual = leadsUnicosSemanaAtual.Count(f => f.EhConvertido);
        var taxaSemanaAtual = totalSemanaAtual > 0 ? (decimal)convertidosSemanaAtual / totalSemanaAtual * 100 : 0m;

        var fatosLeadSemanaAnterior = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(
            inicioSemanaAnterior, fimSemanaAnterior, empresaDimensaoId, filtros);
        var leadsUnicosSemanaAnterior = fatosLeadSemanaAnterior.GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();
        var totalSemanaAnterior = leadsUnicosSemanaAnterior.Count;
        var convertidosSemanaAnterior = leadsUnicosSemanaAnterior.Count(f => f.EhConvertido);
        var taxaSemanaAnterior = totalSemanaAnterior > 0 ? (decimal)convertidosSemanaAnterior / totalSemanaAnterior * 100 : 0m;

        var variacao = taxaSemanaAnterior > 0 ? ((taxaSemanaAtual - taxaSemanaAnterior) / taxaSemanaAnterior) * 100 : 0m;

        // --- Pico de atendimento - via OLAP (hora com mais atividade de leads) ---
        var fatosLeadPeriodo = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var porHora = fatosLeadPeriodo
            .GroupBy(f => (f.DataUltimoEvento ?? f.DataReferencia).Hour)
            .Select(g => new { Hora = g.Key, Total = g.Select(f => f.LeadId).Distinct().Count() })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();
        var horaPico = porHora?.Total > 0 ? porHora.Hora : (int?)null;

        // --- Oportunidades estagnadas - via OLAP (apenas abertas, filtradas por empresa) ---
        var estagnadas = await _fatoOportunidadeRepository.ObterOportunidadesEstagnadasAsync(
            _config.DiasEstagnada, empresaDimensaoId);

        // Resolver nomes dos vendedores das oportunidades estagnadas via dimensão OLAP
        var vendedorIdsEstagnadas = estagnadas.Take(10)
            .Where(f => f.VendedorId.HasValue)
            .Select(f => f.VendedorId!.Value).Distinct().ToList();
        var vendedorLookupEstagnadas = await ObterDimensoesVendedorLookupAsync(vendedorIdsEstagnadas);

        var oportunidadesEstagnadas = estagnadas.Take(10).Select(f =>
        {
            var vendedorNome = f.VendedorId.HasValue && vendedorLookupEstagnadas.TryGetValue(f.VendedorId.Value, out var vd)
                ? vd?.Nome : null;
            return new DashboardOportunidadeEstagnadaDTO
            {
                OportunidadeId = f.OportunidadeId,
                Titulo = $"Oportunidade #{f.OportunidadeId}",
                DiasEstagnada = f.DiasDesdeUltimaInteracao ?? _config.DiasEstagnada,
                ResponsavelNome = vendedorNome,
                NomeResponsavelResumido = vendedorNome != null ? NomeVendedorHelper.AbreviarNome(vendedorNome) : null
            };
        }).ToList();

        return new DashboardInsightsDTO
        {
            TaxaConversaoSemanaAtual = taxaSemanaAtual,
            TaxaConversaoSemanaAnterior = taxaSemanaAnterior,
            VariacaoTaxaConversao = variacao,
            HoraPicoAtendimento = horaPico,
            OportunidadesEstagnadas = oportunidadesEstagnadas
        };
    }

    public async Task<DashboardCampanhaPerformanceDTO> ObterPerformanceCampanhasAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosEvento = await ObterFatosEventoFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);

        var leadsPorCampanha = fatosLead.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var eventosPorCampanha = fatosEvento.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var campanhaIds = leadsPorCampanha.Keys.Union(eventosPorCampanha.Keys).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        var campanhas = new List<DashboardCampanhaDetalheDTO>();
        foreach (var cid in campanhaIds)
        {
            var dim = dimensoesCampanha.GetValueOrDefault(cid);
            var leads = leadsPorCampanha.GetValueOrDefault(cid, new List<FatoLeadAgregado>());
            var eventos = eventosPorCampanha.GetValueOrDefault(cid, new List<FatoEventoAgregado>());
            var leadsUnicos = leads.GroupBy(f => f.LeadId).Select(g => g.First()).ToList();

            var convertidos = leadsUnicos.Count(f => f.EhConvertido);
            var perdidos = leadsUnicos.Count(f => f.OportunidadesPerdidas > 0);
            var emAberto = leadsUnicos.Count(f => !f.EhConvertido && f.TotalOportunidades > 0);
            var contatados = leadsUnicos.Count(f => f.TempoMedioPrimeiroAtendimentoMinutos is > 0);

            campanhas.Add(new DashboardCampanhaDetalheDTO
            {
                NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, cid),
                Disparadas = null,
                Acessados = null,
                Clicados = null,
                Contatados = contatados,
                Leads = leadsUnicos.Count,
                Negociacao = emAberto + convertidos + perdidos,
                Convertidos = convertidos,
                Perdidos = perdidos,
                EmAberto = emAberto,
                ROI = null
            });
        }

        campanhas = campanhas
            .GroupBy(c => c.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var it = g.ToList();
                return new DashboardCampanhaDetalheDTO
                {
                    NomeCampanha = g.Key,
                    Disparadas = null,
                    Acessados = null,
                    Clicados = null,
                    Contatados = it.Sum(x => x.Contatados ?? 0),
                    Leads = it.Sum(x => x.Leads),
                    Negociacao = it.Sum(x => x.Negociacao),
                    Convertidos = it.Sum(x => x.Convertidos),
                    Perdidos = it.Sum(x => x.Perdidos),
                    EmAberto = it.Sum(x => x.EmAberto),
                    ROI = null
                };
            }).ToList();

        return new DashboardCampanhaPerformanceDTO
        {
            Disparadas = null,
            LeadsGerados = campanhas.Sum(c => c.Leads),
            EmNegociacao = campanhas.Sum(c => c.Negociacao),
            Convertidos = campanhas.Sum(c => c.Convertidos),
            Perdidos = campanhas.Sum(c => c.Perdidos),
            ROI = null,
            ValorTotal = fatosOportunidade.Where(f => f.EhGanha).Sum(f => f.ValorFinal ?? f.ValorEstimado),
            Campanhas = campanhas
        };
    }

    public async Task<List<DashboardEventosPorCampanhaDTO>> ObterEventosPorCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosEvento = await ObterFatosEventoFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var porCampanha = fatosEvento.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var campanhaIds = porCampanha.Keys.ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        var linhas = porCampanha.Select(kv =>
        {
            var dim = dimensoesCampanha.GetValueOrDefault(kv.Key);
            var eventos = kv.Value;
            return new DashboardEventosPorCampanhaDTO
            {
                NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, kv.Key),
                TotalEventos = eventos.Count,
                OportunidadesGeradas = eventos.Sum(e => e.TotalOportunidadesGeradas),
                OportunidadesGanhas = eventos.Sum(e => e.OportunidadesGanhas),
                ValorTotal = eventos.Sum(e => e.ValorTotalOportunidadesGanhas)
            };
        }).ToList();

        linhas = linhas
            .GroupBy(x => x.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DashboardEventosPorCampanhaDTO
            {
                NomeCampanha = g.Key,
                TotalEventos = g.Sum(x => x.TotalEventos),
                OportunidadesGeradas = g.Sum(x => x.OportunidadesGeradas),
                OportunidadesGanhas = g.Sum(x => x.OportunidadesGanhas),
                ValorTotal = g.Sum(x => x.ValorTotal)
            }).ToList();

        return linhas.OrderByDescending(x => x.TotalEventos).ToList();
    }

    public async Task<List<DashboardLeadsConvertidosPorCampanhaDTO>> ObterLeadsConvertidosPorCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var porCampanha = fatosLead.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var campanhaIds = porCampanha.Keys.ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        var linhas = porCampanha.Select(kv =>
        {
            var dim = dimensoesCampanha.GetValueOrDefault(kv.Key);
            var leads = kv.Value;
            var total = leads.Count;
            var convertidos = leads.Count(f => f.EhConvertido);
            return new DashboardLeadsConvertidosPorCampanhaDTO
            {
                NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, kv.Key),
                TotalLeads = total,
                TotalConvertidos = convertidos,
                TaxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0
            };
        }).ToList();

        linhas = linhas
            .GroupBy(x => x.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var tl = g.Sum(x => x.TotalLeads);
                var tc = g.Sum(x => x.TotalConvertidos);
                return new DashboardLeadsConvertidosPorCampanhaDTO
                {
                    NomeCampanha = g.Key,
                    TotalLeads = tl,
                    TotalConvertidos = tc,
                    TaxaConversao = tl > 0 ? (decimal)tc / tl * 100 : 0
                };
            }).ToList();

        return linhas.OrderByDescending(x => x.TotalLeads).ToList();
    }

    public async Task<DashboardFunilCampanhaDTO> ObterFunilCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);

        var leadsUnicos = fatosLead.GroupBy(f => f.LeadId).Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();
        var leadsComOp = leadsUnicos.Where(f => f.TotalOportunidades > 0).ToList();

        var contatados = leadsUnicos.Count(f => f.TempoMedioPrimeiroAtendimentoMinutos is > 0);

        return new DashboardFunilCampanhaDTO
        {
            LeadsGerados = leadsUnicos.Count,
            EmNegociacao = leadsComOp.Count(f => !f.EhConvertido),
            Convertidos = leadsComOp.Count(f => f.EhConvertido),
            Perdidos = fatosOportunidade.Count(f => f.EhPerdida),
            Disparadas = null,
            Acessados = null,
            Clicados = null,
            Contatados = contatados,
            TaxaAbertura = null,
            TaxaClique = null,
            DadosExternosDisponiveis = false
        };
    }

    public async Task<DashboardConversaoGeralDTO> ObterConversaoGeralAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);

        var leadsUnicos = fatosLead.GroupBy(f => f.LeadId).Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();
        var emNegociacao = leadsUnicos.Count(f => f.TotalOportunidades > 0 && !f.EhConvertido);
        var convertidos = leadsUnicos.Count(f => f.EhConvertido);
        var total = emNegociacao + convertidos;

        var contatados = leadsUnicos.Count(f => f.TempoMedioPrimeiroAtendimentoMinutos is > 0);

        return new DashboardConversaoGeralDTO
        {
            Contatados = contatados,
            EmNegociacao = emNegociacao,
            Convertidos = convertidos,
            TaxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0
        };
    }

    public async Task<List<DashboardEngajamentoCampanhaDTO>> ObterEngajamentoPorCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var porCampanha = fatosLead.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var campanhaIds = porCampanha.Keys.ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        var linhas = porCampanha.Select(kv =>
        {
            var dim = dimensoesCampanha.GetValueOrDefault(kv.Key);
            var leads = kv.Value;
            var total = leads.Count;
            var convertidos = leads.Count(f => f.EhConvertido);
            return new DashboardEngajamentoCampanhaDTO
            {
                NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, kv.Key),
                TaxaClique = null,
                TaxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0
            };
        }).ToList();

        var chavesNome = linhas
            .GroupBy(x => x.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Key)
            .ToList();
        var merged = new List<DashboardEngajamentoCampanhaDTO>();
        foreach (var nome in chavesNome)
        {
            var total = 0;
            var convertidos = 0;
            foreach (var kv in porCampanha)
            {
                var dim = dimensoesCampanha.GetValueOrDefault(kv.Key);
                if (!string.Equals(ResolverNomeCampanhaParaAgrupamento(dim, kv.Key), nome, StringComparison.OrdinalIgnoreCase))
                    continue;
                var leads = kv.Value;
                total += leads.Count;
                convertidos += leads.Count(f => f.EhConvertido);
            }

            merged.Add(new DashboardEngajamentoCampanhaDTO
            {
                NomeCampanha = nome,
                TaxaClique = null,
                TaxaConversao = total > 0 ? (decimal)convertidos / total * 100 : 0
            });
        }

        linhas = merged;

        return linhas.OrderByDescending(x => x.TaxaConversao).ToList();
    }

    public async Task<DashboardEventosLeadPorHorarioCampanhaDTO> ObterEventosLeadPorHorarioCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosEvento = await ObterFatosEventoFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);

        // Filtrar apenas eventos com campanha
        var eventosComCampanha = fatosEvento.Where(f => f.CampanhaId.HasValue).ToList();

        // Resolver dimensões de campanha
        var campanhaIds = eventosComCampanha.Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        // Agrupar por campanha
        var porCampanha = eventosComCampanha
            .GroupBy(f => f.CampanhaId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var campanhas = new List<CampanhaHorarioDTO>();
        foreach (var kv in porCampanha)
        {
            var dim = dimensoesCampanha.GetValueOrDefault(kv.Key);
            var totalEventosCampanha = kv.Value.Count;

            // Agrupar por hora considerando DataUltimoEvento do lead
            var porHora = kv.Value
                .GroupBy(f => (f.DataUltimoEvento ?? f.DataReferencia).Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            var eventosPorHora = new List<HorarioResumoDTO>();
            for (var h = 0; h < 24; h++)
            {
                var qtd = porHora.GetValueOrDefault(h, 0);
                eventosPorHora.Add(new HorarioResumoDTO
                {
                    Hora = h,
                    TotalEventos = qtd,
                    Percentual = totalEventosCampanha > 0 ? (decimal)qtd / totalEventosCampanha * 100 : 0
                });
            }

            var horaPico = eventosPorHora.OrderByDescending(e => e.TotalEventos).FirstOrDefault();

            campanhas.Add(new CampanhaHorarioDTO
            {
                NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, kv.Key),
                TotalEventos = totalEventosCampanha,
                HoraPico = horaPico?.TotalEventos > 0 ? horaPico.Hora : null,
                EventosPorHora = eventosPorHora
            });
        }

        campanhas = campanhas
            .GroupBy(c => c.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(g => MesclarCampanhasHorarioPorNome(g.Key, g))
            .ToList();

        // Resumo geral (todas as campanhas)
        var totalGeral = eventosComCampanha.Count;
        var resumoGeral = new List<HorarioResumoDTO>();
        var todosEventosPorHora = eventosComCampanha
            .GroupBy(f => (f.DataUltimoEvento ?? f.DataReferencia).Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        for (var h = 0; h < 24; h++)
        {
            var qtd = todosEventosPorHora.GetValueOrDefault(h, 0);
            resumoGeral.Add(new HorarioResumoDTO
            {
                Hora = h,
                TotalEventos = qtd,
                Percentual = totalGeral > 0 ? (decimal)qtd / totalGeral * 100 : 0
            });
        }

        var picoGeral = resumoGeral.OrderByDescending(e => e.TotalEventos).FirstOrDefault();

        return new DashboardEventosLeadPorHorarioCampanhaDTO
        {
            Campanhas = campanhas.OrderByDescending(c => c.TotalEventos).ToList(),
            ResumoGeral = resumoGeral,
            HoraPicoGeral = picoGeral?.TotalEventos > 0 ? picoGeral.Hora : null,
            TotalEventos = totalGeral
        };
    }
}

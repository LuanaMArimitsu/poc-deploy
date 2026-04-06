using System.Linq;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.DTOs.Permissao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadReaderService(ILogger<LeadReaderService> logger, ILeadRepository leadRepository, IConversaReaderService conversaReaderService, IRoleReaderService roleReaderService) : ILeadReaderService
    {
        private readonly ILogger<LeadReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ILeadRepository _leadRepository = leadRepository ?? throw new ArgumentNullException(nameof(leadRepository));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IRoleReaderService _roleReaderService = roleReaderService ?? throw new ArgumentNullException(nameof(roleReaderService));
        public async Task<Domain.Entities.Lead.Lead> GetLeadByIdAsync(int id)
        {
            try
            {
                var lead = await _leadRepository.GetLeadWithUsuarioAsync(id);
                return lead ?? throw new AppException($"Erro ao encontrar lead pelo id: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead pelo id {id}", id);
                throw;
            }
        }

        public async Task<string?> GetLeadStatusCodigoAsync(int leadStatusId)
        {
            try
            {
                return await _leadRepository.GetLeadStatusCodigoAsync(leadStatusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar código do status do lead pelo id: {id}", leadStatusId);
                throw;
            }
        }

        public async Task<LeadRetornoDTO> GetDetalhesAsync(int id, int usuarioLogado)
        {
            try
            {
                var lead = await _leadRepository.GetLeadWithDetailsAsync(id, true) ?? throw new AppException($"Erro ao encontrar Lead pelo id: {id}");

                bool possuiConversaEncerrada = await _conversaReaderService.ExisteConversaEncerradaPorLeadAsync(lead.Id);

                var ultimoEvento = lead.LeadEventos?.FirstOrDefault();

                var whats = lead.WhatsappNumero;
                var telefone = lead.Telefone;
                var email = lead.Email;

                if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioLogado)
                {
                    whats = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                    telefone = ProtegerInfoHelper.ProtegerTelefone(lead.Telefone);
                    email = ProtegerInfoHelper.ProtegerEmail(lead.Email);
                }

                LeadRetornoDTO retorno = new LeadRetornoDTO
                {
                    Id = lead.Id,
                    Nome = lead.Nome,
                    Email = email,
                    Telefone = telefone,
                    Cargo = lead.Cargo,
                    WhatsappNumero = whats,
                    CPF = lead.CPF,
                    DataNascimento = lead.DataNascimento,
                    DataCadastro = lead.DataCriacao,
                    Genero = lead.Genero,
                    NomeEmpresa = lead.NomeEmpresa,
                    CNPJEmpresa = lead.CNPJEmpresa,
                    LeadStatusId = lead.LeadStatusId,
                    LeadStatus = lead.LeadStatus?.Nome ?? string.Empty,
                    LeadStatusCor = lead.LeadStatus?.Cor ?? string.Empty,
                    DataConversaoCliente = lead.DataConversaoCliente,
                    Cliente = lead.DataConversaoCliente.HasValue,
                    NivelInteresse = lead.NivelInteresse,
                    ObservacoesCadastrais = lead.ObservacoesCadastrais,
                    ResponsavelId = lead.ResponsavelId!.Value,
                    Responsavel = lead.Responsavel?.Usuario?.Nome ?? string.Empty,
                    EquipeId = lead.EquipeId ?? 0,
                    Equipe = lead.Equipe?.Nome ?? string.Empty,
                    OrigemId = lead.OrigemId,
                    Origem = lead.Origem?.Nome ?? string.Empty,
                    EmpresaId = lead.EmpresaId,
                    Empresa = lead.Empresa?.Nome ?? string.Empty,
                    DataPrimeiroContato = lead.DataPrimeiroContato,
                    Excluido = lead.Excluido,
                    UsuarioId = lead.Responsavel!.UsuarioId,
                    PossuiConversaEncerrada = possuiConversaEncerrada,
                    CampanhaId = ultimoEvento?.CampanhaId,
                    CampanhaNome = ultimoEvento?.Campanha?.Nome ?? "O evento atual do lead não possui campanha."
                };
                return retorno;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do lead pelo id: {id}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um lead já existe com base no ID.
        /// </summary>
        /// <param name="id">o valor int do ID do lead</param>
        /// <returns>bool que revela se lead existe ou não</returns>
        public async Task<bool> LeadExistsAsync(int id)
        {
            try
            {
                return await _leadRepository.ExistsInDatabaseAsync<Domain.Entities.Lead.Lead>(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se lead existe no banco. Lead id: {id}", id);
                throw;
            }
        }

        public async Task<LeadPaginadoDTO> ListarLeadsAsync(LeadFiltroRequestDTO filtro, int usuarioIdLogado)
        {
            try
            {
                if (filtro.NumeroWhatsapp != null)
                {
                    filtro.NumeroWhatsapp = NormalizarNumeroWhatsApp(filtro.NumeroWhatsapp);
                }
                var (leads, totalItens) = await _leadRepository.ListarLeadsFiltradoAsync(
                    filtro.OrigemId,
                    filtro.StatusId,
                    filtro.UsuarioId,
                    filtro.DataCadastroInicio,
                    filtro.DataCadastroFim,
                    filtro.NivelInteresse,
                    filtro.Pagina,
                    filtro.TamanhoPagina,
                    filtro.Busca,
                    filtro.EmpresaId,
                    filtro.NumeroWhatsapp
                );

                var totalPaginas = 0;
                if (filtro.Pagina > 0 && filtro.TamanhoPagina > 0)
                {
                    totalPaginas = (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina);
                }

                var itens = new List<LeadRetornoDTO>(leads.Count);

                foreach (var l in leads)
                {
                    var conversa = await _conversaReaderService.GetConversaByLead(l.Id, "ENCERRADA");

                    bool possuiConversaEncerrada = await _conversaReaderService.ExisteConversaEncerradaPorLeadAsync(l.Id);

                    var whats = l.WhatsappNumero;
                    var telefone = l.Telefone;
                    var email = l.Email;

                    if (l.Equipe.ResponsavelMembro.UsuarioId != usuarioIdLogado)
                    {
                        whats = ProtegerInfoHelper.ProtegerTelefone(l.WhatsappNumero);
                        telefone = ProtegerInfoHelper.ProtegerTelefone(l.Telefone);
                        email = ProtegerInfoHelper.ProtegerEmail(l.Email);
                    }

                    itens.Add(new LeadRetornoDTO
                    {
                        Id = l.Id,
                        Nome = l.Nome,
                        Email = email,
                        Telefone = telefone,
                        Cargo = l.Cargo,
                        WhatsappNumero = whats,
                        CPF = l.CPF,
                        DataNascimento = l.DataNascimento,
                        DataCadastro = l.DataCriacao,
                        Genero = l.Genero,
                        NomeEmpresa = l.NomeEmpresa,
                        CNPJEmpresa = l.CNPJEmpresa,
                        LeadStatusId = l.LeadStatusId,
                        LeadStatus = l.LeadStatus?.Nome ?? string.Empty,
                        LeadStatusCor = l.LeadStatus?.Cor ?? string.Empty,
                        DataConversaoCliente = l.DataConversaoCliente,
                        Cliente = l.DataConversaoCliente.HasValue,
                        NivelInteresse = l.NivelInteresse,
                        ObservacoesCadastrais = l.ObservacoesCadastrais,
                        ResponsavelId = l.ResponsavelId!.Value,
                        Responsavel = l.Responsavel?.Usuario?.Nome ?? string.Empty,
                        EquipeId = l.EquipeId ?? 0,
                        Equipe = l.Equipe?.Nome ?? string.Empty,
                        OrigemId = l.OrigemId,
                        Origem = l.Origem?.Nome ?? string.Empty,
                        EmpresaId = l.EmpresaId,
                        Empresa = l.Empresa?.Nome ?? string.Empty,
                        DataPrimeiroContato = l.DataPrimeiroContato,
                        DataUltimaMensagem = conversa?.DataUltimaMensagem,
                        UsuarioId = l.Responsavel!.Usuario!.Id,
                        Excluido = l.Excluido,
                        PossuiConversaEncerrada = possuiConversaEncerrada
                    });
                }

                return new LeadPaginadoDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro.Pagina ?? 0,
                    TotalPaginas = totalPaginas,
                    Itens = itens
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar leads com filtros aplicados");
                throw;
            }
        }

        public async Task<LeadPaginadoDTO> ListarLeadsNovoAsync(LeadFiltrosDto filtro, int usuarioIdLogado)
        {
            try
            {
                // Normalizar WhatsApp se presente
                if (filtro.IdentificadoresFiltro?.WhatsApp != null)
                {
                    filtro.IdentificadoresFiltro.WhatsApp = NormalizarNumeroWhatsApp(filtro.IdentificadoresFiltro.WhatsApp);
                }

                var statusEncerrada = await _conversaReaderService.GetConversaStatusAsync(codigo: "ENCERRADA");

                // Chamar repository passando parâmetros individuais
                var (itens, totalItens) = await _leadRepository.ListarLeadsFiltradoAsync(
                    leadId: filtro.LeadId,
                    empresasId: filtro.EmpresaId != null ? new List<int> { filtro.EmpresaId.Value } : null,
                    equipeId: filtro.EquipeId,
                    usuarioIdLogado: usuarioIdLogado,
                    meusLeads: filtro.MeusLeads ?? false,
                    responsavelIds: filtro.ResponsavelIds,
                    comOportunidades: filtro.ComOportunidades,
                    statusIds: filtro.StatusIds,
                    origemIds: filtro.OrigemIds,
                    dataInicio: filtro.PeriodoFiltro?.DataInicio,
                    dataFim: filtro.PeriodoFiltro?.DataFim,
                    comConversasAtivas: filtro.ConversasFiltro?.ComConversasAtivas,
                    comMensagensNaoLidas: filtro.ConversasFiltro?.ComMensagensNaoLidas,
                    aguardandoResposta: filtro.ConversasFiltro?.AguardandoResposta,
                    whatsApp: filtro.IdentificadoresFiltro?.WhatsApp,
                    email: filtro.IdentificadoresFiltro?.Email,
                    cpf: filtro.IdentificadoresFiltro?.CPF,
                    textoBusca: filtro.TextoBusca,
                    pagina: filtro.Paginacao.Pagina,
                    tamanhoPagina: filtro.Paginacao.TamanhoPagina.HasValue ? filtro.Paginacao.TamanhoPagina.Value : null,
                    orderBy: filtro.Paginacao.OrderBy,
                    statusEncerrado: statusEncerrada.Id
                );

                var lista = new List<LeadRetornoDTO>();
                foreach (var lead in itens)
                {
                    bool possuiConversaEncerrada = await _conversaReaderService.ExisteConversaEncerradaPorLeadAsync(lead.Id);
                    var whats = lead.WhatsappNumero;
                    var telefone = lead.Telefone;
                    var email = lead.Email;

                    if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioIdLogado)
                    {
                        whats = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                        telefone = ProtegerInfoHelper.ProtegerTelefone(lead.Telefone);
                        email = ProtegerInfoHelper.ProtegerEmail(lead.Email);
                    }

                    var item = new LeadRetornoDTO
                    {
                        Id = lead.Id,
                        Nome = lead.Nome,
                        Email = email,
                        Telefone = telefone,
                        Cargo = lead.Cargo,
                        WhatsappNumero = whats,
                        CPF = lead.CPF,
                        DataNascimento = lead.DataNascimento,
                        DataCadastro = lead.DataCriacao,
                        Genero = lead.Genero,
                        NomeEmpresa = lead.NomeEmpresa,
                        CNPJEmpresa = lead.CNPJEmpresa,

                        // Status
                        LeadStatusId = lead.LeadStatusId,
                        LeadStatus = lead.LeadStatus?.Nome ?? string.Empty,
                        LeadStatusCor = lead.LeadStatus?.Cor ?? "#000000",

                        // Conversão e classificação
                        DataConversaoCliente = lead.DataConversaoCliente,
                        Cliente = lead.LeadStatus?.ConsiderarCliente ?? false,
                        NivelInteresse = lead.NivelInteresse,
                        ObservacoesCadastrais = lead.ObservacoesCadastrais,

                        // Responsável
                        ResponsavelId = lead.ResponsavelId!.Value,
                        Responsavel = lead.Responsavel?.Usuario?.Nome ?? string.Empty,
                        UsuarioId = lead.Responsavel!.UsuarioId,
                        EquipeId = lead.EquipeId ?? 0,
                        Equipe = lead.Equipe?.Nome ?? string.Empty,

                        // Origem
                        OrigemId = lead.OrigemId,
                        Origem = lead.Origem?.Nome ?? string.Empty,

                        // Empresa (assumindo que existe relacionamento)
                        EmpresaId = lead.EmpresaId,
                        Empresa = lead.Empresa.Nome,

                        // Metadata
                        Excluido = lead.Excluido,

                        PossuiConversaEncerrada = possuiConversaEncerrada,

                        // Datas de interação (já vem do Include das conversas)
                        DataPrimeiroContato = lead.Conversas?
                        .OrderBy(c => c.DataInicio)
                        .Select(c => c.DataInicio)
                        .FirstOrDefault(),

                        DataUltimaMensagem = lead.Conversas?
                        .Where(c => c.StatusId != statusEncerrada.Id)
                        .OrderByDescending(c => c.DataUltimaMensagem)
                        .Select(c => c.DataUltimaMensagem)
                        .FirstOrDefault()
                    };

                    lista.Add(item);
                }

                // Calcular total de páginas
                var totalPaginas = 1;
                if (filtro?.Paginacao != null)
                {
                    totalPaginas = (int)Math.Ceiling(totalItens / (double)(filtro?.Paginacao?.TamanhoPagina ?? 10));
                }

                // Retornar resultado paginado
                return new LeadPaginadoDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro?.Paginacao?.Pagina ?? 1,
                    TamanhoPagina = filtro?.Paginacao?.TamanhoPagina ?? null,
                    TotalPaginas = totalPaginas,
                    Itens = lista
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar leads com filtros aplicados");
                throw;
            }
        }

        public async Task<LeadPaginadoDTO> ListarLeadsPorPermissaoAsync(LeadFiltrosDto? filtro, int usuarioIdLogado)
        {
            try
            {
                // Normalizar WhatsApp se presente
                if (filtro?.IdentificadoresFiltro?.WhatsApp != null)
                {
                    filtro.IdentificadoresFiltro.WhatsApp = NormalizarNumeroWhatsApp(filtro.IdentificadoresFiltro.WhatsApp);
                }

                var statusEncerrada = await _conversaReaderService.GetConversaStatusAsync(codigo: "ENCERRADA");

                bool querVerOutrosLeads = false;
                var meusLeads = filtro?.MeusLeads == true;
                var responsavelIds = filtro?.ResponsavelIds;
                PermissaoEmpresasResult permissao;
                if (!meusLeads)
                {
                    if (responsavelIds == null || !responsavelIds.Any())
                    {
                        // Não filtrou responsável → quer ver todos
                        querVerOutrosLeads = true;
                    }
                    else if (responsavelIds.Count > 1)
                    {
                        // Mais de um responsável
                        querVerOutrosLeads = true;
                    }
                    else if (responsavelIds.First() != usuarioIdLogado)
                    {
                        // Apenas 1 responsável, mas não é ele
                        querVerOutrosLeads = true;
                    }
                }

                if (querVerOutrosLeads)
                {
                    permissao = await _roleReaderService.EmpresasPermissaoAsync(
                        usuarioIdLogado,
                        new List<string> { "LEAD_VISUALIZAR_TODOS" }
                    );

                    if (!permissao.AcessoGlobal && permissao.EmpresasIds.Count == 0)
                        throw new AppException("Usuário não possui permissão para visualizar leads de outros usuários.");
                }
                else
                {
                    permissao = await _roleReaderService.EmpresasPermissaoAsync(
                        usuarioIdLogado,
                        new List<string> { "LEAD_VISUALIZAR" }
                    );

                    if (!permissao.AcessoGlobal && permissao.EmpresasIds.Count == 0)
                        throw new AppException("Usuário não possui permissão para visualizar seus leads.");
                }


                if (filtro?.EmpresaId is not null && !permissao.AcessoGlobal && !permissao.EmpresasIds.Contains(filtro.EmpresaId.Value))
                {
                    throw new AppException("Usuário não possui permissão para visualizar leads desta empresa.");
                }

                List<int>? empresasIds = null;

                if (filtro?.EmpresaId != null)
                {
                    empresasIds = new List<int> { filtro.EmpresaId.Value };
                }
                else if (!permissao.AcessoGlobal)
                {
                    empresasIds = permissao.EmpresasIds;
                }

                // Chamar repository passando parâmetros individuais
                var (itens, totalItens) = await _leadRepository.ListarLeadsFiltradoAsync(
                        leadId: filtro?.LeadId,
                        empresasId: empresasIds,
                        equipeId: filtro?.EquipeId,
                        usuarioIdLogado: usuarioIdLogado,
                        meusLeads: filtro?.MeusLeads ?? false,
                        responsavelIds: filtro?.ResponsavelIds,
                        comOportunidades: filtro?.ComOportunidades,
                        statusIds: filtro?.StatusIds,
                        origemIds: filtro?.OrigemIds,
                        dataInicio: filtro?.PeriodoFiltro?.DataInicio,
                        dataFim: filtro?.PeriodoFiltro?.DataFim,
                        comConversasAtivas: filtro?.ConversasFiltro?.ComConversasAtivas,
                        comMensagensNaoLidas: filtro?.ConversasFiltro?.ComMensagensNaoLidas,
                        aguardandoResposta: filtro?.ConversasFiltro?.AguardandoResposta,
                        whatsApp: filtro?.IdentificadoresFiltro?.WhatsApp,
                        email: filtro?.IdentificadoresFiltro?.Email,
                        cpf: filtro?.IdentificadoresFiltro?.CPF,
                        textoBusca: filtro?.TextoBusca,
                        pagina: filtro?.Paginacao?.Pagina,
                        tamanhoPagina: filtro?.Paginacao?.TamanhoPagina,
                        orderBy: filtro?.Paginacao?.OrderBy,
                        statusEncerrado: statusEncerrada.Id
                    );

                var lista = new List<LeadRetornoDTO>();
                foreach (var lead in itens)
                {
                    bool possuiConversaEncerrada = await _conversaReaderService.ExisteConversaEncerradaPorLeadAsync(lead.Id);
                    var whats = lead.WhatsappNumero;
                    var telefone = lead.Telefone;
                    var email = lead.Email;
                    if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioIdLogado)
                    {
                        whats = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                        telefone = ProtegerInfoHelper.ProtegerTelefone(lead.Telefone);
                        email = ProtegerInfoHelper.ProtegerEmail(lead.Email);
                    }

                    var item = new LeadRetornoDTO
                    {
                        Id = lead.Id,
                        Nome = lead.Nome,
                        Email = email,
                        Telefone = telefone,
                        Cargo = lead.Cargo,
                        WhatsappNumero = whats,
                        CPF = lead.CPF,
                        DataNascimento = lead.DataNascimento,
                        DataCadastro = lead.DataCriacao,
                        Genero = lead.Genero,
                        NomeEmpresa = lead.NomeEmpresa,
                        CNPJEmpresa = lead.CNPJEmpresa,

                        // Status
                        LeadStatusId = lead.LeadStatusId,
                        LeadStatus = lead.LeadStatus?.Nome ?? string.Empty,
                        LeadStatusCor = lead.LeadStatus?.Cor ?? "#000000",

                        // Conversão e classificação
                        DataConversaoCliente = lead.DataConversaoCliente,
                        Cliente = lead.LeadStatus?.ConsiderarCliente ?? false,
                        NivelInteresse = lead.NivelInteresse,
                        ObservacoesCadastrais = lead.ObservacoesCadastrais,

                        // Responsável
                        ResponsavelId = lead.ResponsavelId!.Value,
                        Responsavel = lead.Responsavel?.Usuario?.Nome ?? string.Empty,
                        UsuarioId = lead.Responsavel!.UsuarioId,
                        EquipeId = lead.EquipeId ?? 0,
                        Equipe = lead.Equipe?.Nome ?? string.Empty,

                        // Origem
                        OrigemId = lead.OrigemId,
                        Origem = lead.Origem?.Nome ?? string.Empty,

                        // Empresa (assumindo que existe relacionamento)
                        EmpresaId = lead.EmpresaId,
                        Empresa = lead.Empresa.Nome,

                        // Metadata
                        Excluido = lead.Excluido,

                        PossuiConversaEncerrada = possuiConversaEncerrada,

                        // Datas de interação (já vem do Include das conversas)
                        DataPrimeiroContato = lead.Conversas?
                        .OrderBy(c => c.DataInicio)
                        .Select(c => c.DataInicio)
                        .FirstOrDefault(),

                        DataUltimaMensagem = lead.Conversas?
                        .Where(c => c.StatusId != statusEncerrada.Id)
                        .OrderByDescending(c => c.DataUltimaMensagem)
                        .Select(c => c.DataUltimaMensagem)
                        .FirstOrDefault()
                    };

                    lista.Add(item);
                }

                // Calcular total de páginas
                var totalPaginas = 1;
                if (filtro?.Paginacao != null)
                {
                    totalPaginas = (int)Math.Ceiling(totalItens / (double)(filtro?.Paginacao?.TamanhoPagina ?? 10));
                }

                // Retornar resultado paginado
                return new LeadPaginadoDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro?.Paginacao?.Pagina ?? 1,
                    TamanhoPagina = filtro?.Paginacao?.TamanhoPagina ?? null,
                    TotalPaginas = totalPaginas,
                    Itens = lista
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar leads com filtros aplicados");
                throw;
            }
        }

        private LeadFiltrosAplicadosDto MontarFiltrosAplicados(LeadFiltrosDto filtro, List<Domain.Entities.Lead.Lead> leads)
        {
            var filtrosAplicados = new LeadFiltrosAplicadosDto
            {
                MeusLeads = filtro.MeusLeads ?? false,
                ComOportunidades = filtro.ComOportunidades,
                ComConversasAtivas = filtro.ConversasFiltro?.ComConversasAtivas,
                ComMensagensNaoLidas = filtro.ConversasFiltro?.ComMensagensNaoLidas
            };

            // Extrair nomes dos status dos leads já carregados (vem do Include)
            if (filtro.StatusIds?.Any() == true && leads.Any())
            {
                filtrosAplicados.Status = leads
                    .Where(l => filtro.StatusIds.Contains(l.LeadStatusId))
                    .Select(l => l.LeadStatus?.Nome)
                    .Where(n => n != null)
                    .Distinct()
                    .ToList()!;
            }

            // Extrair nomes das origens dos leads já carregados (vem do Include)
            if (filtro.OrigemIds?.Any() == true && leads.Any())
            {
                filtrosAplicados.Origens = leads
                    .Where(l => filtro.OrigemIds.Contains(l.OrigemId))
                    .Select(l => l.Origem?.Nome)
                    .Where(n => n != null)
                    .Distinct()
                    .ToList()!;
            }

            // Extrair nomes dos responsáveis dos leads já carregados (vem do Include)
            if (filtro.ResponsavelIds?.Any() == true && leads.Any())
            {
                filtrosAplicados.Responsaveis = leads
                    .Where(l => filtro.ResponsavelIds.Contains(l.ResponsavelId.Value))
                    .Select(l => l.Responsavel.Usuario.Nome)
                    .Where(n => n != null)
                    .Distinct()
                    .ToList()!;
            }

            // Montar descrição do período
            if (filtro.PeriodoFiltro != null)
            {
                if (filtro.PeriodoFiltro.DataInicio.HasValue && filtro.PeriodoFiltro.DataFim.HasValue)
                {
                    var dataInicio = filtro.PeriodoFiltro.DataInicio.Value;
                    var dataFim = filtro.PeriodoFiltro.DataFim.Value;

                    // Verificar se é "Hoje"
                    if (dataInicio.Date == DateTime.Today && dataFim.Date == DateTime.Today)
                    {
                        filtrosAplicados.Periodo = "Hoje";
                    }
                    // Verificar se é "Últimos 7 dias"
                    else if (dataInicio.Date == DateTime.Today.AddDays(-7) && dataFim.Date == DateTime.Today)
                    {
                        filtrosAplicados.Periodo = "Últimos 7 dias";
                    }
                    // Verificar se é "Últimos 30 dias"
                    else if (dataInicio.Date == DateTime.Today.AddDays(-30) && dataFim.Date == DateTime.Today)
                    {
                        filtrosAplicados.Periodo = "Últimos 30 dias";
                    }
                    else
                    {
                        filtrosAplicados.Periodo = $"{dataInicio:dd/MM/yyyy} - {dataFim:dd/MM/yyyy}";
                    }
                }
                else if (filtro.PeriodoFiltro.DataInicio.HasValue)
                {
                    filtrosAplicados.Periodo = $"A partir de {filtro.PeriodoFiltro.DataInicio.Value:dd/MM/yyyy}";
                }
                else if (filtro.PeriodoFiltro.DataFim.HasValue)
                {
                    filtrosAplicados.Periodo = $"Até {filtro.PeriodoFiltro.DataFim.Value:dd/MM/yyyy}";
                }
            }

            return filtrosAplicados;
        }

        public async Task<List<StatusLeadDTO>> ListarStatusDoLeadAsync()
        {
            var status = await _leadRepository.ListarStatusAsync();

            return status.Select(t => new StatusLeadDTO
            {
                Id = t.Id,
                Codigo = t.Codigo,
                Nome = t.Nome,
                Cor = t.Cor ?? "#808080"
            }).ToList();
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPendentesDistribuicaoAsync(int empresaId, int maxLeads)
        {
            try
            {
                return await _leadRepository.ObterLeadsPendentesDistribuicaoAsync(empresaId, maxLeads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter leads pendentes de distribuição para empresa ID {empresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> CountLeadsDistribuidosAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            try
            {
                return await _leadRepository.CountLeadsDistribuidosAsync(empresaId, dataInicio, dataFim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar leads recebidos por vendedor ID {vendedorId}", empresaId);
                throw;
            }
        }

        /// <summary>
        /// Normaliza o número do WhatsApp removendo caracteres especiais e formatações
        /// </summary>
        public string? NormalizarNumeroWhatsApp(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return null;

            // Remove todos os caracteres não numéricos
            var numeroLimpo = new string(numero.Where(char.IsDigit).ToArray());

            // Adiciona código do país se não estiver presente (Brasil = 55)
            if (numeroLimpo.Length == 11 && (numeroLimpo.StartsWith("11") || numeroLimpo.StartsWith("1")))
            {
                numeroLimpo = "55" + numeroLimpo;
            }
            else if (numeroLimpo.Length == 10)
            {
                numeroLimpo = "5511" + numeroLimpo;
            }

            return numeroLimpo;
        }

        public async Task<bool> ExisteLeadAtribuidoAsync(int equipeId)
        {
            try
            {
                return await _leadRepository.ExisteLeadAtribuidoAsync(equipeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se existe lead atribuído à equipe {EquipeId}", equipeId);
                throw;
            }
        }

        public async Task<Domain.Entities.Lead.Lead> GetLeadComResponsavelAsync(int id)
        {
            try
            {
                return await _leadRepository.GetLeadsComResponsavelAsync(id) ?? throw new AppException($"Erro ao encontrar lead pelo id: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead pelo id {id}", id);
                throw;
            }
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPorIdsAsync(IEnumerable<int> leadIds, bool includeDeleted = false)
        {
            var ids = leadIds.ToList();
            if (ids.Count == 0) return new List<Domain.Entities.Lead.Lead>();
            return await _leadRepository.GetListByPredicateAsync<Domain.Entities.Lead.Lead>(l => ids.Contains(l.Id), includeDeleted: includeDeleted);
        }

        public Task<List<Domain.Entities.Lead.Lead>> ObterLeadsComResponsavelUsuarioPorIdsAsync(
            IEnumerable<int> leadIds, bool includeDeleted = true) =>
            _leadRepository.GetLeadsComResponsavelUsuarioPorIdsAsync(leadIds, includeDeleted);

        public async Task<List<Domain.Entities.Lead.LeadStatus>> ListarStatusLeadEntidadesAsync()
        {
            return await _leadRepository.ListarStatusAsync();
        }

        public async Task<Domain.Entities.Lead.Lead?> ObterLeadComResponsavelParaETLAsync(int id, bool includeDeleted = true)
        {
            return await _leadRepository.GetLeadComResponsavelAsync(id, includeDeleted);
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPorPeriodoModificacaoParaETLAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _leadRepository.GetListByPredicateAsync<Domain.Entities.Lead.Lead>(l =>
                l.DataModificacao >= dataInicio && l.DataModificacao <= dataFim, includeDeleted: true);
        }

        public async Task<Domain.Entities.Lead.Lead> ObterLeadPorGrupoAsync(string? whatsAppNumero, string? email, string? cpf, int grupoEmpresaId)
        {
            try
            {
                if (grupoEmpresaId <= 0)
                    throw new AppException("Grupo da empresa inválido.");

                var whatsappNormalizado = NormalizarNumeroWhatsApp(whatsAppNumero);

                if (!string.IsNullOrWhiteSpace(cpf))
                    cpf = new string(cpf.Where(char.IsDigit).ToArray());

                return await _leadRepository.ObterLeadExistenteNoMesmoGrupo(
                    whatsappNormalizado,
                    email,
                    cpf,
                    grupoEmpresaId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao buscar lead existente no mesmo grupo. GrupoEmpresaId: {GrupoEmpresaId}",
                    grupoEmpresaId);
                throw;
            }
        }

        public bool LeadPertenceAoBot(Domain.Entities.Lead.Lead lead)
        {
            return lead.Responsavel?.Usuario?.IsBot == true;
        }

        public async Task<LeadStatus?> GetLeadStatusByCodigo(string? codigo = null)
        {
            return await _leadRepository.GetByPredicateAsync<LeadStatus>(s => s.Codigo == codigo);
        }
    }
}

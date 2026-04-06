using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Application.Services.Usuario
{
    public class UsuarioReaderService(ILogger<UsuarioReaderService> logger, IUsuarioRepository usuarioRepository, IAzureAdService azureAdService, IEmpresaReaderService empresaReaderService, IMembroEquipeReaderService membroEquipeReaderService, IConversaReaderService conversaReaderService) : IUsuarioReaderService
    {
        private readonly ILogger<UsuarioReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        private readonly IAzureAdService _azureAdService = azureAdService ?? throw new ArgumentNullException(nameof(azureAdService));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));

        public async Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterUsuarioPorIdAsync(int id)
        {
            return await _usuarioRepository.ObterUsuarioPorIdAsync(id);
        }

        public async Task<Domain.Entities.Usuario.Usuario?> GetUsuarioByEmail(string email)
        {
            return await _usuarioRepository.GetByPredicateAsync<Domain.Entities.Usuario.Usuario>(e => e.Email == email);
        }

        public async Task<Domain.Entities.Usuario.Usuario?> GetUsuarioBot()
        {
            return await _usuarioRepository.GetByPredicateAsync<Domain.Entities.Usuario.Usuario>(u => u.IsBot && !u.Excluido);
        }

        public async Task<List<AzureUserDTO>> BuscarUsuariosAzureAdPorNome(string? startsWith)
        {
            var usuariosAzure = await _azureAdService.GetUserAsync(startsWith);

            foreach (var user in usuariosAzure)
            {
                var usuarioDb = await _usuarioRepository.BuscarUsuarioPorObjectIdAsync(user.Id);
                if (usuarioDb != null)
                {
                    user.Cadastrado = true;
                    user.IdUsuario = usuarioDb.Id;
                    user.Ativo = usuarioDb.Ativo;
                    user.UsuarioSuperiorId = usuarioDb.UsuarioSuperiorId;
                }
            }

            return usuariosAzure;
        }

        public async Task<UsuarioDetalheSimplesDTO?> ObterUsuarioDetalhadoSimplesAsync(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(id);
                if (usuario == null)
                    return null;

                var usuarioSuperior = await _usuarioRepository.GetByPredicateAsync<Domain.Entities.Usuario.Usuario>(
                    u => u.Id == usuario.UsuarioSuperiorId
                );

                var dto = new UsuarioDetalheSimplesDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email ?? string.Empty,
                    Cargo = usuario.Cargo ?? string.Empty,
                    Departamento = usuario.Departamento ?? string.Empty,
                    Ativo = usuario.Ativo,
                    Cadastrado = true,
                    UsuarioSuperiorId = usuario.UsuarioSuperiorId,
                    UsuarioSuperiorNome = usuarioSuperior?.Nome ?? string.Empty,
                    InicialAvatar = ObterIniciais(usuario.Nome),
                    CorAvatar = "#9b59b6"
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário detalhado simples para o ID {UsuarioId}", id);
                throw;
            }
        }

        public async Task<UsuarioDetalheDTO?> ObterUsuarioDetalhadoAsync(int id)
        {
            var usuario = await _usuarioRepository.BuscarUsuarioPorIdAsync(id);

            if (usuario == null) return null;

            var empresaPrincipal = usuario.UsuarioEmpresas.FirstOrDefault(e => e.IsPrincipal);

            var membrosUsuario = await _membroEquipeReaderService.ObterMembrosPorUsuarioAsync(usuario.Id);

            var totalLeadsResponsavel = membrosUsuario.Where(m => !m.Excluido && m.DataSaida == null).Sum(m => m.LeadsSobResponsabilidade?.Count(l => !l.Excluido) ?? 0);

            var statusFinalizado = await _conversaReaderService.GetConversaStatusAsync(codigo: "ENCERRADA");

            var dto = new UsuarioDetalheDTO
            {
                // Informações Básicas Usuario
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email ?? string.Empty,
                Cargo = usuario.Cargo ?? string.Empty,
                Departamento = usuario.Departamento ?? string.Empty,
                Ativo = usuario.Ativo,
                Cadastrado = true,

                UltimoAcesso = usuario.Dispositivos.MaxBy(d => d.UltimaSincronizacao)?.UltimaSincronizacao,

                // Avatar
                InicialAvatar = ObterIniciais(usuario.Nome),
                CorAvatar = "#9b59b6",

                // Hierarquia
                UsuarioSuperiorId = usuario.UsuarioSuperiorId,
                UsuarioSuperiorNome = usuario.UsuarioSuperior?.Nome ?? string.Empty,

                // Azure AD
                ObjectId = usuario.ObjectId,
                Upn = usuario.Upn,
                DisplayName = usuario.DisplayName,
                IsExternal = usuario.IsExternal,

                //Empresas
                EmpresaPrincipal = empresaPrincipal != null ? new EmpresaDetalheDTO
                {
                    Id = empresaPrincipal.Empresa.Id,
                    Nome = empresaPrincipal.Empresa.Nome,
                    RazaoSocial = empresaPrincipal.Empresa.RazaoSocial,
                    Cnpj = empresaPrincipal.Empresa.Cnpj,
                    Telefone = empresaPrincipal.Empresa.Telefone,
                    Email = empresaPrincipal.Empresa.Email,
                    Ativo = empresaPrincipal.Empresa.Ativo,
                    GrupoEmpresa = empresaPrincipal.Empresa.GrupoEmpresa != null ? new GrupoEmpresaDTO
                    {
                        Id = empresaPrincipal.Empresa.GrupoEmpresa.Id,
                        Nome = empresaPrincipal.Empresa.GrupoEmpresa.Nome,
                        CnpjHolding = empresaPrincipal.Empresa.GrupoEmpresa.CnpjHolding,
                        Ativo = empresaPrincipal.Empresa.GrupoEmpresa.Ativo
                    } : null
                } : null,

                Empresas = usuario.UsuarioEmpresas.Select(e => new UsuarioEmpresaDTO
                {
                    Id = e.Id,
                    EmpresaId = e.Empresa.Id,
                    EmpresaNome = e.Empresa.Nome,
                    EmpresaCnpj = e.Empresa.Cnpj,
                    IsPrincipal = e.IsPrincipal,
                    DataAssociacao = e.DataAssociacao,
                    CodVendedorNBS = e.CodVendedorNBS,
                    GrupoEmpresaNome = e.Empresa.GrupoEmpresa?.Nome ?? string.Empty,
                    CanalPadraoId = e.CanalPadraoId,
                    CanalPadraoNome = e.CanalPadrao?.Nome ?? string.Empty
                }).ToList(),

                // Dispositivos
                Dispositivos = usuario.Dispositivos.Select(d => new DispositivoDTO
                {
                    Id = d.Id,
                    DeviceId = d.DeviceId,
                    Modelo = d.Modelo,
                    Ativo = d.Ativo,
                    UltimaSincronizacao = d.UltimaSincronizacao,
                    SignalRConnectionId = d.SignalRConnectionId,
                    UltimoHeartbeatSignalR = d.UltimoHeartbeatSignalR,
                    Online = d.EstaConectadoViaSignalR()
                }).ToList(),

                // Horários
                HorariosTrabalho = MontarHorariosTrabalhoCompletos(usuario.HorariosUsuario),

                // Estatísticas
                TotalLeadsResponsavel = totalLeadsResponsavel,
                TotalConversasAtivas = usuario.Conversas.Count(c => c.StatusId != statusFinalizado.Id),

                // Auditoria
                DataCriacao = usuario.DataCriacao,
                DataModificacao = usuario.DataModificacao
            };

            return dto;
        }

        public async Task<Domain.Entities.Usuario.Usuario?> GetEmpresaByUsuarioId(int id)
        {
            try
            {
                return await _usuarioRepository.GetEmpresaByUsuarioId(id);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro ao retornar empresa do usuário {UsuarioId}", id);
                throw;
            }
        }
        private static readonly int[] StatusFinalizados = { 35, 36 };

        public async Task<PagedResponseDTO<UsuarioListagemDTO>> ListarUsuariosAsync(UsuarioFiltroRequestDTO filtro)
        {
            if (filtro == null)
                throw new ArgumentNullException(nameof(filtro));

            var (usuarios, totalItens) = await _usuarioRepository.ObterUsuariosComFiltros(filtro.EmpresaId, filtro.Nome, filtro.Ativo, filtro.TamanhoPagina, filtro.Pagina);

            int totalPaginas = 0;
            if (filtro.Pagina > 0 && filtro.TamanhoPagina > 0)
            {

                totalPaginas = (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina);
            }

            // Paginação
            var itens = usuarios.Select(u => new UsuarioListagemDTO
            {
                Id = u.Id,
                Nome = u.Nome ?? "",
                Email = u.Email ?? "",
                Cargo = u.Cargo ?? "",
                Departamento = u.Departamento ?? "",
                Ativo = u.Ativo,
                EmpresaPrincipalId = u.UsuarioEmpresas.FirstOrDefault(ue => ue.IsPrincipal)!.EmpresaId,
                EmpresaPrincipal = u.UsuarioEmpresas.FirstOrDefault(ue => ue.IsPrincipal).Empresa.Nome,
                InicialAvatar = ObterIniciais(u.Nome),
                CorAvatar = "#9b59b6"
            })
            .ToList();

            return new PagedResponseDTO<UsuarioListagemDTO>
            {
                Itens = itens,
                PaginaAtual = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalPaginas
            };
        }
        public async Task<List<UsuarioSuperiorDTO>> ObterUsuariosSuperioresAsync()
        {
            List<Domain.Entities.Usuario.Usuario> usuarios = await _usuarioRepository.ObterUsuariosComSubordinadosAsync();
            return usuarios
                .Select(u => new UsuarioSuperiorDTO
                {
                    Id = u.Id,
                    Nome = u.Nome
                })
                .ToList();
        }

        public async Task<List<UsuarioEmpresaDTO>?> ObterEmpresasUsuarioAsync(int usuarioId)
        {
            var empresas = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);

            if (empresas == null || !empresas.Any())
                return null;

            return empresas.Select(e => new UsuarioEmpresaDTO
            {
                Id = e.Id,
                EmpresaId = e.EmpresaId,
                EmpresaNome = e.Empresa?.Nome ?? string.Empty,
                EmpresaCnpj = e.Empresa?.Cnpj ?? string.Empty,
                IsPrincipal = e.IsPrincipal,
                DataAssociacao = e.DataAssociacao,
                Logo = e.Empresa?.GrupoEmpresa?.Logo ?? string.Empty,
                GrupoEmpresaNome = e.Empresa?.GrupoEmpresa?.Nome ?? string.Empty,
                CanalPadraoId = e.CanalPadraoId,
                CanalPadraoNome = e.CanalPadrao?.Nome ?? string.Empty,
                CodVendedorNBS = e.CodVendedorNBS,
                EquipePadraoId = e.EquipePadraoId,
                EquipePadraoNome = e.EquipePadrao?.Nome ?? string.Empty
            }).ToList();
        }

        public async Task<bool> UserExistsAsync(int usuarioId)
        {
            return await _usuarioRepository.ExistsInDatabaseAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
        }

        public async Task<List<UsuarioHorarioDTO>> ObterHorariosUsuarioAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
            if (usuario is null)
                return null!;

            var horarios = MontarHorariosTrabalhoCompletos(usuario.HorariosUsuario);

            return horarios;
        }

        public async Task<Dictionary<int, List<UsuarioHorarioDTO>>> ObterHorariosMultiplosUsuariosAsync(IEnumerable<int> usuarioIds)
        {
            var result = new Dictionary<int, List<UsuarioHorarioDTO>>();
            foreach (var id in usuarioIds.Distinct())
            {
                var horarios = await ObterHorariosUsuarioAsync(id);
                if (horarios != null)
                    result[id] = horarios;
            }
            return result;
        }

        public async Task<(List<WebsupplyConnect.Domain.Entities.Usuario.Usuario> Vendedores, bool FallbackAplicado, string? DetalhesFallback)> ObterVendedoresDisponiveisAsync(int empresaId, ConfiguracaoDistribuicao configuracao)
        {
            try
            {
                _logger.LogInformation("Obtendo vendedores disponíveis. Empresa: {EmpresaId}, ConsiderarHorario: {ConsiderarHorario}, ConsiderarFeriados: {ConsiderarFeriados}",
                    empresaId, configuracao.ConsiderarHorarioTrabalho, configuracao.ConsiderarFeriados);

                // 1. Se não considerar horário, retornar todos os vendedores ativos
                if (!configuracao.ConsiderarHorarioTrabalho)
                {
                    var vendedoresSemFiltro = await _usuarioRepository.ObterVendedoresAtivosPorEmpresaAsync(empresaId);
                    _logger.LogInformation("Configuração não considera horário. Retornando {Count} vendedores",
                        vendedoresSemFiltro.Count);
                    return (vendedoresSemFiltro, false, null);
                }

                var dataAtual = TimeHelper.GetBrasiliaTime();
                var horaAtual = dataAtual.TimeOfDay;
                var diaSemanaNet = (int)dataAtual.DayOfWeek;

                // Converter do enum .NET (0=Domingo, 1=Segunda, etc.) para o padrão do banco (1=Domingo, 2=Segunda, etc.)
                var diaSemana = diaSemanaNet == 0 ? 1 : diaSemanaNet + 1;

                _logger.LogInformation("Verificando disponibilidade. Dia .NET: {DiaSemanaNet}, Dia Banco: {DiaSemana}, Hora: {HoraAtual}",
                    diaSemanaNet, diaSemana, horaAtual);

                // 2. Verificar fim de semana ANTES de aplicar filtro de horário
                if (configuracao.ConsiderarFeriados)
                {
                    if (diaSemanaNet == 0 || diaSemanaNet == 6) // Domingo (0) ou Sábado (6)
                    {
                        _logger.LogWarning("Fim de semana detectado (Dia: {DiaSemanaNet}). Aplicando fallback devido a ConsiderarFeriados=True", diaSemanaNet);

                        // FALLBACK: Se for fim de semana e configurado para considerar, retorna todos os vendedores ativos
                        var detalhesFallbackFimSemana = $"Fallback aplicado: Fim de semana detectado (Dia: {diaSemanaNet}) e ConsiderarFeriados=True. Retornando todos os vendedores ativos da empresa.";

                        var vendedoresFallbackFimSemana = await _usuarioRepository.ObterVendedoresAtivosPorEmpresaAsync(empresaId);
                        _logger.LogInformation("Fallback para fim de semana: retornando {Count} vendedores ativos da empresa",
                            vendedoresFallbackFimSemana.Count);
                        return (vendedoresFallbackFimSemana, true, detalhesFallbackFimSemana);
                    }
                }

                // 3. Aplicar filtro de horário
                _logger.LogInformation("Executando query para vendedores no horário. DiaSemanaId: {DiaSemana}, HoraAtual: {HoraAtual}",
                    diaSemana, horaAtual);

                var vendedoresNoHorario = await _usuarioRepository.ObterVendedoresDisponiveisNoHorarioAsync(empresaId, diaSemana, horaAtual);

                // 4. Se encontrou vendedores no horário, retorna eles
                if (vendedoresNoHorario.Count != 0)
                {
                    _logger.LogInformation("Retornando {Count} vendedores com horário válido", vendedoresNoHorario.Count);
                    return (vendedoresNoHorario, false, null);
                }

                // 5. FALLBACK: Se não encontrou vendedores no horário, retorna todos os vendedores ativos
                var detalhesFallback = $"Fallback aplicado: Nenhum vendedor encontrado no horário atual (Dia: {diaSemana}, Hora: {horaAtual}). " +
                    "Retornando todos os vendedores ativos da empresa.";

                _logger.LogWarning("Nenhum vendedor encontrado no horário atual (Dia: {DiaSemana}, Hora: {HoraAtual}). " +
                    "Aplicando fallback: retornando todos os vendedores ativos", diaSemana, horaAtual);

                var vendedoresFallback = await _usuarioRepository.ObterVendedoresAtivosPorEmpresaAsync(empresaId);
                _logger.LogInformation("Fallback aplicado: retornando {Count} vendedores ativos da empresa",
                    vendedoresFallback.Count);
                return (vendedoresFallback, true, detalhesFallback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter vendedores disponíveis para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<UsuarioSimplesDTO>> UsuariosEmpresa(int empresaId)
        {
            try
            {
                var empresaExiste = await _empresaReaderService.EmpresaExistsAsync(empresaId);
                if (!empresaExiste)
                    throw new DomainException("A empresa informada não existe.");

                var usuarios = await _usuarioRepository.ObterUsuariosPorEmpresaAsync(empresaId);

                return usuarios
                    .Select(u => new UsuarioSimplesDTO
                    {
                        Id = u.Id,
                        Nome = u.Nome
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuários da empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        private static string ObterIniciais(string nome)
        {
            var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length >= 2
                ? $"{partes[0][0]}{partes[1][0]}".ToUpper()
                : partes[0][..1].ToUpper();
        }

        private static List<UsuarioHorarioDTO> MontarHorariosTrabalhoCompletos(IEnumerable<UsuarioHorario> horariosUsuario)
        {
            var diasSemana = new Dictionary<int, (string Nome, string Abreviacao)>
            {
                { 1, ("Domingo", "Dom") },
                { 2, ("Segunda-feira", "Seg") },
                { 3, ("Terça-feira", "Ter") },
                { 4, ("Quarta-feira", "Qua") },
                { 5, ("Quinta-feira", "Qui") },
                { 6, ("Sexta-feira", "Sex") },
                { 7, ("Sábado", "Sáb") },
            };

            var horariosCompletos = new List<UsuarioHorarioDTO>();

            for (int dia = 1; dia <= 7; dia++)
            {
                var horario = horariosUsuario.FirstOrDefault(h => h.DiaSemanaId == dia);
                var (descricao, abreviacao) = diasSemana[dia];

                if (horario == null)
                {
                    horariosCompletos.Add(new UsuarioHorarioDTO
                    {
                        Id = 0,
                        DiaSemanaId = dia,
                        DiaSemanaDescricao = descricao,
                        DiaSemanaAbreviacao = abreviacao,
                        SemExpediente = true,
                        HorarioInicio = null,
                        HorarioFim = null,
                        //DuracaoHoras = null,
                        HorarioFormatado = "Sem expediente"
                    });
                }
                else
                {
                    var duracao = horario.CalcularDuracaoExpediente();
                    var ehSemExpediente = horario.HorarioInicio == horario.HorarioFim || duracao == 0;

                    horariosCompletos.Add(new UsuarioHorarioDTO
                    {
                        Id = horario.Id,
                        DiaSemanaId = dia,
                        DiaSemanaDescricao = descricao,
                        DiaSemanaAbreviacao = abreviacao,
                        SemExpediente = ehSemExpediente,
                        HorarioInicio = ehSemExpediente ? null : horario.HorarioInicio,
                        HorarioFim = ehSemExpediente ? null : horario.HorarioFim,
                        //DuracaoHoras = ehSemExpediente ? null : duracao,
                        HorarioFormatado = ehSemExpediente
                            ? "Sem expediente"
                            : $"{horario.HorarioInicio:hh\\:mm} - {horario.HorarioFim:hh\\:mm}",
                        IsTolerancia = horario.IsTolerancia
                    });
                }
            }

            return horariosCompletos;
        }

        public async Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterUsuariosAtivosNaoBotParaETLAsync()
        {
            return await _usuarioRepository.GetListByPredicateAsync<WebsupplyConnect.Domain.Entities.Usuario.Usuario>(u => !u.Excluido && !u.IsBot, includeDeleted: true);
        }

        public async Task<HashSet<int>> ObterBotUserIdsAsync()
        {
            var bots = await _usuarioRepository.GetListByPredicateAsync<WebsupplyConnect.Domain.Entities.Usuario.Usuario>(u => u.IsBot && !u.Excluido, includeDeleted: false);
            return bots.Select(b => b.Id).ToHashSet();
        }
        public async Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterVendedorPorMembroId(int usuarioId)
        {
            return await _usuarioRepository.ObterUsuarioPorMembroId(usuarioId);
        }
    }
}

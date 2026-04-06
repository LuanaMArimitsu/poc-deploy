using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Equipe;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class MembroEquipeReaderService : IMembroEquipeReaderService
    {
        private readonly IEquipeRepository _equipeRepo;
        private readonly IMembroEquipeRepository _membroRepo;
        private readonly ILogger<MembroEquipeReaderService> _logger;

        public MembroEquipeReaderService(IEquipeRepository equipeRepo, IMembroEquipeRepository membroRepo, ILogger<MembroEquipeReaderService> logger)
        {
            _equipeRepo = equipeRepo;
            _membroRepo = membroRepo;
            _logger = logger;
        }

        public async Task<MembrosEquipePaginadoDto> ListarMembrosAsync(MembrosEquipeFiltroRequestDto filtro)
        {
            if (filtro is null) throw new DomainException("Payload inválido.");
            if (filtro.EquipeId <= 0) throw new DomainException("EquipeId é obrigatório.");

            var equipe = await _equipeRepo.GetByIdAsync<Domain.Entities.Equipe.Equipe>(filtro.EquipeId);
            if (equipe is null) throw new DomainException("Equipe não encontrada.");

            var apenasAtivos = filtro.ApenasAtivos ?? true;
            var buscaNome = string.IsNullOrWhiteSpace(filtro.Busca) ? null : filtro.Busca.Trim();

            if (filtro.StatusIds is not null && filtro.StatusIds.Any())
            {
                var invalido = filtro.StatusIds.Any(s => s < 94 || s > 98);
                if (invalido) throw new DomainException("Status inválido (válidos: 94..98).");
            }

            var (itens, total) = await _membroRepo.ListarMembrosAsync(
                filtro.EquipeId, apenasAtivos, filtro.StatusIds, buscaNome, filtro.Pagina, filtro.TamanhoPagina);

            var list = itens.Select(m => new ListMembroEquipeDto
            {
                Id = m.Id,
                UsuarioId = m.UsuarioId,
                UsuarioNome = m.Usuario?.Nome ?? string.Empty,
                StatusMembroEquipeId = m.StatusMembroEquipeId,
                StatusNome = m.StatusMembroEquipe?.Nome ?? string.Empty,
                IsLider = m.IsLider,
                DataSaida = m.DataSaida,
                Observacoes = m.Observacoes
            }).ToList();

            var totalPaginas = 0;
            var paginaAtual = 1;
            if (filtro.TamanhoPagina > 0)
            {
                totalPaginas = (int)Math.Ceiling(total / (double)filtro.TamanhoPagina);
                paginaAtual = filtro.Pagina;
            }

            return new MembrosEquipePaginadoDto
            {
                TotalItens = total,
                PaginaAtual = paginaAtual,
                TotalPaginas = totalPaginas,
                Itens = list
            };
        }

        public async Task<List<MembroEquipe>> ObterMembrosPorEquipe(int equipeId, string? statusCodigo = null, int? statusId = null)
        {
            return await _membroRepo.ObterMembrosPorEquipeAsync(equipeId, statusCodigo, statusId);
        }

        public async Task<MembroEquipe> ObterLiderDaEquipeAsync(int equipeId)
        {
            if (equipeId <= 0)
                throw new DomainException("EquipeId inválido para consulta de líder.");
            var lider = await _membroRepo.GetLiderDaEquipeAsync(equipeId);
            if (lider is null)
            {
                _logger.LogError("Líder não encontrado para a equipe {EquipeId}.", equipeId);
                throw new AppException("Líder não encontrado para a equipe informada.");
            }
            return lider;
        }

        public async Task<Dictionary<int, MembroEquipe>> ObterLideresDaEquipeAsync(List<int> equipeIds)
        {
            var lideres = await _membroRepo.GetLideresPorEquipesAsync(equipeIds);
            return lideres;
        }

        public async Task<List<MembroEquipe>> ObterMembrosPorUsuarioAsync(int usuarioId, int? equipeId = null)
        {
            if (usuarioId <= 0)
                throw new DomainException("UsuárioId inválido para consulta de membros.");

            if (equipeId.HasValue && equipeId.Value > 0)
            {
                var membro = await _membroRepo.GetMembroPorUsuario(usuarioId, equipeId.Value);

                if (membro == null)
                    return new List<MembroEquipe>();

                return new List<MembroEquipe> { membro };
            }

            return await _membroRepo
                .GetListByPredicateAsync<MembroEquipe>(m =>
                    m.UsuarioId == usuarioId &&
                    m.IsLider == false &&
                    !m.Excluido &&
                    m.DataSaida == null)
                ?? new List<MembroEquipe>();
        }

        public async Task<MembroEquipe?> GetByIdAsync(int membroId)
        {
            if (membroId <= 0)
                throw new DomainException("MembroId inválido.");

            var membro = await _membroRepo.GetByIdComStatusAsync(membroId);

            if (membro is null)
            {
                _logger.LogWarning("Membro de equipe {MembroId} não encontrado.", membroId);
                return null;
            }

            return membro;
        }

        public async Task<(List<MembroEquipe> Vendedores, bool FallbackAplicado, string? DetalhesFallback)> ObterVendedoresDisponiveisPorEquipeAsync(int equipeId, ConfiguracaoDistribuicao configuracao)
        {
            try
            {
                List<MembroEquipe> vendedoresPorEquipe = await ObterMembrosPorEquipe(equipeId, "ATIVO");
                if (vendedoresPorEquipe.Count == 0)
                {
                    _logger.LogError("Nenhum membro foi encontrado na equipe id {equipeId}", equipeId);
                    return (vendedoresPorEquipe, false, null);
                }

                // 1. Se não considerar horário, retornar todos os vendedores ativos
                if (!configuracao.ConsiderarHorarioTrabalho)
                {
                    return (vendedoresPorEquipe, false, null);
                }

                var dataAtual = TimeHelper.GetBrasiliaTime();
                var horaAtual = dataAtual.TimeOfDay;
                var diaSemanaNet = (int)dataAtual.DayOfWeek;

                // Converter do enum .NET (0=Domingo, 1=Segunda, etc.) para o padrão do banco (1=Domingo, 2=Segunda, etc.)
                var diaSemana = diaSemanaNet == 0 ? 1 : diaSemanaNet + 1;

                // 2. Verificar fim de semana ANTES de aplicar filtro de horário
                if (configuracao.ConsiderarFeriados)
                {
                    if (diaSemanaNet == 0 || diaSemanaNet == 6) // Domingo (0) ou Sábado (6)
                    {
                        // FALLBACK: Se for fim de semana e configurado para considerar, retorna todos os vendedores ativos
                        var detalhesFallbackFimSemana = $"Fallback aplicado: Fim de semana detectado (Dia: {diaSemanaNet}) e ConsiderarFeriados=True. Retornando todos os vendedores ativos da empresa.";

                        return (vendedoresPorEquipe, true, detalhesFallbackFimSemana);
                    }
                }

                List<int> vendedoresIds = vendedoresPorEquipe.Select(v => v.UsuarioId).ToList();
                var vendedoresNoHorario = await _membroRepo.ObterVendedoresPorEquipeDisponiveisNoHorarioAsync(diaSemana, horaAtual, vendedoresIds);

                // 4. Se encontrou vendedores no horário, retorna eles
                if (vendedoresNoHorario.Count != 0)
                {
                    return (vendedoresPorEquipe, false, null);
                }

                // 5. FALLBACK: Se não encontrou vendedores no horário, retorna todos os vendedores ativos
                var detalhesFallback = $"Fallback aplicado: Nenhum vendedor encontrado no horário atual (Dia: {diaSemana}, Hora: {horaAtual}). " +
                    "Retornando todos os vendedores ativos da empresa.";

                return (vendedoresPorEquipe, true, detalhesFallback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter vendedores disponíveis na equipe {EquipeId}", equipeId);
                throw;
            }
        }

        public async Task<bool> VerificarAssociacaoUsuarioEmpresaAsync(int usuarioId, int empresaId)
        {
            return await _membroRepo.VerificarAssociacaoEquipeAsync(usuarioId, empresaId);
        }

        public async Task<MembroEquipe> GetBotMembroEquipeAsync(int usuarioId, int empresaId)
        {
            if (usuarioId <= 0)
                throw new DomainException("UsuárioId inválido para consulta de membro.");
            if (empresaId <= 0)
                throw new DomainException("EmpresaId inválido para consulta de membro.");
            var membro = await _membroRepo.GetBotMembroEquipeAsync(usuarioId, empresaId);
            if (membro is null)
            {
                _logger.LogError("Membro de equipe não encontrado para UsuárioId {UsuarioId} e EmpresaId {EmpresaId}.", usuarioId, empresaId);
                throw new AppException("Membro de equipe não encontrado para o usuário e empresa informados.");
            }
            return membro;
        }

        /// <summary> Obtém membrosId de equipes onde o usuário é líder, filtrando apenas membros ativos e sem data de saída.</summary>
        public async Task<List<MembroEquipe>> ObterMembrosPorUsuarioIsLiderAsync(int usuarioId)
        {
            if (usuarioId <= 0)
                throw new DomainException("UsuárioId inválido para consulta de membros.");

            return await _membroRepo
                .GetListByPredicateAsync<MembroEquipe>(m =>
                    m.UsuarioId == usuarioId &&
                    m.IsLider == true &&
                    !m.Excluido &&
                    m.DataSaida == null)
                ?? new List<MembroEquipe>();
        }

        public Task<List<MembroEquipe>> ObterVendedoresAtivosPorEquipeIdsAsync(IReadOnlyList<int> equipeIds) =>
            _membroRepo.ObterVendedoresAtivosPorEquipeIdsAsync(equipeIds);
    }
}

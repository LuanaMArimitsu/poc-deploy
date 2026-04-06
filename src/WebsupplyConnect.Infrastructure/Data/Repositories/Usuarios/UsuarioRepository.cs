using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Usuarios;

/// <summary>
/// Construtor do reposit�rio
/// </summary>
/// <param name="dbContext">Contexto do banco de dados</param>
/// <param name="unitOfWork">Inst�ncia do UnitOfWork</param>

internal class UsuarioRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork, ILogger<UsuarioRepository> logger) : BaseRepository(dbContext, unitOfWork), IUsuarioRepository
{
    private readonly ILogger<UsuarioRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RemoverUsuarioEmpresaAsync(int usuarioId, int empresaId)
    {
        var vinculo = await _context.UsuarioEmpresa
            .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);

        if (vinculo != null)
        {
            _context.UsuarioEmpresa.Remove(vinculo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AtualizarUsuarioEmpresaAsync(UsuarioEmpresa usuarioEmpresa)
    {
        _context.UsuarioEmpresa.Update(usuarioEmpresa);
        await Task.CompletedTask;
    }

    public async Task<int> GetUsuarioResponsavelByIdAsync(int id, bool includeDeleted = false)
    {
        var usuarioID = await GetByIdAsync<Usuario>(id);
        return usuarioID!.Id;
    }

    public async Task<Usuario?> ObterNovoResponsavelAsync(int idUsuarioDesativado)
    {
        return await _context.Usuario
            .Where(u => u.Ativo && !u.Excluido && u.Id != idUsuarioDesativado)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Usuario?> BuscarUsuarioPorObjectIdAsync(string objectId)
    {
        return await _context.Usuario
            .Include(u => u.UsuarioEmpresas)
            .FirstOrDefaultAsync(u => u.ObjectId == objectId);
    }

    public async Task<Usuario?> BuscarUsuarioPorIdAsync(int id)
    {
        return await _context.Usuario
            .AsSplitQuery()
            .Include(u => u.UsuarioSuperior)
            .Include(u => u.Dispositivos)
            .Include(u => u.UsuarioEmpresas)
                .ThenInclude(ue => ue.CanalPadrao)
                .ThenInclude(c => c.Empresa)
            .Include(u => u.HorariosUsuario)
            .Include(u => u.Conversas)
                .ThenInclude(c => c.Lead)
            .Include(u => u.MembrosEquipe)
                .ThenInclude(m => m.LeadsSobResponsabilidade)
            .FirstOrDefaultAsync(u => u.Id == id && !u.Excluido);
    }


    public async Task<Domain.Entities.Usuario.Usuario?> GetEmpresaByUsuarioId(int usuarioId)
    {
        return await _context.Usuario
            .Include(u => u.UsuarioEmpresas)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
    }

    public async Task<bool> ExisteUsuarioComAzureIdAsync(string azureId)
    {
        return await _context.Usuario
            .AnyAsync(u => u.ObjectId == azureId);
    }

    public IQueryable<Usuario> ObterQueryUsuariosComEmpresa()
    {
        return _context.Usuario
            .Include(u => u.UsuarioEmpresas)
                .ThenInclude(ue => ue.Empresa)
            .Where(u => !u.Excluido);
    }

    public async Task<(List<Usuario> itens, int totalItens)> ObterUsuariosComFiltros(
     int? empresaId = null,
     string? nome = null,
     bool? ativo = null,
     int? tamanhoPagina = null,
     int? pagina = null)
    {
        var query = ObterQueryUsuariosComEmpresa();

        if (empresaId.HasValue)
            query = query.Where(u => u.UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId));

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(u => u.Nome.Contains(nome));

        if (ativo.HasValue)
            query = query.Where(u => u.Ativo == ativo.Value);

        query = query.Where(u => !u.Excluido);
        query = query.Where(u => !u.IsBot);

        var totalItens = await query.CountAsync();

        // Garante que a ordenação existe antes da paginação
        var queryOrdenada = query.OrderBy(u => u.Nome).AsQueryable();

        if (pagina.HasValue && tamanhoPagina.HasValue && pagina.Value > 0 && tamanhoPagina.Value > 0)
        {
            int paginaSeguro = pagina.Value;
            int tamanhoSeguro = tamanhoPagina.Value;

            queryOrdenada = queryOrdenada
                .Skip((paginaSeguro - 1) * tamanhoSeguro)
                .Take(tamanhoSeguro);
        }

        var itens = await queryOrdenada.ToListAsync();

        return (itens, totalItens);
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await CreateAsync(usuario);
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        Update(usuario);
        await Task.CompletedTask;
    }

    public async Task<Usuario?> GetUsuarioByIdWithConversasAsync(int id)
    {
        return await _context.Usuario
            .Include(u => u.Conversas)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetUsuarioByIdWithLeadsAsync(int id)
    {
        return await _context.Usuario
            .Include(u => u.MembrosEquipe)
                .ThenInclude(m => m.LeadsSobResponsabilidade)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<UsuarioEmpresa>?> ObterEmpresasPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.UsuarioEmpresa
            .Include(ue => ue.Empresa)
            .ThenInclude(e => e.GrupoEmpresa)
            .Include(ue => ue.CanalPadrao)
            .Include(ue => ue.EquipePadrao)
            .Where(ue => ue.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task AdicionarUsuarioEmpresaAsync(UsuarioEmpresa associacao)
    {
        await _context.UsuarioEmpresa.AddAsync(associacao);
    }

    public async Task RemoverAssociacoesPorUsuarioIdAsync(int usuarioId)
    {
        var associacoes = await _context.UsuarioEmpresa
            .Where(ue => ue.UsuarioId == usuarioId)
            .ToListAsync();

        if (associacoes.Any())
        {
            _context.UsuarioEmpresa.RemoveRange(associacoes);
        }
    }

    public async Task<Usuario?> ObterUsuarioPorMembroId(int membroId)
    {
        return await _context.Usuario
            .Include(u => u.MembrosEquipe)
            .FirstOrDefaultAsync(u => u.MembrosEquipe.Any(m => m.Id == membroId));
    }

    public void RemoverVinculoEmpresa(UsuarioEmpresa vinculo)
    {
        _context.UsuarioEmpresa.Remove(vinculo);
    }

    public async Task<List<Usuario>> ObterUsuariosComSubordinadosAsync()
    {
        return await _context.Usuario
            .Where(u => u.UsuarioSuperiorId == null && !u.Excluido && u.Ativo == true)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ObterUsuariosPorEmpresaAsync(int empresaId)
    {
        return await _context.Usuario
            .Include(u => u.UsuarioEmpresas)
            .Where(u => u.UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId) && !u.Excluido && u.Ativo == true && !u.IsBot)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task<Usuario?> ObterUsuarioPorIdAsync(int id)
    {
        return await _context.Usuario
            .Include(u => u.UsuarioEmpresas)
            .Include(u => u.HorariosUsuario)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Obtém todos os vendedores ativos de uma empresa
    /// </summary>
    public async Task<List<Usuario>> ObterVendedoresAtivosPorEmpresaAsync(int empresaId)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.UsuarioEmpresas)
            .Include(u => u.HorariosUsuario)
            .Where(u => u.Ativo &&
                      !u.Excluido &&
                      u.UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId))
            .ToListAsync();
    }

    /// <summary>
    /// Obtém vendedores disponíveis em um horário específico
    /// </summary>
    public async Task<List<Usuario>> ObterVendedoresDisponiveisNoHorarioAsync(int empresaId, int diaSemana, TimeSpan horaAtual)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.UsuarioEmpresas)
            .Include(u => u.HorariosUsuario)
            .Where(u => u.Ativo &&
                      !u.Excluido &&
                      u.UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId) &&
                      u.HorariosUsuario.Any(h =>
                          h.DiaSemanaId == diaSemana &&
                          h.HorarioInicio <= horaAtual &&
                          h.HorarioFim >= horaAtual))
            .ToListAsync();
    }
}

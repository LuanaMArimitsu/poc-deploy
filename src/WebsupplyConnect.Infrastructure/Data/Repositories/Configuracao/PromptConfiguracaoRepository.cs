using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Configuracao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Configuracao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Configuracao;

public class PromptConfiguracaoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IPromptConfiguracaoRepository
{
    public async Task<PromptConfiguracao?> ObterPorCodigoAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            return await _context.Set<PromptConfiguracao>()
                .FirstOrDefaultAsync(p => p.Codigo == codigo && !p.Excluido);
        }
        catch (Exception ex)
        {
            throw new InfraException($"Erro ao buscar PromptConfiguracao por código '{codigo}': {ex.Message}");
        }
    }

    public async Task<PromptConfiguracaoVersao?> ObterUltimaVersaoPublicadaAsync(int promptConfiguracaoId)
    {
        try
        {
            if (promptConfiguracaoId <= 0)
                return null;

            return await _context.Set<PromptConfiguracaoVersao>()
                .Where(v => v.PromptConfiguracaoId == promptConfiguracaoId
                         && v.Publicada
                         && !v.Excluido)
                .OrderByDescending(v => v.NumeroVersao)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new InfraException(
                $"Erro ao buscar última versão publicada de PromptConfiguracao '{promptConfiguracaoId}': {ex.Message}");
        }
    }

    public async Task<bool> EmpresaVinculadaAsync(int promptConfiguracaoId, int empresaId)
    {
        try
        {
            if (promptConfiguracaoId <= 0 || empresaId <= 0)
                return false;

            return await _context.Set<PromptConfiguracaoEmpresa>()
                .AnyAsync(e => e.PromptConfiguracaoId == promptConfiguracaoId
                            && e.EmpresaId == empresaId);
        }
        catch (Exception ex)
        {
            throw new InfraException(
                $"Erro ao verificar vínculo de empresa '{empresaId}' em PromptConfiguracao '{promptConfiguracaoId}': {ex.Message}");
        }
    }

    public async Task<PromptConfiguracao> CreateAsync(PromptConfiguracao prompt)
    {
        try
        {
            var entry = await _context.Set<PromptConfiguracao>().AddAsync(prompt);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            throw new InfraException($"Erro ao criar PromptConfiguracao: {ex.Message}");
        }
    }

    public async Task<PromptConfiguracaoVersao> CreateVersaoAsync(PromptConfiguracaoVersao versao)
    {
        try
        {
            var entry = await _context.Set<PromptConfiguracaoVersao>().AddAsync(versao);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            throw new InfraException($"Erro ao criar PromptConfiguracaoVersao: {ex.Message}");
        }
    }
}

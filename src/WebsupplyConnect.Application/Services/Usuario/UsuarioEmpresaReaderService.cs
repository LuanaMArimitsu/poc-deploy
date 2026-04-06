using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Application.Services.Usuario
{
    public class UsuarioEmpresaReaderService(ILogger<UsuarioEmpresaReaderService> logger, IUsuarioEmpresaRepository usuarioEmpresaRepository) : IUsuarioEmpresaReaderService
    {
        private readonly ILogger<UsuarioEmpresaReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));    
        private readonly IUsuarioEmpresaRepository _usuarioEmpresaRepository = usuarioEmpresaRepository ?? throw new ArgumentNullException(nameof(usuarioEmpresaRepository));


        public async Task<UsuarioEmpresa> GetCanalPadraoByUsuarioEmpresaAsync(int usuarioId, int empresaId)
        {
            try
            {
                var vinculo = await _usuarioEmpresaRepository.GetVinculoUsuarioEmpresaAsync(usuarioId, empresaId);
                return vinculo ?? throw new AppException($"Vínculo entre usuário {usuarioId} e empresa {empresaId} não encontrado.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter canal padrão por usuário e empresa. UsuárioId: {UsuarioId}, EmpresaId: {EmpresaId}", usuarioId, empresaId);
                throw;
            }
        }

        public async Task<List<UsuarioEmpresa>> GetVinculosPorUsuarioIdAsync(int usuarioId)
        {
            try
            {
                return await _usuarioEmpresaRepository.GetVinculosPorUsuarioIdAsync(usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter vínculos por usuário ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<UsuarioEmpresa?> GetBotByEmpresa(int empresaId)
        {
            try
            {
                var botVinculo = await _usuarioEmpresaRepository.GetBotVinculoByEmpresaAsync(empresaId);
                return botVinculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter vínculo do bot por empresa ID: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<UsuarioEmpresa?> GetUsuarioEmpresaByEmpresa(int empresaId, int usuarioLogado)
        {
            try
            {
                return await _usuarioEmpresaRepository.GetUsuarioEmpresaAsync(empresaId, usuarioLogado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter UsuarioEmpresa por empresa ID: {EmpresaId} e usuário ID: {UsuarioLogado}", empresaId, usuarioLogado);
                throw;
            }
        }

        public async Task<UsuarioEmpresa?> GetEquipePadraoByUsuarioEmpresaAsync(int usuarioId, int empresaId)
        {
            try
            {
                return await _usuarioEmpresaRepository.GetEquipePadraoVinculoAsync(usuarioId, empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter equipe padrão por usuário e empresa. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}", usuarioId, empresaId);
                throw;
            }
        }
    }
}

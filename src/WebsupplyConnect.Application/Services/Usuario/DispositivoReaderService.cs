using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Application.Services.Usuario
{
    public class DispositivoReaderService(ILogger<DispositivoReaderService> logger, IDispositivosRepository dispositivosRepository, IUsuarioReaderService usuarioReaderService) : IDispositivosReaderService
    {
        private readonly ILogger<DispositivoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IDispositivosRepository _dispositivosRepository = dispositivosRepository ?? throw new ArgumentNullException(nameof(dispositivosRepository));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));

        /// <summary>
        /// Método para buscar dispositivos do usuario
        /// </summary>
        public async Task<List<Dispositivo>> GetDispositivosByUserAsync(int usuarioId)
        {
            try
            {
                var uservalido = await _usuarioReaderService.UserExistsAsync(usuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }
                return await _dispositivosRepository.DispositivosUserAsync(usuarioId);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dispositivos do usuário {UsuarioId}", usuarioId);
                throw new AppException("Erro interno ao buscar dispositivos do usuário", ex);
            }
        }

        public async Task<PagedResponseDTO<DispositivoListagemDTO>> ListarDispositivosPaginadoAsync(DispositivoFiltroRequestDTO filtro)
        {
            var query = _dispositivosRepository.ObterQueryComUsuario();

            query = AplicarFiltros(query, filtro);
            query = AplicarOrdenacao(query, filtro);

            var dispositivos = await query.ToListAsync();

            // Filtro pós-query para status "Online" e "Offline"
            if (!string.IsNullOrWhiteSpace(filtro.Status) && filtro.Status != "Todos")
            {
                if (filtro.Status == "Online")
                    dispositivos = dispositivos.Where(d => d.EstaConectadoViaSignalR()).ToList();
                else if (filtro.Status == "Offline")
                    dispositivos = dispositivos.Where(d => !d.EstaConectadoViaSignalR()).ToList();
            }

            var totalItens = dispositivos.Count;

            var itensPaginados = dispositivos
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .Select(d => new DispositivoListagemDTO
                {
                    Id = d.Id,
                    DeviceId = d.DeviceId,
                    Modelo = d.Modelo,
                    UsuarioId = d.Usuario.Id,
                    UsuarioNome = d.Usuario.Nome,
                    UsuarioEmail = d.Usuario.Email,
                    Ativo = d.Ativo,
                    Online = d.EstaConectadoViaSignalR(),
                    UltimaSincronizacao = d.UltimaSincronizacao,
                    SignalRConnectionId = d.SignalRConnectionId,
                    UltimoHeartbeatSignalR = d.UltimoHeartbeatSignalR,
                    DataCriacao = d.DataCriacao
                })
                .ToList();

            return new PagedResponseDTO<DispositivoListagemDTO>
            {
                Itens = itensPaginados,
                PaginaAtual = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = (int)Math.Ceiling((double)totalItens / filtro.TamanhoPagina)
            };
        }

        private static IQueryable<Dispositivo> AplicarFiltros(IQueryable<Dispositivo> query, DispositivoFiltroRequestDTO filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var termo = filtro.Busca.ToLower();
                query = query.Where(d =>
                    d.DeviceId.ToLower().Contains(termo) ||
                    d.Modelo.ToLower().Contains(termo));
            }

            if (filtro.UsuarioId.HasValue)
            {
                query = query.Where(d => d.UsuarioId == filtro.UsuarioId.Value);
            }

            // Removido filtro por Online/Offline aqui (aplicado no método principal)

            if (filtro.SincronizadoApos.HasValue)
            {
                query = query.Where(d => d.UltimaSincronizacao >= filtro.SincronizadoApos.Value);
            }

            if (filtro.SincronizadoAntes.HasValue)
            {
                query = query.Where(d => d.UltimaSincronizacao <= filtro.SincronizadoAntes.Value);
            }

            return query;
        }

        private static IQueryable<Dispositivo> AplicarOrdenacao(IQueryable<Dispositivo> query, DispositivoFiltroRequestDTO filtro)
        {
            bool asc = filtro.DirecaoOrdenacao.ToUpper() == "ASC";

            return filtro.OrdenarPor switch
            {
                "DeviceId" => asc ? query.OrderBy(d => d.DeviceId) : query.OrderByDescending(d => d.DeviceId),
                "Modelo" => asc ? query.OrderBy(d => d.Modelo) : query.OrderByDescending(d => d.Modelo),
                "UltimaSincronizacao" => asc ? query.OrderBy(d => d.UltimaSincronizacao) : query.OrderByDescending(d => d.UltimaSincronizacao),
                "DataCriacao" => asc ? query.OrderBy(d => d.DataCriacao) : query.OrderByDescending(d => d.DataCriacao),
                _ => asc ? query.OrderBy(d => d.Id) : query.OrderByDescending(d => d.Id),
            };
        }

        public async Task<bool> UsuarioPossuiDispositivoAsync(int usuarioId, string deviceId)
        {
            var dispositivo = await _dispositivosRepository.ObterPorDeviceIdAsync(usuarioId, deviceId);
            return dispositivo != null;
        }

        public async Task<DispositivoDetalheDTO?> ObterDispositivoDetalhadoAsync(string deviceId)
        {
            var dispositivo = await _dispositivosRepository.ObterDetalhadoPorDeviceIdAsync(deviceId);
            if (dispositivo == null)
                return null;

            var empresaPrincipal = dispositivo.Usuario?.UsuarioEmpresas
                .FirstOrDefault(ue => ue.IsPrincipal)?.Empresa?.Nome ?? string.Empty;

            return new DispositivoDetalheDTO
            {
                Id = dispositivo.Id,
                DeviceId = dispositivo.DeviceId,
                Modelo = dispositivo.Modelo,
                Ativo = dispositivo.Ativo,
                Online = dispositivo.EstaConectadoViaSignalR(),
                UltimaSincronizacao = dispositivo.UltimaSincronizacao,
                SignalRConnectionId = dispositivo.SignalRConnectionId,
                UltimoHeartbeatSignalR = dispositivo.UltimoHeartbeatSignalR,
                UltimaReconexao = dispositivo.UltimaReconexao,
                DataCriacao = dispositivo.DataCriacao,
                DataModificacao = dispositivo.DataModificacao,
                Usuario = new DispositivoUsuarioDTO
                {
                    Id = dispositivo.Usuario.Id,
                    Nome = dispositivo.Usuario.Nome,
                    Email = dispositivo.Usuario.Email,
                    Cargo = dispositivo.Usuario.Cargo ?? string.Empty,
                    Departamento = dispositivo.Usuario.Departamento ?? string.Empty,
                    EmpresaPrincipal = empresaPrincipal
                }
            };
        }

        public async Task<DispositivoAcessoDTO> VerificarDispositivoStatusAsync(int dispositivoId)
        {
            try
            {
                var dispositivo = await _dispositivosRepository.GetByIdAsync<Dispositivo>(dispositivoId);

                if (dispositivo == null)
                {
                    return new DispositivoAcessoDTO(true, false);
                }

                var usuario = await _usuarioReaderService.ObterUsuarioPorIdAsync(dispositivo.UsuarioId) ?? throw new AppException($"Usuário com id: {dispositivo.UsuarioId}");
                return new DispositivoAcessoDTO
                (
                    UsuarioAtivo: usuario.Ativo,
                    DispositivoAtivo: dispositivo.Ativo
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar dispositivo ativo com ID {DispositivoId}", dispositivoId);
                throw;

            }
        }

    }
}

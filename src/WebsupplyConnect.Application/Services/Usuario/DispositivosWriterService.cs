using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Application.Services.Usuario
{
    public class DispositivosWriterService(
        IUnitOfWork unitOfWork,
        ILogger<DispositivosWriterService> logger,
        IUsuarioReaderService usuarioReaderService,
        IMailSenderService mailSenderService,
        IValidator<AdicionarDispositivoDTO> adcValidator,
        ISignalRConnection connectionManager,
        IDispositivosRepository dispositivosRepository
    ) : IDispositivosWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
        private readonly IMailSenderService _mailSenderService = mailSenderService ?? throw new ArgumentNullException(nameof(mailSenderService));
        private readonly ILogger<DispositivosWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISignalRConnection _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        private readonly IDispositivosRepository _dispositivosRepository = dispositivosRepository ?? throw new ArgumentNullException(nameof(dispositivosRepository));
        private readonly IValidator<AdicionarDispositivoDTO> _adcValidator = adcValidator ?? throw new ArgumentNullException(nameof(adcValidator));

        public async Task Create(AdicionarDispositivoDTO dto)
        {
            try
            {
                var validationResult = await _adcValidator.ValidateAsync(dto);

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var dispositivo = new Dispositivo(dto.UsuarioId, dto.DeviceId, dto.Modelo);

                await _dispositivosRepository.CreateAsync(dispositivo);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar dispositivo");
                throw new AppException("Erro interno ao adicionar dispositivo", ex);
            }
        }

        public async Task AlterarStatusDispositivoAsync(int id, AlterarStatusDispositivoRequestDTO dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var dispositivo = await _dispositivosRepository.ObterPorIdAsync(id);
                if (dispositivo == null)
                    throw new AppException("Dispositivo não encontrado.");

                if (dispositivo.Ativo == dto.Ativo)
                    return;

                if (dto.Ativo)
                {
                    dispositivo.Ativar();
                }
                else
                {
                    dispositivo.Desativar();
                    await LimparConexaoSignalRAsync(dispositivo);
                }

                await _dispositivosRepository.AtualizarAsync(dispositivo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                if (!dto.Ativo && dto.NotificarUsuario)
                {
                    var usuario = dispositivo.Usuario;
                    if (usuario != null && !string.IsNullOrWhiteSpace(usuario.Email))
                    {
                        var assunto = "Seu dispositivo foi bloqueado!";
                        var conteudo = $"""
                        Olá {usuario.Nome},<br><br>
                        Informamos que o seu dispositivo <strong>"{dispositivo.Modelo}"</strong> foi bloqueado.<br><br>
                        <strong>Motivo:</strong> {dto.Motivo ?? "Não Informado"}<br><br>
                        Caso tenha dúvidas ou precise de suporte, entre em contato com nossa equipe.<br><br>
                        Atenciosamente,<br>
                        Websupply Connect.
                        """;

                        await _mailSenderService.EnviarAsync(usuario.Email, usuario.Nome, assunto, conteudo);
                    }
                }
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao alterar status do dispositivo com ID {DispositivoId}", id);
                throw new AppException("Erro interno ao alterar status do dispositivo.", ex);
            }
        }

        public async Task<bool> LimparConexaoSignalRAsync(Dispositivo dispositivo)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(dispositivo.SignalRConnectionId))
                {
                    _connectionManager.RemoveConnection(dispositivo.UsuarioId, dispositivo.SignalRConnectionId);
                }

                dispositivo.LimparConexaoSignalR();

                await _dispositivosRepository.AtualizarAsync(dispositivo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Conexão SignalR removida do dispositivo {DeviceId} do usuário {UsuarioId}", dispositivo.DeviceId, dispositivo.UsuarioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar conexão SignalR do dispositivo {DeviceId}", dispositivo.DeviceId);
                throw new AppException("Erro interno ao limpar conexão SignalR.", ex);
            }
        }

        public async Task<bool> AtualizarConexaoSignalRAsync(string deviceId, int usuarioId, string connectionId)
        {
            var dispositivo = await _dispositivosRepository.ObterPorDeviceIdAsync(usuarioId, deviceId);
            if (dispositivo == null)
                throw new AppException("Dispositivo não encontrado.");

            dispositivo.AtualizarSignalRConnectionId(connectionId);
            dispositivo.RegistrarReconexao();

            await _dispositivosRepository.AtualizarAsync(dispositivo);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("SignalR connection atualizada para o dispositivo {DeviceId} do usuário {UsuarioId}", deviceId, usuarioId);

            return true;
        }

        public async Task<bool> LimparConexaosignalRAsync(string deviceId, int usuarioId)
        {
            try
            {
                var dispositivo = await _dispositivosRepository.ObterPorDeviceIdAsync(usuarioId, deviceId);
                if (dispositivo == null)
                    return false;

                if (!string.IsNullOrWhiteSpace(dispositivo.SignalRConnectionId))
                {
                    _connectionManager.RemoveConnection(dispositivo.UsuarioId, dispositivo.SignalRConnectionId);
                }

                dispositivo.LimparConexaoSignalR();

                await _dispositivosRepository.AtualizarAsync(dispositivo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Conexão SignalR removida do dispositivo {DeviceId} do usuário {UsuarioId}", deviceId, usuarioId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar conexão SignalR do dispositivo {DeviceId}", deviceId);
                throw new AppException("Erro interno ao limpar conexão SignalR.", ex);
            }
        }

        public async Task<bool> RegistrarHeartbeatAsync(string deviceId, int usuarioLogadoId)
        {
            var dispositivo = await _dispositivosRepository.ObterPorDeviceIdAsync(usuarioLogadoId, deviceId);
            if (dispositivo == null)
                return false;

            if (!dispositivo.Ativo)
                throw new AppException("Não é possível registrar heartbeat em um dispositivo inativo.");

            dispositivo.RegistrarHeartbeatSignalR();

            await _dispositivosRepository.AtualizarAsync(dispositivo);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Heartbeat registrado para o dispositivo {DeviceId} do usuário {UsuarioId}", deviceId, usuarioLogadoId);

            return true;
        }

        public async Task<SincronizacaoDispositivoDTO> RegistrarSincronizacaoAsync(string deviceId, int usuarioLogadoId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var dispositivo = await _dispositivosRepository.ObterPorDeviceIdAsync(usuarioLogadoId, deviceId);
                if (dispositivo is null)
                    throw new DomainException("Dispositivo não encontrado.");

                if (!dispositivo.Ativo)
                    throw new DomainException("Não é possível registrar sincronização em um dispositivo inativo.");

                dispositivo.RegistrarSincronizacao();

                await _dispositivosRepository.AtualizarAsync(dispositivo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new SincronizacaoDispositivoDTO
                {
                    DispositivoId = dispositivo.Id,
                    UltimaSincronizacao = dispositivo.UltimaSincronizacao
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
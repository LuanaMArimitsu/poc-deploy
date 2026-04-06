using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Application.Services.Equipe;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Application.Services.Usuario
{
    internal class UsuarioWriterService(
        IUnitOfWork unitOfWork,
        IUsuarioRepository usuarioRepository,
        IAzureAdService azureAdService,
        ILogger<UsuarioWriterService> logger,
        IEmpresaReaderService empresaReaderService,
        IValidator<AtualizarVinculosRequestDTO> vinculosValidator,
        IValidator<AtualizarUsuarioRequestDTO> atualizarUsuarioValidator,
        IHorariosRepository horariosRepository,
        IRedisCacheService redisCacheService,
        ICanalReaderService canalReaderService,
        IConversaReaderService conversaReaderService,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService,
        IMembroEquipeReaderService membroEquipeReaderService,
        IEquipeReaderService equipeReaderService) : IUsuarioWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
        private readonly IAzureAdService _azureAdService = azureAdService;
        private readonly ILogger<UsuarioWriterService> _logger = logger;
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService;
        private readonly IValidator<AtualizarVinculosRequestDTO> _vinculosValidator = vinculosValidator;
        private readonly IHorariosRepository _horariosRepository = horariosRepository;
        private readonly IRedisCacheService _redisCacheService = redisCacheService;
        private readonly IValidator<AtualizarUsuarioRequestDTO> _atualizarUsuarioValidator = atualizarUsuarioValidator;
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService;
        private readonly ICanalReaderService _canalReaderService = canalReaderService;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService;
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService;
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService;

        public async Task<Domain.Entities.Usuario.Usuario> IncluirUsuarioDoAzureAsync(string azureUserId, int? usuarioSuperiorId, int empresaId, int equipePadraoId, int canalPadraoId, string cargo, string departamento, string? codVendedorNBS = null)
        {
            var azUser = await _azureAdService.GetUserByIdAsync(azureUserId);
            if (azUser == null)
                throw new Exception("Usuário não encontrado no Azure AD.");

            var existe = await _usuarioRepository.ExisteUsuarioComAzureIdAsync(azUser.Id);
            if (existe)
                throw new Exception("usuário Já Cadastrado.");

            var empresaExiste = await _empresaReaderService.EmpresaExistsAsync(empresaId);
            if (!empresaExiste)
                throw new DomainException("A empresa informada não existe.");

            var canal = await _canalReaderService.GetCanalByIdAsync(canalPadraoId);
            if (canal is null)
                throw new DomainException("O canal informado não existe.");

            if (canal.EmpresaId != empresaId)
                throw new DomainException("O canal informado não pertence à empresa informada.");

            var equipesEmpresa = await _equipeReaderService.GetEquipesByEmpresaId(empresaId);
            var equipe = equipesEmpresa.FirstOrDefault(e => e.Id == equipePadraoId)
                ?? throw new DomainException("A equipe informada não existe ou não pertence à empresa informada.");

            var novoUsuario = new WebsupplyConnect.Domain.Entities.Usuario.Usuario(
                nome: azUser.DisplayName,
                email: string.IsNullOrEmpty(azUser.Email) ? azUser.Upn : azUser.Email,
                cargo: string.IsNullOrWhiteSpace(cargo) ? azUser.Cargo : cargo,
                departamento: string.IsNullOrWhiteSpace(departamento) ? azUser.Departamento : departamento,
                objectId: azUser.Id,
                upn: azUser.Upn,
                displayName: azUser.DisplayName,
                usuarioSuperiorId: usuarioSuperiorId
            );

            try
            {
                await _usuarioRepository.AdicionarAsync(novoUsuario);
                await _unitOfWork.SaveChangesAsync();

                var vinculo = new UsuarioEmpresa(
                    usuarioId: novoUsuario.Id,
                    empresaId: empresaId,
                    canalPadraoId: canalPadraoId,
                    equipePadraoId: equipePadraoId,
                    codVendedorNBS: codVendedorNBS,
                    isPrincipal: true
                );

                await _usuarioRepository.AdicionarUsuarioEmpresaAsync(vinculo);

                await _unitOfWork.CommitAsync();
                return novoUsuario;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AtualizarUsuarioAsync(int id, AtualizarUsuarioRequestDTO request, int usuarioLogadoId)
        {

            if (request.UsuarioSuperiorId.HasValue && request.UsuarioSuperiorId.Value == id)
                throw new AppException("Um usuário não pode ser superior de si mesmo.");

            var validationResult = await _atualizarUsuarioValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errorMessages = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Erro de validação ao atualizar usuário {UsuarioId}: {Erros}", id, errorMessages);
                throw new AppException($"Erro de validação: {errorMessages}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(id);
            if (usuario == null)
            {
                return false;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cargo = request.Cargo ?? usuario.Cargo ?? string.Empty;
                var departamento = request.Departamento ?? usuario.Departamento ?? string.Empty;

                usuario.AtualizarInformacoes(
                    usuario.Nome,
                    cargo,
                    departamento,
                    usuario.DisplayName
                );

                if (request.UsuarioSuperiorId != usuario.UsuarioSuperiorId)
                {
                    usuario.AlterarSuperior(request.UsuarioSuperiorId);
                }

                if (request.Ativo != usuario.Ativo)
                {
                    var status = request.Ativo ? "Ativado" : "Desativado";
                    _logger.LogInformation("Alterando status do usuário {UsuarioId}: {Status}", id, status);

                    if (request.Ativo)
                        usuario.Ativar();
                    else
                        usuario.Desativar();
                }

                await _usuarioRepository.AtualizarAsync(usuario);
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar usuário {UsuarioId}", id);
                throw;
            }
        }

        public async Task AlterarStatusUsuarioAsync(int usuarioId, bool novoStatus, int usuarioLogadoId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new AppException("Usuário não encontrado.");

            if (usuario.Id == usuarioLogadoId)
                throw new AppException("Você não pode alterar seu próprio status.");

            bool desativando = usuario.Ativo && !novoStatus;

            if (novoStatus)
                usuario.Ativar();
            else
                usuario.Desativar();

            _logger.LogInformation("Usuário {UsuarioId} teve o status alterado para {Status} por {UsuarioLogadoId}",
                usuarioId, novoStatus ? "Ativo" : "Inativo", usuarioLogadoId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _usuarioRepository.AtualizarAsync(usuario);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao alterar status do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<bool> AssociarEmpresaAoUsuarioAsync(int usuarioId, int empresaId, int canalPadraoId, int equipePadraoId, string? metadata = null)
        {
            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario == null)
                throw new Exception("Usuário não encontrado");

            var empresaExiste = await _empresaReaderService.EmpresaExistsAsync(empresaId);
            if (!empresaExiste)
                throw new Exception("Empresa não encontrada");

            var canal = await _canalReaderService.GetCanalByIdAsync(canalPadraoId);
            if (canal == null)
                throw new Exception("Canal padrão não encontrado");

            if (canal.EmpresaId != empresaId)
                throw new DomainException("O canal informado não pertence à empresa informada.");

            var equipesEmpresa = await _equipeReaderService.GetEquipesByEmpresaId(empresaId);
            var equipe = equipesEmpresa.FirstOrDefault(e => e.Id == equipePadraoId)
                ?? throw new DomainException("A equipe informada não existe ou não pertence à empresa informada.");

            var empresasAssociadas = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);
            if (empresasAssociadas.Any(e => e.EmpresaId == empresaId))
                return false;

            bool definirComoPrincipal = !empresasAssociadas.Any();

            var novaAssociacao = new UsuarioEmpresa(usuarioId, empresaId, canalPadraoId, equipePadraoId, metadata, definirComoPrincipal);

            await _usuarioRepository.AdicionarUsuarioEmpresaAsync(novaAssociacao);
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task AtualizarVinculosEmpresasDoUsuario(int usuarioId, List<EmpresaVinculoDTO> empresasVinculos)
        {
            var validationResult = await _vinculosValidator.ValidateAsync(new AtualizarVinculosRequestDTO
            {
                EmpresasVinculos = empresasVinculos
            });

            if (!validationResult.IsValid)
            {
                var erros = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new AppException($"Dados inválidos: {erros}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId)
                ?? throw new AppException("Usuário não encontrado");

            var empresasValidas = await _empresaReaderService.ExistemEmpresasAtivasAsync(empresasVinculos.Select(e => e.EmpresaId).ToList());
            if (!empresasValidas)
                throw new AppException("Uma ou mais empresas não existem ou estão inativas");

            var canaisInformadosIds = empresasVinculos.Select(v => v.CanalPadraoId).Distinct().ToList();
            var canaisInformados = await _canalReaderService.GetListCanaisById(canaisInformadosIds);

            if (canaisInformados.Count != canaisInformadosIds.Count)
                throw new AppException("Um ou mais canais informados são inválidos.");

            var empresasIds = empresasVinculos.Select(v => v.EmpresaId).Distinct().ToList();
            var equipesEmpresa = await _equipeReaderService.GetListEquipesByEmpresasIds(empresasIds);

            foreach (var vinculo in empresasVinculos)
            {
                var canal = canaisInformados.FirstOrDefault(c => c.Id == vinculo.CanalPadraoId)
                    ?? throw new AppException($"Canal com ID {vinculo.CanalPadraoId} não encontrado.");

                if (canal.EmpresaId != vinculo.EmpresaId)
                    throw new DomainException($"O canal '{canal.Nome}' (ID {canal.Id}) não pertence à empresa ID {vinculo.EmpresaId}.");

                var equipe = equipesEmpresa.FirstOrDefault(e => e.Id == vinculo.EquipePadraoId && e.EmpresaId == vinculo.EmpresaId)
                    ?? throw new DomainException($"A equipe ID {vinculo.EquipePadraoId} não existe ou não pertence à empresa ID {vinculo.EmpresaId}.");
            }

            var vinculosAtuais = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);

            var empresasRemovidas = vinculosAtuais!
                .Where(v => !empresasVinculos.Any(ev => ev.EmpresaId == v.EmpresaId))
                .ToList();

            // Validar remoções antes de abrir transação
            foreach (var empresaRemovida in empresasRemovidas)
            {
                bool possuiConversas = await _conversaReaderService.ExisteConversaNoCanalAsync(usuarioId, empresaRemovida.CanalPadraoId);
                if (possuiConversas)
                    throw new AppException(
                        $"Não é possível remover a empresa {empresaRemovida.Empresa.Nome}, pois o usuário possui conversas ativas no canal padrão {empresaRemovida.CanalPadraoId}."
                    );
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remover vínculos não mais existentes
                foreach (var empresaRemovida in empresasRemovidas)
                {
                    await _usuarioRepository.RemoverUsuarioEmpresaAsync(usuarioId, empresaRemovida.EmpresaId);
                    vinculosAtuais!.Remove(empresaRemovida);
                }

                var vinculosPorEmpresa = vinculosAtuais!.ToDictionary(v => v.EmpresaId);

                foreach (var vinculo in empresasVinculos)
                {
                    var ehPrincipal = vinculo.EhPrincipal ?? false;

                    if (!vinculosPorEmpresa.TryGetValue(vinculo.EmpresaId, out var vinculoExistente))
                    {
                        var novaAssociacao = new UsuarioEmpresa(
                            usuarioId,
                            vinculo.EmpresaId,
                            vinculo.CanalPadraoId,
                            vinculo.EquipePadraoId,
                            vinculo.CodVendedorNBS,
                            ehPrincipal);

                        await _usuarioRepository.AdicionarUsuarioEmpresaAsync(novaAssociacao);
                        continue;
                    }

                    var alterou = false;

                    if (vinculoExistente.CanalPadraoId != vinculo.CanalPadraoId)
                    {
                        vinculoExistente.AtualizarCanalPadrao(vinculo.CanalPadraoId);
                        alterou = true;
                    }

                    if (vinculoExistente.IsPrincipal != ehPrincipal)
                    {
                        vinculoExistente.DefinirComoPrincipal(ehPrincipal);
                        alterou = true;
                    }

                    if (vinculoExistente.CodVendedorNBS != vinculo.CodVendedorNBS)
                    {
                        vinculoExistente.AtualizarCodVendedorNBS(vinculo.CodVendedorNBS);
                        alterou = true;
                    }

                    if (vinculoExistente.EquipePadraoId != vinculo.EquipePadraoId)
                    {
                        vinculoExistente.AtualizarEquipePadrao(vinculo.EquipePadraoId);
                        alterou = true;
                    }

                    if (alterou)
                    {
                        await _usuarioRepository.AtualizarUsuarioEmpresaAsync(vinculoExistente);
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


        public async Task AssociarMultiplasEmpresasAoUsuarioAsync(int usuarioId, AtualizarVinculosRequestDTO request)
        {
            var validationResult = await _vinculosValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var erros = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new AppException($"Dados inválidos: {erros}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new AppException("Usuário não encontrado");

            var empresasIds = request.EmpresasVinculos.Select(v => v.EmpresaId).ToList();

            foreach (var id in empresasIds)
            {
                if (!await _empresaReaderService.EmpresaExistsAsync(id))
                    throw new AppException($"Empresa com ID {id} não encontrada");
            }

            var canalIds = request.EmpresasVinculos.Select(v => v.CanalPadraoId).Distinct().ToList();
            var canais = await _canalReaderService.GetListCanaisById(canalIds);

            if (canais.Count != canalIds.Count)
                throw new AppException("Um ou mais canais informados são inválidos.");

            var equipesEmpresa = await _equipeReaderService.GetListEquipesByEmpresasIds(empresasIds);

            foreach (var vinculo in request.EmpresasVinculos)
            {
                var canal = canais.FirstOrDefault(c => c.Id == vinculo.CanalPadraoId);
                if (canal == null)
                    throw new AppException($"Canal com ID {vinculo.CanalPadraoId} não encontrado.");

                if (canal.EmpresaId != vinculo.EmpresaId)
                    throw new DomainException($"O canal '{canal.Nome}' (ID {canal.Id}) não pertence à empresa ID {vinculo.EmpresaId}.");

                var equipe = equipesEmpresa.FirstOrDefault(e => e.Id == vinculo.EquipePadraoId && e.EmpresaId == vinculo.EmpresaId)
                    ?? throw new DomainException($"A equipe ID {vinculo.EquipePadraoId} não existe ou não pertence à empresa ID {vinculo.EmpresaId}.");
            }

            await _usuarioRepository.RemoverAssociacoesPorUsuarioIdAsync(usuarioId);

            foreach (var vinculo in request.EmpresasVinculos)
            {
                var novaAssociacao = new UsuarioEmpresa(usuarioId, vinculo.EmpresaId, vinculo.CanalPadraoId, vinculo.EquipePadraoId, vinculo.CodVendedorNBS, vinculo.EhPrincipal ?? false);
                await _usuarioRepository.AdicionarUsuarioEmpresaAsync(novaAssociacao);
            }

            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.CommitAsync();
        }

        public async Task<bool?> DesassociarEmpresaAsync(int usuarioId, int empresaId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
                if (usuario == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                var possuiEquipes = await _membroEquipeReaderService.VerificarAssociacaoUsuarioEmpresaAsync(usuarioId, empresaId);
                if (possuiEquipes)
                {
                    await _unitOfWork.RollbackAsync();
                    throw new AppException("Não é possível desvincular o usuário da empresa pois ele faz parte de uma equipe.");
                }

                var vinculos = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);
                var empresaParaRemover = vinculos.FirstOrDefault(e => e.EmpresaId == empresaId);

                if (empresaParaRemover == null)
                {
                    await _unitOfWork.CommitAsync();
                    return false;
                }

                if (empresaParaRemover.IsPrincipal)
                {
                    var outraEmpresa = vinculos.FirstOrDefault(e => e.EmpresaId != empresaId);
                    if (outraEmpresa == null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return false;
                    }

                    await _unitOfWork.RollbackAsync();
                    return false;
                }

                _usuarioRepository.RemoverVinculoEmpresa(empresaParaRemover);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DefinirEmpresaPrincipalAsync(int usuarioId, int empresaId)
        {
            var empresasVinculadas = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);
            if (empresasVinculadas == null || !empresasVinculadas.Any())
                throw new KeyNotFoundException("Usuário não encontrado ou sem empresas vinculadas.");

            var empresaVinculo = empresasVinculadas.FirstOrDefault(e => e.EmpresaId == empresaId);
            if (empresaVinculo == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var vinculo in empresasVinculadas)
                    vinculo.IsPrincipal = vinculo.EmpresaId == empresaId;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<object> AlternarAssociacaoEmpresaAsync(int usuarioId, int empresaId, bool? definirComoPrincipal, int? canalPadraoId = null, int? equipePadraoId = null, string? metadata = null)
        {
            var empresasVinculadas = await _usuarioRepository.ObterEmpresasPorUsuarioIdAsync(usuarioId);
            if (empresasVinculadas == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var associacaoExistente = empresasVinculadas.FirstOrDefault(e => e.EmpresaId == empresaId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (associacaoExistente != null)
                {
                    if (definirComoPrincipal == true && !associacaoExistente.IsPrincipal)
                    {
                        // Tornar esta como principal e remover principalidade das outras
                        foreach (var vinculo in empresasVinculadas)
                        {
                            if (vinculo.IsPrincipal)
                                vinculo.RemoverComoPrincipal();
                        }

                        associacaoExistente.DefinirComoPrincipal();

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitAsync();

                        return new { associada = true, isPrincipal = true };
                    }

                    if (definirComoPrincipal == false)
                    {
                        // Se for a empresa principal, é necessário transferir antes
                        if (associacaoExistente.IsPrincipal)
                        {
                            var outraEmpresa = empresasVinculadas.FirstOrDefault(e => e.EmpresaId != empresaId);
                            if (outraEmpresa == null)
                                throw new InvalidOperationException("Não é possível remover a empresa principal sem definir outra como principal.");

                            outraEmpresa.DefinirComoPrincipal();
                        }

                        _usuarioRepository.RemoverVinculoEmpresa(associacaoExistente);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitAsync();

                        return new { associada = false, isPrincipal = false };
                    }

                    await _unitOfWork.RollbackAsync();
                    return new { associada = true, isPrincipal = associacaoExistente.IsPrincipal };
                }

                if (canalPadraoId is null || canalPadraoId <= 0)
                    throw new AppException("O canal padrão deve ser informado para nova associação.");

                var empresaExiste = await _empresaReaderService.EmpresaExistsAsync(empresaId);
                if (!empresaExiste)
                    throw new KeyNotFoundException("Empresa não encontrada.");

                var canal = await _canalReaderService.GetCanalByIdAsync(canalPadraoId.Value);
                if (canal == null)
                    throw new AppException("Canal padrão não encontrado.");

                if (canal.EmpresaId != empresaId)
                    throw new DomainException($"O canal '{canal.Nome}' (ID {canal.Id}) não pertence à empresa ID {empresaId}.");

                if (equipePadraoId is null || equipePadraoId <= 0)
                    throw new AppException("A equipe padrão deve ser informada para nova associação.");

                var equipesEmpresa = await _equipeReaderService.GetEquipesByEmpresaId(empresaId);
                var equipe = equipesEmpresa.FirstOrDefault(e => e.Id == equipePadraoId)
                    ?? throw new DomainException("A equipe informada não existe ou não pertence à empresa informada.");


                var novaAssociacao = new UsuarioEmpresa(usuarioId, empresaId, canalPadraoId.Value, equipePadraoId.Value, metadata, definirComoPrincipal == true);

                if (!empresasVinculadas.Any())
                    novaAssociacao.DefinirComoPrincipal();

                if (novaAssociacao.IsPrincipal)
                {
                    foreach (var vinculo in empresasVinculadas)
                        vinculo.RemoverComoPrincipal();
                }

                await _usuarioRepository.AdicionarUsuarioEmpresaAsync(novaAssociacao);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new { associada = true, isPrincipal = novaAssociacao.IsPrincipal };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<UsuarioHorarioDTO>> ConfigurarHorariosAsync(int usuarioId, List<HorarioTrabalhoDTO> horarios)
        {
            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new DomainException("Usuário não encontrado.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _horariosRepository.RemoverHorariosPorUsuarioAsync(usuarioId);

                foreach (var horario in horarios)
                {
                    UsuarioHorario novoHorario;

                    if (horario.SemExpediente)
                    {
                        novoHorario = new UsuarioHorario(usuarioId, horario.DiaSemanaId);
                    }
                    else
                    {
                        novoHorario = new UsuarioHorario(
                            usuarioId,
                            horario.DiaSemanaId,
                            horario.HorarioInicio!.Value,
                            horario.HorarioFim!.Value
                        );
                    }

                    novoHorario.DefinirTolerancia(false);
                    await _horariosRepository.AdicionarAsync(novoHorario);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var cacheKey = $"usuario:{usuarioId}:horarios";

            var usuarioAtualizado = await _usuarioRepository.ObterUsuarioPorIdAsync(usuarioId);
            var horariosAtualizados = MontarHorariosTrabalhoCompletos(usuarioAtualizado!.HorariosUsuario);

            await _redisCacheService.SetAsync(cacheKey, horariosAtualizados, TimeSpan.FromDays(1));

            return horariosAtualizados;
        }

        public async Task AtualizarHorarioDiaAsync(int usuarioId, int diaSemanaId, AtualizarHorarioTrabalhoDTO horario)
        {
            if (diaSemanaId < 1 || diaSemanaId > 7)
                throw new DomainException("Dia da semana inválido.");

            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new DomainException("Usuário não encontrado.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var horarioExistente = await _horariosRepository.ObterUsuarioEDiaAsync(usuarioId, diaSemanaId);
                if (horarioExistente != null)
                    await _horariosRepository.RemoverAsync(horarioExistente);

                if (!horario.SemExpediente)
                {
                    var novoHorario = new UsuarioHorario(
                        usuarioId,
                        diaSemanaId,
                        horario.HorarioInicio!.Value,
                        horario.HorarioFim!.Value
                    );

                    novoHorario.DefinirTolerancia(false);
                    await _horariosRepository.AdicionarAsync(novoHorario);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CopiarHorariosDeUsuarioAsync(int usuarioDestinoId, int usuarioOrigemId)
        {
            var usuarioDestino = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioDestinoId);
            if (usuarioDestino == null)
                return false;

            var usuarioOrigem = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioOrigemId);
            if (usuarioOrigem == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var horariosOrigem = await _horariosRepository.ObterPorUsuarioAsync(usuarioOrigemId);

                await _horariosRepository.RemoverHorariosPorUsuarioAsync(usuarioDestinoId);

                foreach (var horarioOrigem in horariosOrigem)
                {
                    UsuarioHorario novoHorario;

                    var duracao = (horarioOrigem.HorarioFim - horarioOrigem.HorarioInicio).TotalMinutes;
                    var ehSemExpediente = horarioOrigem.HorarioInicio == TimeSpan.Zero && horarioOrigem.HorarioFim == TimeSpan.Zero || duracao <= 0;

                    if (ehSemExpediente)
                    {
                        novoHorario = new UsuarioHorario(usuarioDestinoId, horarioOrigem.DiaSemanaId);
                    }
                    else
                    {
                        novoHorario = new UsuarioHorario(
                            usuarioDestinoId,
                            horarioOrigem.DiaSemanaId,
                            horarioOrigem.HorarioInicio,
                            horarioOrigem.HorarioFim
                        );
                    }

                    novoHorario.DefinirTolerancia(false);
                    await _horariosRepository.AdicionarAsync(novoHorario);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RemoverHorarioDiaAsync(int usuarioId, int diaSemanaId)
        {
            if (diaSemanaId < 1 || diaSemanaId > 7)
                throw new DomainException("Dia da semana inválido.");

            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new DomainException("Usuário não encontrado.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var horarioExistente = await _horariosRepository.ObterUsuarioEDiaAsync(usuarioId, diaSemanaId);

                if (horarioExistente != null)
                    await _horariosRepository.RemoverAsync(horarioExistente);

                var horarioSemExpediente = new UsuarioHorario(usuarioId, diaSemanaId);
                horarioSemExpediente.DefinirTolerancia(false);
                await _horariosRepository.AdicionarAsync(horarioSemExpediente);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private List<HorarioTrabalhoDTO>? ObterHorariosPadrao(string tipoPadro)
        {
            return tipoPadro switch
            {
                "comercial" => Enumerable.Range(1, 7).Select(dia =>
                {
                    if (dia is >= 2 and <= 6)
                    {
                        return new HorarioTrabalhoDTO
                        {
                            DiaSemanaId = dia,
                            SemExpediente = false,
                            HorarioInicio = new TimeSpan(9, 0, 0),
                            HorarioFim = new TimeSpan(18, 0, 0)
                        };
                    }

                    return new HorarioTrabalhoDTO
                    {
                        DiaSemanaId = dia,
                        SemExpediente = true
                    };
                }).ToList(),

                _ => null
            };
        }

        public async Task AplicarHorarioPadraoAsync(int usuarioId, string tipoPadrao)
        {
            var usuario = await _usuarioRepository.GetByIdAsync<Domain.Entities.Usuario.Usuario>(usuarioId);
            if (usuario is null)
                throw new DomainException("Usuário não encontrado.");

            var horariosPadrao = ObterHorariosPadrao(tipoPadrao);
            if (horariosPadrao is null)
                throw new DomainException("tipo de horário padrão inválido");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _horariosRepository.RemoverHorariosPorUsuarioAsync(usuarioId);

                foreach (var horario in horariosPadrao)
                {
                    UsuarioHorario novoHorario = horario.SemExpediente
                        ? new UsuarioHorario(usuarioId, horario.DiaSemanaId)
                        : new UsuarioHorario(usuarioId, horario.DiaSemanaId, horario.HorarioInicio!.Value, horario.HorarioFim!.Value);

                    novoHorario.DefinirTolerancia(false);
                    await _horariosRepository.AdicionarAsync(novoHorario);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
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
                            : $"{horario.HorarioInicio:hh\\:mm} - {horario.HorarioFim:hh\\:mm}"
                    });
                }
            }

            return horariosCompletos;
        }

        public async Task<ToleranciaResponseDTO> DefinirToleranciaAsync(int userId, bool ativo)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userExists = await _usuarioRepository.ExistsInDatabaseAsync<Domain.Entities.Usuario.Usuario>(userId);
                if (!userExists)
                    throw new AppException("Usuário não encontrado.");

                var agora = TimeHelper.GetBrasiliaTime();
                var diaIds = new Dictionary<DayOfWeek, int>
                {
                    { DayOfWeek.Sunday,    1 }, // Domingo
                    { DayOfWeek.Monday,    2 }, // Segunda-feira
                    { DayOfWeek.Tuesday,   3 }, // Terça-feira
                    { DayOfWeek.Wednesday, 4 }, // Quarta-feira
                    { DayOfWeek.Thursday,  5 }, // Quinta-feira
                    { DayOfWeek.Friday,    6 }, // Sexta-feira
                    { DayOfWeek.Saturday,  7 }, // Sábado
                };
                var diaId = diaIds[agora.DayOfWeek];

                var horario = await _horariosRepository.ObterUsuarioEDiaAsync(userId, diaId)
                             ?? throw new AppException("Horário do usuário para hoje não encontrado.");

                horario.DefinirTolerancia(ativo);

                await _unitOfWork.SaveChangesAsync();
                var cacheKey = $"usuario:{userId}:horarios";
                await _redisCacheService.RemoveAsync(cacheKey);
                await _unitOfWork.CommitAsync();

                return new ToleranciaResponseDTO
                {
                    UsuarioId = userId,
                    Tolerancia = ativo,
                    Mensagem = $"Tolerância é igual a {ativo}"
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

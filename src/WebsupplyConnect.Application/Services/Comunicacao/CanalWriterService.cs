using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class CanalWriterService(
        IUnitOfWork unitOfWork,
        ICanalRepository canalRepository,
        ILogger<CanalWriterService> logger,
        IValidator<CreateCanalDTO> createCanalValidator) : ICanalWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ICanalRepository _canalRepository = canalRepository ?? throw new ArgumentNullException(nameof(canalRepository));
        private readonly ILogger<CanalWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IValidator<CreateCanalDTO> _createCanalValidator = createCanalValidator ?? throw new ArgumentNullException(nameof(createCanalValidator));

        /// <summary>
        /// Método para criar um novo canal 
        /// </summary>
        public async Task Create(CreateCanalDTO dto)
        {
            try
            {
                var validationResult = await _createCanalValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para criação do canal: {errors}");
                }

                await ValidateRulesAsync(dto);

                var canal = new Canal(dto.Nome, dto.Descricao, dto.CanalTipoId, dto.EmpresaId, dto.OrigemPadraoId, dto.LimiteDiario, dto.WhatsAppNumero, dto.ConfiguracaoIntegracao);

                await _canalRepository.CreateAsync(canal);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Erro ao registrar um novo canal");
                throw;
            }
        }

        /// <summary>
        /// Validações de regras de negócio que requerem consulta ao banco
        /// </summary>
        private async Task ValidateRulesAsync(CreateCanalDTO createDto)
        {
            var empresaExiste = await _canalRepository.ExistsInDatabaseAsync<WebsupplyConnect.Domain.Entities.Empresa.Empresa>(createDto.EmpresaId);

            if (!empresaExiste)
            {
                throw new AppException($"Empresa com ID {createDto.EmpresaId} não encontrada");
            }

            var canalTipo = await _canalRepository.GetByIdAsync<CanalTipo>(createDto.CanalTipoId);
            if (canalTipo == null)
            {
                throw new AppException($"Tipo de canal com ID {createDto.CanalTipoId} não encontrado");
            }
            if (canalTipo.Codigo == "WHATSAPP")
            {
                if (string.IsNullOrWhiteSpace(createDto.WhatsAppNumero))
                {
                    throw new AppException("O número do WhatsApp é obrigatório para o canal do tipo WHATSAPP");
                }
            }

            if (!string.IsNullOrEmpty(createDto.WhatsAppNumero))
            {
                var canalExistente = await _canalRepository.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero);

                if (canalExistente != null)
                {
                    throw new AppException($"Já existe um canal cadastrado com o número WhatsApp: {createDto.WhatsAppNumero}");
                }
            }

            var canalComMesmoNome = await _canalRepository.CanalNameExistsAsync(createDto.Nome);
            if (canalComMesmoNome)
            {
                throw new AppException($"Já existe um canal com o nome '{createDto.Nome}' nesta empresa");
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Redis;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Comando especializado para transferência de lead
    /// Responsabilidade: Executar APENAS a transferência de um lead e suas entidades relacionadas
    /// Garante atomicidade através de Unit of Work pattern
    /// </summary>
    public class TransferenciaLeadCommand(
        ILeadReaderService leadReaderService,
        ILeadWriterService leadWriterService,
        IOportunidadeWriterService oportunidadeWriterService,
        IOportunidadeReaderService oportunidadeReaderService,
        IConversaReaderService conversaReaderService,
        IConversaWriterService conversaWriterService,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService,
        IMembroEquipeReaderService membroEquipeReaderService,
        IEquipeReaderService equipeReaderService,
        IUnitOfWork unitOfWork,
        IRedisCacheService redisCacheService,
        INotificacaoClient notificacaoClient,
        ILogger<TransferenciaLeadCommand> logger,
        ILeadEventoWriterService leadEventoWriterService,
        IEmpresaReaderService empresaReaderService) : ITransferenciaLeadCommand
    {
        private readonly ILeadWriterService _leadWriterService = leadWriterService ?? throw new ArgumentNullException(nameof(leadWriterService));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IOportunidadeWriterService _oportunidadeWriterService = oportunidadeWriterService ?? throw new ArgumentNullException(nameof(oportunidadeWriterService));
        private readonly IOportunidadeReaderService _oportunidadeReaderService = oportunidadeReaderService ?? throw new ArgumentNullException(nameof(oportunidadeReaderService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService ?? throw new ArgumentNullException(nameof(equipeReaderService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IRedisCacheService _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        private readonly ILogger<TransferenciaLeadCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));

        /// <summary>
        /// Constantes para configuração do comando
        /// </summary>
        private const string STATUS_CONVERSA_EXCLUIDA = "ENCERRADA";

        /// <summary>
        /// Executa a transferência completa de um lead com garantia de atomicidade
        /// </summary>
        public async Task ExecutarAsync(int leadId, int novoResponsavelId, int equipeId ,int empresaId)
        {
            // Validar parâmetros de entrada
            ValidarParametros(leadId, novoResponsavelId, equipeId ,empresaId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Obter e validar lead
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId)
                               ?? throw new AppException($"Lead {leadId} não encontrado");

                // Obtém o grupo de empresas para validação de leads duplicados no mesmo grupo
                var grupoEmpresas = await _empresaReaderService.GetGrupoEmpresaByEmpresaId(empresaId);

                // Com o numero de WhatsApp do lead, verifica se já existe outro lead em atendimento no mesmo grupo de empresas
                var leadNoMesmoGrupo = await _leadReaderService.ObterLeadPorGrupoAsync(lead.WhatsappNumero, null, null, grupoEmpresas.Id);

                if (leadNoMesmoGrupo != null && leadNoMesmoGrupo.EmpresaId != lead.EmpresaId)
                {
                    throw new AppException($"Lead não pode ser transferido pois já está em atendimento na filial: {leadNoMesmoGrupo.Empresa.Nome}.");
                }

                var responsavelAntigoId = lead.ResponsavelId;

                if (lead.ResponsavelId == novoResponsavelId)
                    throw new AppException("Não é possível transferir o lead para o mesmo responsável atual.");

                // 2️. Validar equipe e empresa
                var equipeDestino = await _equipeReaderService.GetByIdAsync(equipeId)
                                    ?? throw new AppException($"Equipe {equipeId} não encontrada.");

                if (equipeDestino.EmpresaId != empresaId)
                    throw new AppException("A equipe informada não pertence à empresa de destino.");

                // 3️. Validar membro
                var membro = await _membroEquipeReaderService.GetByIdAsync(novoResponsavelId)
                             ?? throw new AppException($"Membro {novoResponsavelId} não encontrado.");

                if (membro.EquipeId != equipeId)
                    throw new AppException("O membro informado não pertence à equipe informada.");

                if (membro.StatusMembroEquipe?.Codigo != "ATIVO")
                    throw new AppException("O membro informado está inativo.");

                if (membro.Equipe?.EmpresaId != empresaId)
                    throw new AppException("A equipe do membro não pertence à empresa informada.");

                var membroAntigo = await _membroEquipeReaderService.GetByIdAsync(lead.ResponsavelId!.Value)
                ?? throw new AppException($"Membro {novoResponsavelId} não encontrado.");

                // 4️. Buscar canais de comunicação
                var canalAntigo = await _usuarioEmpresaReaderService
                    .GetCanalPadraoByUsuarioEmpresaAsync(membroAntigo.UsuarioId, lead.EmpresaId);

                var canalNovo = await _usuarioEmpresaReaderService
                    .GetCanalPadraoByUsuarioEmpresaAsync(membro.UsuarioId, empresaId);

                // 5️. Transferir conversas e oportunidades
                await TransferirConversaAsync(leadId, membro.UsuarioId, canalNovo.CanalPadraoId, equipeId);
                if (membro.UsuarioId != membroAntigo.UsuarioId)
                {
                    await TransferirOportunidadesAsync(leadId, membro.UsuarioId, empresaId);
                }

                // 6️. Atualizar lead (Responsável, Equipe, Empresa)
                await _leadWriterService.AtualizarResponsavel(
                    lead.Id, membro.Id, equipeId, empresaId);

                // 7. Atualizar cache Redis
                await AtualizarCacheAsync(lead, membro.UsuarioId, equipeId, empresaId,
                    canalAntigo.CanalPadraoId, canalNovo.CanalPadraoId);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex,
                    "Erro ao transferir lead {LeadId} para usuário {NovoResponsavelId} (Equipe {EquipeId}, Empresa {EmpresaId})",
                    leadId, novoResponsavelId, equipeId, empresaId);

                throw;
            }
        }

        public async Task ExecutarSemOportunidadeAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId)
        {
            // Validar parâmetros de entrada
            ValidarParametros(leadId, novoResponsavelId, equipeId, empresaId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Obter e validar lead
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId)
                               ?? throw new AppException($"Lead {leadId} não encontrado");

                var responsavelAntigoId = lead.ResponsavelId;

                if (lead.ResponsavelId == novoResponsavelId)
                    throw new AppException("Não é possível transferir o lead para o mesmo responsável atual.");

                // 2️. Validar equipe e empresa
                var equipeDestino = await _equipeReaderService.GetByIdAsync(equipeId)
                                    ?? throw new AppException($"Equipe {equipeId} não encontrada.");

                if (equipeDestino.EmpresaId != empresaId)
                    throw new AppException("A equipe informada não pertence à empresa de destino.");

                // 3️. Validar membro
                var membro = await _membroEquipeReaderService.GetByIdAsync(novoResponsavelId)
                             ?? throw new AppException($"Membro {novoResponsavelId} não encontrado.");

                if (membro.EquipeId != equipeId)
                    throw new AppException("O membro informado não pertence à equipe informada.");

                if (membro.StatusMembroEquipe?.Codigo != "ATIVO")
                    throw new AppException("O membro informado está inativo.");

                if (membro.Equipe?.EmpresaId != empresaId)
                    throw new AppException("A equipe do membro não pertence à empresa informada.");

                var membroAntigo = await _membroEquipeReaderService.GetByIdAsync(lead.ResponsavelId!.Value)
                ?? throw new AppException($"Membro {novoResponsavelId} não encontrado.");

                // 4️. Buscar canais de comunicação
                var canalAntigo = await _usuarioEmpresaReaderService
                    .GetCanalPadraoByUsuarioEmpresaAsync(membroAntigo.UsuarioId, lead.EmpresaId);

                var canalNovo = await _usuarioEmpresaReaderService
                    .GetCanalPadraoByUsuarioEmpresaAsync(membro.UsuarioId, empresaId);

                // 5️. Transferir conversas e oportunidades
                await TransferirConversaAsync(leadId, membro.UsuarioId, canalNovo.CanalPadraoId, equipeId);

                // 6️. Atualizar lead (Responsável, Equipe, Empresa)
                await _leadWriterService.AtualizarResponsavel(
                    lead.Id, membro.Id, equipeId, empresaId);

                // 7. Atualizar cache Redis
                await AtualizarCacheAsync(lead, membro.UsuarioId, equipeId, empresaId,
                    canalAntigo.CanalPadraoId, canalNovo.CanalPadraoId);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex,
                    "Erro ao transferir lead {LeadId} para usuário {NovoResponsavelId} (Equipe {EquipeId}, Empresa {EmpresaId})",
                    leadId, novoResponsavelId, equipeId, empresaId);

                throw;
            }
        }

        /// <summary>
        /// Valida os parâmetros de entrada do comando
        /// </summary>
        private static void ValidarParametros(int leadId, int novoResponsavelId, int equipeId, int empresaId)
        {
            if (leadId <= 0)
                throw new ArgumentException("ID do lead deve ser maior que zero", nameof(leadId));

            if (novoResponsavelId <= 0)
                throw new ArgumentException("ID do novo responsável deve ser maior que zero", nameof(novoResponsavelId));

            if (empresaId <= 0)
                throw new ArgumentException("ID da empresa deve ser maior que zero", nameof(empresaId));
            
            if (equipeId <= 0)
                throw new ArgumentException("ID da equipe deve ser maior que zero", nameof(equipeId));
        }

        /// <summary>
        /// Transfere a conversa associada ao lead
        /// </summary>
        private async Task TransferirConversaAsync(int leadId, int novoResponsavelId, int canalId, int equipeId)
        {
            var conversa = await _conversaReaderService.GetConversaByLead(leadId, STATUS_CONVERSA_EXCLUIDA);
            if (conversa == null)
            {
                return;
            }

            await _conversaWriterService.UpdateResponsavelAsync(conversa, novoResponsavelId, canalId, equipeId);
        }

        /// <summary>
        /// Transfere as oportunidades associadas ao lead
        /// </summary>
        private async Task TransferirOportunidadesAsync(int leadId, int novoResponsavelId, int empresaId)
        {
            var oportunidades = await _oportunidadeReaderService.GetListOportunidadesByLeadIdAsync(leadId);

            if (oportunidades.Count == 0)
            {
                return;
            }

            foreach (var oportunidade in oportunidades)
            {
                await _oportunidadeWriterService.UpdateResponsavelOportunidade(
                    oportunidade, novoResponsavelId, empresaId);
            }
        }

        /// <summary>
        /// Atualiza o cache Redis após a transferência do lead.
        /// </summary>
        private async Task AtualizarCacheAsync(
            Domain.Entities.Lead.Lead lead,
            int novoResponsavelUsuarioId,
            int equipeId,
            int empresaId,
            int canalAntigoId,
            int canalNovoId)
        {
            try
            {
                // 1. Monta a chave primária do lead
                var primaryKey = $"lead:{lead.Id}";

                // 2. Remove o cache antigo (caso exista)
                await _redisCacheService.RemoveAsync(primaryKey);

                // 3. Monta o novo DTO para o cache
                var leadAtualizado = new LeadRedisDTO(
                    lead.Id,
                    lead.Nome,
                    lead.WhatsappNumero,
                    novoResponsavelUsuarioId,
                    equipeId,
                    empresaId
                );

                // 4. Salva o novo cache do lead
                await _redisCacheService.SetAsync(primaryKey, leadAtualizado);

                // 5. Atualiza o índice por número de WhatsApp, se houver
                if (!string.IsNullOrWhiteSpace(lead.WhatsappNumero))
                {
                    var oldIdxKey = $"idx:whatsapp:{lead.WhatsappNumero}:canal:{canalAntigoId}";
                    await _redisCacheService.RemoveAsync(oldIdxKey);

                    var newIdxKey = $"idx:whatsapp:{lead.WhatsappNumero}:canal:{canalNovoId}";
                    await _redisCacheService.SetStringAsync(newIdxKey, primaryKey);
                }

                _logger.LogInformation(
                    "Cache Redis atualizado para lead {LeadId}: Responsável={Responsavel}, Equipe={Equipe}, Empresa={Empresa}",
                    lead.Id, novoResponsavelUsuarioId, equipeId, empresaId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar cache Redis para lead {LeadId} (Responsável={Responsavel}, Empresa={Empresa})",
                    lead.Id, novoResponsavelUsuarioId, empresaId);
            }
        }
    }
}

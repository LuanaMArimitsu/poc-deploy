using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class TemplateReaderService(ILogger<TemplateReaderService> logger, ITemplateRepository templateRepository, IUsuarioEmpresaReaderService usuarioEmpresaService) : ITemplateReaderService
    {
        private readonly ILogger<TemplateReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaService = usuarioEmpresaService ?? throw new ArgumentNullException(nameof(usuarioEmpresaService));

        public async Task<Template> GetTemplateByIdAsync(int id)
        {
            try
            {
                return await _templateRepository.GetByIdAsync<Template>(id, false) ?? throw new AppException($"Erro ao encontrar template com o Id: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao encontrar Template Id: {id}. Erro: {erro}", id, ex.Message);
                throw new AppException($"Erro ao buscar template com o id: {id}. Erro: {ex.Message}");
            }
        }

        public async Task<Template?> GetTemplateByNameAsync(string nomeTemplate, int canalId)
        {
            try
            {
                return await _templateRepository.GetByPredicateAsync<Template>(e => e.Nome == nomeTemplate && e.CanalId == canalId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao encontrar Template pelo nome: {nome}. Erro: {erro}", nomeTemplate, ex.Message);
                throw;
            }
        }
        public async Task<List<ListaTemplatesReponseDTO>> GetListTemplates(int usuarioId, int empresaId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    throw new AppException("ID do usuário deve ser maior que zero");
                }

                if (empresaId <= 0)
                {
                    throw new AppException("ID da empresa deve ser maior que zero");
                }

                var canalPadrao = await _usuarioEmpresaService.GetCanalPadraoByUsuarioEmpresaAsync(usuarioId, empresaId) ?? throw new AppException($"Canal com usuário id {usuarioId} e empresa id {empresaId} não foi encontrado.");

                List<Template> templates = await _templateRepository.GetListByPredicateAsync<Template>(e => e.CanalId == canalPadrao.CanalPadraoId);

                var listaTemplates = templates.Select(template => new ListaTemplatesReponseDTO
                {
                    Nome = template.Nome,
                    Descricao = template.Descricao,
                    Conteudo = template.Conteudo.Replace("\r\n", " "),
                    Id = template.Id
                }).ToList();

                return listaTemplates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retornar a lista de templates. Detalhes: {detalhes}", ex.Message);
                throw new AppException(ex.Message);
            }
        }

        public async Task<Template?> GetTemplateByOrigem(int origemId, int canalId)
        {
            try
            {
                return await _templateRepository.GetTemplateByOrigem(origemId, canalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encontrar Template pela origem id: {origemId}. Erro: {erro}", origemId, ex.Message);
                throw new AppException($"Erro ao buscar template com a origem id: {origemId}. Erro: {ex.Message}");
            }
        }
    }
}

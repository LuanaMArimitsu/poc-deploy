using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadWriterService
    {
        Task<Domain.Entities.Lead.Lead> CreateAsync(LeadCompletoDTO dto, bool commit = true, string? observacaoLeadEvento = null);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int id, int statusId, string observacao);
        Task UpdateAsync(int id, LeadUpdateDTO dto, bool excluido = false);
        Task<LeadDTO> VerificarLeadExistente(string whatsappNumero, List<CanalDTO> listaCanais, string apelido);
        Task AtualizarEnderecoLeadAsync(int leadId, Endereco endereco, bool isComercial);
        Task RemoverEnderecoLeadAsync(int leadId, int enderecoId);
        Task AtualizarResponsavel(int leadId, int novoResponsavelId, int equipeId, int empresaId);
        Task AtualizarResponsavelSemNotificar(int leadId, int novoResponsavelId, int equipeId, int empresaId);
        Task AtribuirSomenteEquipe(int leadId, int equipeId);
        Task AlterarNomeLeadIdAsync(int id, string novoNome);
        Task<Domain.Entities.Lead.Lead> CreateLeadRapidoAsync(LeadRapidoDTO dto, int usuarioId);
    }
}

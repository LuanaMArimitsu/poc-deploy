namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    public interface IRedistribuicaoService
    {
        Task RedistribuirLeadAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId);

        /// <summary>
        /// Transfere todos os leads de um usuário desativado para um novo responsável
        /// </summary>
        //Task TransferirTodosLeadsDoUsuarioAsync(Domain.Entities.Usuario.Usuario usuarioDesativado, int novoResponsavelId, int empresaPadraoId);
        Task TransferirLeadSemOportunidadeAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId, bool usarCommit = false);
        Task TransferirLeadParaEquipePadraoAsync(int leadId, int empresaId);
    }
}

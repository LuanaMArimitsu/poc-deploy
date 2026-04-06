using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Equipe
{
    /// <summary>Vínculo de um usuário com uma equipe.</summary>
    public class MembroEquipe : EntidadeBase
    {
        // Propriedades
        public int EquipeId { get; private set; }
        public int UsuarioId { get; private set; }
        public int StatusMembroEquipeId { get; private set; }
        public bool IsLider { get; private set; }
        public DateTime? DataSaida { get; private set; }
        public string? Observacoes { get; private set; }

        // Navegações
        public virtual Equipe? Equipe { get; private set; }
        public virtual Usuario.Usuario? Usuario { get; private set; }
        public virtual StatusMembroEquipe? StatusMembroEquipe { get; private set; }

        /// <summary>
        /// Coleção de leads sob responsabilidade deste usuário
        /// </summary>
        public virtual ICollection<Lead.Lead> LeadsSobResponsabilidade { get; private set; }

        // EF
        protected MembroEquipe()
        {
            LeadsSobResponsabilidade = new HashSet<Lead.Lead>();
        }

        public MembroEquipe(
            int equipeId,
            int usuarioId,
            int statusMembroEquipeId,
            bool isLider = false,
            string? observacoes = null)
        {
            ValidarDominio(equipeId, usuarioId, statusMembroEquipeId);

            EquipeId = equipeId;
            UsuarioId = usuarioId;
            StatusMembroEquipeId = statusMembroEquipeId;
            IsLider = isLider;

            Observacoes = observacoes;

            LeadsSobResponsabilidade = new HashSet<Lead.Lead>();
        }

        // Regras de negócio
        public void DefinirComoLider()
        {
            IsLider = true;
            AtualizarDataModificacao();
        }

        public void RemoverLideranca()
        {
            IsLider = false;
            AtualizarDataModificacao();
        }

        public void AlterarStatus(int novoStatusId)
        {
            if (novoStatusId <= 0)
                throw new DomainException("O status deve ser informado.", nameof(MembroEquipe));

            StatusMembroEquipeId = novoStatusId;
            AtualizarDataModificacao();
        }

        public void AtualizarObservacoes(string? observacoes)
        {
            Observacoes = observacoes;
            AtualizarDataModificacao();
        }

        public void DefinirDataSaida(DateTime? dataSaida = null)
        {
            DataSaida = dataSaida ?? TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }

        public void RemoverDataSaida()
        {
            DataSaida = null;
            AtualizarDataModificacao();
        }

        // Validações internas
        private static void ValidarDominio(int equipeId, int usuarioId, int statusMembroEquipeId)
        {
            if (equipeId <= 0)
                throw new DomainException("Equipe deve ser informada.", nameof(MembroEquipe));

            if (usuarioId <= 0)
                throw new DomainException("Usuário deve ser informado.", nameof(MembroEquipe));

            if (statusMembroEquipeId <= 0)
                throw new DomainException("Status do membro deve ser informado.", nameof(MembroEquipe));
        }

        public void Reativar(int statusMembroEquipeId, bool isLider, string? observacoes)
        {
            StatusMembroEquipeId = statusMembroEquipeId;
            IsLider = isLider;
            Observacoes = observacoes;
            Excluido = false;
            DataSaida = null;
            AtualizarDataModificacao();
        }

    }
}

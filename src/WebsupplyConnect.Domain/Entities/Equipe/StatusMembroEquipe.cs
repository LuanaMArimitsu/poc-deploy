using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Equipe
{
    public class StatusMembroEquipe : EntidadeTipificacao
    {
        /// <summary>Membros com este status.</summary>
        public virtual ICollection<MembroEquipe> Membros { get; private set; }

        /// <summary>EF Core</summary>
        protected StatusMembroEquipe() : base()
        {
            Membros = new HashSet<MembroEquipe>();
        }

        public StatusMembroEquipe(
           int id,
           DateTime dataCriacao,
           DateTime dataModificacao,
           string codigo,
           string nome,
           string descricao,
           int ordem,
           string icone = null,
           string cor = null) : base (codigo, nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;

            Membros = new HashSet<MembroEquipe>();
        }
    }
}

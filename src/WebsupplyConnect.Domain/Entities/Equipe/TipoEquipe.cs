using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Equipe
{
    public class TipoEquipe : EntidadeTipificacao
    {
        /// <summary>Equipes que usam este tipo.</summary>
        public virtual ICollection<Equipe> Equipes { get; private set; }

        /// <summary>EF Core</summary>
        protected TipoEquipe() : base()
        {
            Equipes = new HashSet<Equipe>();
        }

        public TipoEquipe(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            string cor = null)
        {
            Id = id;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            Equipes = new HashSet<Equipe>();
        }
    }
}

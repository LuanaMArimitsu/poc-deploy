using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Base
{
    /// <summary>
    /// Classe base abstrata para todas as entidades do domínio
    /// </summary>
    public abstract class EntidadeBase
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }
        
        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime DataCriacao { get; protected set; }
        
        /// <summary>
        /// Data da última modificação do registro
        /// </summary>
        public DateTime DataModificacao { get; protected set; }
        
        /// <summary>
        /// Flag que indica se o registro foi excluído logicamente
        /// </summary>
        public bool Excluido { get; protected set; }

        protected EntidadeBase()
        {
            DataCriacao = TimeHelper.GetBrasiliaTime();
            DataModificacao = TimeHelper.GetBrasiliaTime();
            Excluido = false;
        }

        /// <summary>
        /// Marca a entidade como excluída logicamente
        /// </summary>
        public virtual void ExcluirLogicamente()
        {
            if (!Excluido)
            {
                Excluido = true;
                AtualizarDataModificacao();
            }
        }

        /// <summary>
        /// Restaura a entidade após uma exclusão lógica
        /// </summary>
        public virtual void RestaurarExclusaoLogica()
        {
            if (Excluido)
            {
                Excluido = false;
                AtualizarDataModificacao();
            }
        }
        
        /// <summary>
        /// Atualiza a data de modificação para o momento atual
        /// </summary>
        protected void AtualizarDataModificacao()
        {
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }
    }
}
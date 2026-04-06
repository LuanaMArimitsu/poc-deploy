using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    public class CanalTipo : EntidadeTipificacao
    {
        // Construtor protegido para EF
        protected CanalTipo() : base()
        {

        }

        /// <summary>
        /// Cria um novo tipo de Canal
        /// </summary>
        public CanalTipo( int id, string codigo, string nome, string descricao, int ordem, DateTime dataCriacao, DateTime dataModificacao)
        {
            Id = id;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
        }

        /// <summary>
        /// Atualiza as informações do tipo
        /// </summary>
        public override void Atualizar(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }

    }
}

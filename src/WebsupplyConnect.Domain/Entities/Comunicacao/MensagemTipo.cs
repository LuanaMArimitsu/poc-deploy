using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa o tipo de uma mensagem
    /// </summary>
    public class MensagemTipo : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se o tipo suporta mídia
        /// </summary>
        public bool SuportaMidia { get; private set; }

        /// <summary>
        /// Indica se o tipo requer mídia
        /// </summary>
        public bool RequerMidia { get; private set; }

        // Construtor protegido para EF
        protected MensagemTipo() : base()
        {
        }

        /// <summary>
        /// Cria um novo tipo de mensagem
        /// </summary>
        public MensagemTipo(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool suportaMidia = false,
            bool requerMidia = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            SuportaMidia = suportaMidia;
            RequerMidia = requerMidia;

            if (RequerMidia && !SuportaMidia)
            {
                throw new DomainException("Um tipo que requer mídia deve também suportar mídia", nameof(requerMidia));
            }
        }

        /// <summary>
        /// Cria um seed novo tipo de mensagem
        /// </summary>
        public MensagemTipo(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool suportaMidia = false,
            bool requerMidia = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            SuportaMidia = suportaMidia;
            RequerMidia = requerMidia;

            if (RequerMidia && !SuportaMidia)
            {
                throw new DomainException("Um tipo que requer mídia deve também suportar mídia", nameof(requerMidia));
            }
        }

        /// <summary>
        /// Atualiza as informações do tipo
        /// </summary>
        public override void Atualizar(
            string nome,
            string descricao,
            int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }

        /// <summary>
        /// Atualiza as propriedades específicas do tipo de mensagem
        /// </summary>
        public void AtualizarPropriedadesEspecificas(
            string icone,
            bool suportaMidia,
            bool requerMidia)
        {
            if (requerMidia && !suportaMidia)
            {
                throw new DomainException("Um tipo que requer mídia deve também suportar mídia", nameof(requerMidia));
            }

            SuportaMidia = suportaMidia;
            RequerMidia = requerMidia;
            AtualizarDataModificacao();
        }
    }
}
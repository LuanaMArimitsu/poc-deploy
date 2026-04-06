using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa o status de um lead no sistema.
    /// Herda de EntidadeTipificacao para implementar o padrão TPH (Table Per Hierarchy).
    /// </summary>
    public class LeadStatus : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se leads com este status podem gerar oportunidades
        /// </summary>
        public bool PermiteOportunidades { get; private set; }

        /// <summary>
        /// Indica se leads com este status são considerados clientes
        /// </summary>
        public bool ConsiderarCliente { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected LeadStatus() : base()
        {
        }

        /// <summary>
        /// Construtor para criar um novo status de lead
        /// </summary>
        /// <param name="codigo">Código único do status</param>
        /// <param name="nome">Nome do status</param>
        /// <param name="descricao">Descrição do status</param>
        /// <param name="ordem">Ordem de exibição</param>
        /// <param name="cor">Cor hexadecimal do status</param>
        /// <param name="permiteOportunidades">Indica se permite gerar oportunidades</param>
        /// <param name="considerarCliente">Indica se é considerado cliente</param>
        public LeadStatus(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string cor,
            bool permiteOportunidades = false,
            bool considerarCliente = false) : base(codigo, nome, descricao, ordem, null, cor)
        {
            PermiteOportunidades = permiteOportunidades;
            ConsiderarCliente = considerarCliente;
        }

        /// <summary>
        /// Construtor para criar um novo status de lead
        /// </summary>
        /// <param name="codigo">Código único do status</param>
        /// <param name="nome">Nome do status</param>
        /// <param name="descricao">Descrição do status</param>
        /// <param name="ordem">Ordem de exibição</param>
        /// <param name="cor">Cor hexadecimal do status</param>
        /// <param name="permiteOportunidades">Indica se permite gerar oportunidades</param>
        /// <param name="considerarCliente">Indica se é considerado cliente</param>
        public LeadStatus(
            int id,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string cor,
            DateTime dataCriacao,
            DateTime dataModificacao,
            bool permiteOportunidades = false,
            bool considerarCliente = false
            ) : base(codigo, nome, descricao, ordem, null, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            PermiteOportunidades = permiteOportunidades;
            ConsiderarCliente = considerarCliente;
        }

        /// <summary>
        /// Atualiza as propriedades específicas do status de lead
        /// </summary>
        /// <param name="cor">Cor hexadecimal do status</param>
        /// <param name="permiteOportunidades">Indica se permite gerar oportunidades</param>
        /// <param name="considerarCliente">Indica se é considerado cliente</param>
        public void AtualizarPropriedades(string cor, bool permiteOportunidades, bool considerarCliente)
        {
            PermiteOportunidades = permiteOportunidades;
            ConsiderarCliente = considerarCliente;

            AtualizarDataModificacao();
        }

    }
}
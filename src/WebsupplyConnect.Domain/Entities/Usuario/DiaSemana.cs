using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Usuario
{
    /// <summary>
    /// Entidade que representa um dia da semana
    /// </summary>
    public class DiaSemana
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Descrição do dia da semana
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected DiaSemana()
        {
        }

        /// <summary>
        /// Construtor para criar um novo dia da semana
        /// </summary>
        /// <param name="id">ID do dia da semana (1 = Domingo, 2 = Segunda, ...)</param>
        /// <param name="descricao">Descrição do dia da semana</param>
        public DiaSemana(int id, string descricao)
        {
            Id = id;
            Descricao = descricao;
        }
    }
}
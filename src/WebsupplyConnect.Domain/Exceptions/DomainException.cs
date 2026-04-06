using System.Runtime.Serialization;

namespace WebsupplyConnect.Domain.Exceptions
{
    /// <summary>
    /// Exceção personalizada para representar erros de domínio no sistema.
    /// Deve ser utilizada quando regras de negócio ou invariantes de domínio são violadas.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// Inicializa uma nova instância da classe DomainException.
        /// </summary>
        public DomainException() : base()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe DomainException com uma mensagem de erro específica.
        /// </summary>
        /// <param name="message">Mensagem que descreve o erro.</param>
        public DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe DomainException com uma mensagem de erro específica
        /// e uma referência à exceção interna que é a causa desse erro.
        /// </summary>
        /// <param name="message">Mensagem que descreve o erro.</param>
        /// <param name="innerException">Exceção que é a causa do erro atual.</param>
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe DomainException com uma mensagem de erro específica
        /// e o nome da entidade ou conceito do domínio onde ocorreu o erro.
        /// </summary>
        /// <param name="message">Mensagem que descreve o erro.</param>
        /// <param name="entityName">Nome da entidade ou conceito do domínio onde ocorreu o erro.</param>
        public DomainException(string message, string entityName) : base($"{entityName}: {message}")
        {
            EntityName = entityName;
        }

        /// <summary>
        /// Nome da entidade ou conceito do domínio onde ocorreu o erro.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Quando sobrescrito em uma classe derivada, configura o objeto SerializationInfo com as informações
        /// sobre a exceção.
        /// </summary>
        /// <param name="info">O objeto SerializationInfo que contém os dados serializados do objeto.</param>
        /// <param name="context">O objeto que descreve a origem ou o destino dos dados serializados.</param>
        protected DomainException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
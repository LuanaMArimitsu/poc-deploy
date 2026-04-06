using System;

namespace WebsupplyConnect.Application.Common
{
    /// <summary>
    /// Exceção lançada quando um recurso não é encontrado
    /// </summary>
    public class NotFoundAppException : Exception
    {
        /// <summary>
        /// Construtor padrão
        /// </summary>
        public NotFoundAppException() : base("O recurso solicitado não foi encontrado.")
        {
        }

        /// <summary>
        /// Construtor com mensagem personalizada
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        public NotFoundAppException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construtor com mensagem personalizada e exceção interna
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        /// <param name="innerException">Exceção interna</param>
        public NotFoundAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
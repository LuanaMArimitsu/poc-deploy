using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace WebsupplyConnect.Application.Common
{
    /// <summary>
    /// Exceção lançada quando ocorre erro de validação
    /// </summary>
    public class ValidationAppException : Exception
    {
        /// <summary>
        /// Lista de erros de validação
        /// </summary>
        public IList<ValidationFailure> Errors { get; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public ValidationAppException() : base("Ocorreram um ou mais erros de validação.")
        {
            Errors = new List<ValidationFailure>();
        }

        /// <summary>
        /// Construtor com lista de erros
        /// </summary>
        /// <param name="errors">Lista de erros de validação</param>
        public ValidationAppException(IList<ValidationFailure> errors) : base("Ocorreram um ou mais erros de validação.")
        {
            Errors = errors;
        }

        /// <summary>
        /// Construtor com mensagem personalizada
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        public ValidationAppException(string message) : base(message)
        {
            Errors = new List<ValidationFailure>();
        }

        /// <summary>
        /// Construtor com mensagem personalizada e exceção interna
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        /// <param name="innerException">Exceção interna</param>
        public ValidationAppException(string message, Exception innerException) : base(message, innerException)
        {
            Errors = new List<ValidationFailure>();
        }
    }
}
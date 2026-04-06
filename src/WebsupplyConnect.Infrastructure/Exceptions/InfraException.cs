namespace WebsupplyConnect.Infrastructure.Exceptions
{
    /// <summary>
    /// Exceção genérica para falhas em serviços de infraestrutura (ex: Azure, APIs externas).
    /// </summary>
    public class InfraException : Exception
    {
        public InfraException() { }
        public InfraException(string message)
             : base(message) { }

        public InfraException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

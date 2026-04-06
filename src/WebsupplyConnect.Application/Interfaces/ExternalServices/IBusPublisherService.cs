namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface  IBusPublisherService
    {
        /// <summary>
        /// Publica o conteúdo desejado no Azure Bus
        /// </summary>
        /// <param name="message">Conteúdo ou mensagem a ser publicado no Azure Bus</param>
        Task PublishAsync<T>(T message) where T : class;
    }
}

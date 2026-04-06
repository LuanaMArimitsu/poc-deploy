using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface ICanalWriterService
    {
        public Task Create(CreateCanalDTO dto);
    }
}

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadExportService
    {
        Task ExportarLeadsEEnviarPorEmailAsync(
                    int empresaId,
                    string destinatarioEmail,
                    string destinatarioNome,
                    int? equipeId = null,
                    int? usuarioId = null,
                    int? statusId = null,
                    DateTime? de = null,
                    DateTime? ate = null);
    }
}

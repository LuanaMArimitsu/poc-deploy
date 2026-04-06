namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record CreateCanalDTO(
        string Nome,
        string Descricao,
        int CanalTipoId,
        int EmpresaId,
        int OrigemPadraoId,
        int? LimiteDiario,
        string? WhatsAppNumero,
        string? ConfiguracaoIntegracao
    );
}

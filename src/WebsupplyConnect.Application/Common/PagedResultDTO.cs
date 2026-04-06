namespace WebsupplyConnect.Application.Common;

/// <summary>
/// DTO genérico para resultados paginados
/// </summary>
public class PagedResultDTO<T>
{
    public List<T> Itens { get; set; } = new();
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
}

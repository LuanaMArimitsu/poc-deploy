namespace WebsupplyConnect.Application.DTOs.Produto
{
    public class ProdutoDetalhadoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal? ValorReferencia { get; set; }
        public string? Url { get; set; }
        public bool Ativo { get; set; }
        public List<ProdutoEmpresaDTO> Empresas { get; set; } = new();
        public List<ProdutoHistoricoDTO> Historico { get; set; } = new();
    }

    public class ProdutoEmpresaDTO
    {
        public int EmpresaId { get; set; }
        public string NomeEmpresa { get; set; }
        public decimal? ValorPersonalizado { get; set; }
        public DateTime DataAssociacao { get; set; }
    }

    public class ProdutoHistoricoDTO
    {
        public DateTime DataOperacao { get; set; }
        public string NomeUsuario { get; set; }
        public string NomeOperacao { get; set; }
        public string Descricao { get; set; }
        public string Json { get; set; }
    }
}

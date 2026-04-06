namespace WebsupplyConnect.Application.DTOs.Empresa
{
    public class EmpresaDetalheDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string RazaoSocial { get; set; }
        public string Cnpj { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public bool Ativo { get; set; }
        public GrupoEmpresaDTO GrupoEmpresa { get; set; }
    }

    public class GrupoEmpresaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CnpjHolding { get; set; }
        public bool Ativo { get; set; }
    }
}

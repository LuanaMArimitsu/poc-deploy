using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

/// <summary>
/// Dimensão de vendedores. IMPORTANTE: Exclui usuários com IsBot = true
/// </summary>
public class DimensaoVendedor : EntidadeBase
{
    public int UsuarioOrigemId { get; private set; }  // ID da tabela Usuario transacional
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public int? EquipeId { get; private set; }
    public int? EmpresaId { get; private set; }
    public bool Ativo { get; private set; }

    protected DimensaoVendedor() { } // EF Core

    public DimensaoVendedor(int usuarioOrigemId, string nome, string email,
        int? equipeId, int? empresaId, bool ativo) : base()
    {
        UsuarioOrigemId = usuarioOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? string.Empty;
        EquipeId = equipeId;
        EmpresaId = empresaId;
        Ativo = ativo;
    }

    public void Atualizar(string nome, string email, int? equipeId, int? empresaId, bool ativo)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? string.Empty;
        EquipeId = equipeId;
        EmpresaId = empresaId;
        Ativo = ativo;
        AtualizarDataModificacao();
    }
}

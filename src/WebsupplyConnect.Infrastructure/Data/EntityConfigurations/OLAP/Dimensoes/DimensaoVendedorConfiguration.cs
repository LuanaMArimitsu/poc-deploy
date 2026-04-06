using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoVendedorConfiguration : EntidadeBaseConfiguration<DimensaoVendedor>
{
    public override void Configure(EntityTypeBuilder<DimensaoVendedor> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoVendedor", "OLAP");

        builder.Property(d => d.UsuarioOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Email).HasMaxLength(200);
        builder.Property(d => d.EquipeId).IsRequired(false);
        builder.Property(d => d.EmpresaId).IsRequired(false);
        builder.Property(d => d.Ativo).IsRequired();

        builder.HasIndex(d => d.UsuarioOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoVendedor_UsuarioOrigemId");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoEmpresaConfiguration : EntidadeBaseConfiguration<DimensaoEmpresa>
{
    public override void Configure(EntityTypeBuilder<DimensaoEmpresa> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoEmpresa", "OLAP");

        builder.Property(d => d.EmpresaOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Ativa).IsRequired();
        builder.Property(d => d.GrupoEmpresaId).IsRequired();

        builder.HasIndex(d => d.EmpresaOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoEmpresa_EmpresaOrigemId");
    }
}

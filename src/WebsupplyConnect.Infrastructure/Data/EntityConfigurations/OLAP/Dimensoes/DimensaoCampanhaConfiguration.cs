using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoCampanhaConfiguration : EntidadeBaseConfiguration<DimensaoCampanha>
{
    public override void Configure(EntityTypeBuilder<DimensaoCampanha> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoCampanha", "OLAP");

        builder.Property(d => d.CampanhaOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Codigo).HasMaxLength(100);
        builder.Property(d => d.Ativo).IsRequired();
        builder.Property(d => d.Temporaria).IsRequired();
        builder.Property(d => d.EmpresaId).IsRequired();
        builder.Property(d => d.GrupoEmpresaId).IsRequired();

        builder.HasIndex(d => d.CampanhaOrigemId)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("UX_DimensaoCampanha_CampanhaOrigemId");
    }
}

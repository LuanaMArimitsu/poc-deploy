using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoCampanhaMapeamentoConfiguration : EntidadeBaseConfiguration<DimensaoCampanhaMapeamento>
{
    public override void Configure(EntityTypeBuilder<DimensaoCampanhaMapeamento> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoCampanhaMapeamento", "OLAP");

        builder.Property(d => d.CampanhaOrigemId).IsRequired();
        builder.Property(d => d.DimensaoCampanhaId).IsRequired();

        builder.HasIndex(d => d.CampanhaOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoCampanhaMapeamento_CampanhaOrigemId");
    }
}

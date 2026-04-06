using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoOrigemConfiguration : EntidadeBaseConfiguration<DimensaoOrigem>
{
    public override void Configure(EntityTypeBuilder<DimensaoOrigem> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoOrigem", "OLAP");

        builder.Property(d => d.OrigemOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.OrigemTipoId).IsRequired(false);
        builder.Property(d => d.Descricao).HasMaxLength(500);

        builder.HasIndex(d => d.OrigemOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoOrigem_OrigemOrigemId");
    }
}

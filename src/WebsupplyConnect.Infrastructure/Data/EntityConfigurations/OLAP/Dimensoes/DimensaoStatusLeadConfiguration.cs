using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoStatusLeadConfiguration : EntidadeBaseConfiguration<DimensaoStatusLead>
{
    public override void Configure(EntityTypeBuilder<DimensaoStatusLead> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoStatusLead", "OLAP");

        builder.Property(d => d.StatusOrigemId).IsRequired();
        builder.Property(d => d.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Cor).HasMaxLength(50);
        builder.Property(d => d.Ordem).IsRequired();

        builder.HasIndex(d => d.StatusOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoStatusLead_StatusOrigemId");
    }
}

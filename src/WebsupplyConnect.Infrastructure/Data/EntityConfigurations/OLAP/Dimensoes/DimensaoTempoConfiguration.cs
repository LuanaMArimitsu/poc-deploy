using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoTempoConfiguration : EntidadeBaseConfiguration<DimensaoTempo>
{
    public override void Configure(EntityTypeBuilder<DimensaoTempo> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoTempo", "OLAP");

        builder.Property(d => d.Ano).IsRequired();
        builder.Property(d => d.Mes).IsRequired();
        builder.Property(d => d.Dia).IsRequired();
        builder.Property(d => d.Hora).IsRequired();
        builder.Property(d => d.DiaSemana).IsRequired();
        builder.Property(d => d.Trimestre).IsRequired();
        builder.Property(d => d.Semana).IsRequired();
        builder.Property(d => d.DataCompleta).IsRequired().HasColumnType("datetime2");

        builder.HasIndex(d => new { d.Ano, d.Mes, d.Dia, d.Hora })
            .IsUnique()
            .HasDatabaseName("UX_DimensaoTempo_AnoMesDiaHora");

        builder.HasIndex(d => d.DataCompleta)
            .HasDatabaseName("IX_DimensaoTempo_DataCompleta");
    }
}

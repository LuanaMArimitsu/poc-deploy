using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Controle;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Controle;

public class ETLControleProcessamentoConfiguration : EntidadeBaseConfiguration<ETLControleProcessamento>
{
    public override void Configure(EntityTypeBuilder<ETLControleProcessamento> builder)
    {
        base.Configure(builder);

        builder.ToTable("ETLControleProcessamento", "OLAP");

        builder.Property(c => c.TipoProcessamento).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UltimaDataProcessada).IsRequired().HasColumnType("datetime2");
        builder.Property(c => c.DataUltimaExecucao).IsRequired().HasColumnType("datetime2");
        builder.Property(c => c.StatusUltimaExecucao).IsRequired().HasMaxLength(50);
        builder.Property(c => c.MensagemErro).HasMaxLength(4000);

        builder.HasIndex(c => c.TipoProcessamento)
            .IsUnique()
            .HasDatabaseName("UX_ETLControleProcessamento_TipoProcessamento");
    }
}

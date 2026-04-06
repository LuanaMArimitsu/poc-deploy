using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoEtapaFunilConfiguration : EntidadeBaseConfiguration<DimensaoEtapaFunil>
{
    public override void Configure(EntityTypeBuilder<DimensaoEtapaFunil> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoEtapaFunil", "OLAP");

        builder.Property(d => d.EtapaOrigemId).IsRequired();
        builder.Property(d => d.FunilDimensaoId).IsRequired();
        builder.Property(d => d.FunilOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Ordem).IsRequired();
        builder.Property(d => d.Cor).IsRequired().HasMaxLength(20);
        builder.Property(d => d.ProbabilidadePadrao).IsRequired();
        builder.Property(d => d.EhAtiva).IsRequired();
        builder.Property(d => d.EhFinal).IsRequired();
        builder.Property(d => d.EhVitoria).IsRequired();
        builder.Property(d => d.EhPerdida).IsRequired();
        builder.Property(d => d.EhExibida).IsRequired();
        builder.Property(d => d.Ativo).IsRequired();

        builder.HasIndex(d => d.EtapaOrigemId)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("UX_DimensaoEtapaFunil_EtapaOrigemId");

        builder.HasIndex(d => new { d.FunilDimensaoId, d.Ordem })
            .HasDatabaseName("IX_DimensaoEtapaFunil_FunilDimensaoId_Ordem");

        builder.HasIndex(d => d.FunilOrigemId)
            .HasDatabaseName("IX_DimensaoEtapaFunil_FunilOrigemId");

        builder.HasOne(d => d.Funil)
            .WithMany()
            .HasForeignKey(d => d.FunilDimensaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

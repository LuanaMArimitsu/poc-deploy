using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoEquipeConfiguration : EntidadeBaseConfiguration<DimensaoEquipe>
{
    public override void Configure(EntityTypeBuilder<DimensaoEquipe> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoEquipe", "OLAP");

        builder.Property(d => d.EquipeOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.TipoEquipeId).IsRequired(false);
        builder.Property(d => d.EmpresaId).IsRequired();
        builder.Property(d => d.Ativa).IsRequired();

        builder.HasIndex(d => d.EquipeOrigemId)
            .IsUnique()
            .HasDatabaseName("UX_DimensaoEquipe_EquipeOrigemId");
    }
}

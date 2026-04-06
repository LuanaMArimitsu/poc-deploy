using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Dimensoes;

public class DimensaoFunilConfiguration : EntidadeBaseConfiguration<DimensaoFunil>
{
    public override void Configure(EntityTypeBuilder<DimensaoFunil> builder)
    {
        base.Configure(builder);

        builder.ToTable("DimensaoFunil", "OLAP");

        builder.Property(d => d.FunilOrigemId).IsRequired();
        builder.Property(d => d.EmpresaOrigemId).IsRequired();
        builder.Property(d => d.Nome).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Ativo).IsRequired();
        builder.Property(d => d.EhPadrao).IsRequired();
        builder.Property(d => d.Cor).HasMaxLength(20);

        builder.HasIndex(d => d.FunilOrigemId)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("UX_DimensaoFunil_FunilOrigemId");

        builder.HasIndex(d => d.EmpresaOrigemId)
            .HasDatabaseName("IX_DimensaoFunil_EmpresaOrigemId");
    }
}

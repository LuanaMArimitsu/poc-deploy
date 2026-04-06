using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ProdutoConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade Produto
    /// </summary>
    public class ProdutoConfiguration : EntidadeBaseConfiguration<Produto>
    {
        public override void Configure(EntityTypeBuilder<Produto> builder)
        {
            base.Configure(builder);
            builder.ToTable("Produto");

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Descricao)
                .HasMaxLength(1000);

            builder.Property(p => p.ValorReferencia)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Url)
                .HasMaxLength(500);

            builder.Property(p => p.Ativo)
                .IsRequired();

            builder.HasMany(p => p.ProdutoEmpresas)
                .WithOne(pe => pe.Produto)
                .HasForeignKey(pe => pe.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Historicos)
                .WithOne(h => h.Produto)
                .HasForeignKey(h => h.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.Nome);
            builder.HasIndex(p => p.Ativo);
        }
    }
}
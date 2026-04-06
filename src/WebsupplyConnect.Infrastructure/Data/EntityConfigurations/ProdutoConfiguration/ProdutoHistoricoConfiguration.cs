using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ProdutoConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade ProdutoHistorico
    /// </summary>
    public class ProdutoHistoricoConfiguration : IEntityTypeConfiguration<ProdutoHistorico>
    {
        public void Configure(EntityTypeBuilder<ProdutoHistorico> builder)
        {
            builder.ToTable("ProdutoHistorico");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.DetalhesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(h => h.DataOperacao)
                .IsRequired();

            builder.HasOne(h => h.Produto)
                .WithMany(p => p.Historicos)
                .HasForeignKey(h => h.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Usuario)
                .WithMany()
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.TipoOperacao)
                .WithMany()
                .HasForeignKey(h => h.TipoOperacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(h => h.ProdutoId);
            builder.HasIndex(h => h.UsuarioId);
            builder.HasIndex(h => h.TipoOperacaoId);
            builder.HasIndex(h => h.DataOperacao);
        }
    }
}


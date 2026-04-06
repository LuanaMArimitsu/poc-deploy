using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ProdutoConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade ProdutoEmpresa
    /// </summary>
    public class ProdutoEmpresaConfiguration : IEntityTypeConfiguration<ProdutoEmpresa>
    {
        public void Configure(EntityTypeBuilder<ProdutoEmpresa> builder)
        {
            builder.ToTable("ProdutoEmpresa");

            builder.HasKey(pe => pe.Id);

            builder.Property(pe => pe.ValorPersonalizado)
                .HasColumnType("decimal(18,2)");

            builder.Property(pe => pe.DataAssociacao)
                .IsRequired();

            builder.HasOne(pe => pe.Produto)
                .WithMany(p => p.ProdutoEmpresas)
                .HasForeignKey(pe => pe.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pe => pe.Empresa)
                .WithMany()
                .HasForeignKey(pe => pe.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pe => new { pe.ProdutoId, pe.EmpresaId })
                .IsUnique();
        }
    }
}

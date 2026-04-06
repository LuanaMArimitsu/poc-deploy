using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Permissao;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.PerfilConfiguration
{
    public class RolePermissaoConfiguration : IEntityTypeConfiguration<RolePermissao>
    {
        public void Configure(EntityTypeBuilder<RolePermissao> builder)
        {
            builder.ToTable("RolePermissoes");

            // Chave composta
            builder.HasKey(rp => new { rp.RoleId, rp.PermissaoId });

            // Propriedades
            builder.Property(rp => rp.DataConcessao)
                .IsRequired();

            builder.Property(rp => rp.Observacoes)
                .HasMaxLength(200);

            builder.Property(rp => rp.ConcessorId)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissoes)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permissao)
                .WithMany(p => p.RolePermissoes)
                .HasForeignKey(rp => rp.PermissaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Concessor)
                .WithMany(u => u.RolePermissoesConcedidas)
                .HasForeignKey(rp => rp.ConcessorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

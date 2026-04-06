using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Permissao;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.PerfilConfiguration
{
    public class UsuarioRoleConfiguration : IEntityTypeConfiguration<UsuarioRole>
    {
        public void Configure(EntityTypeBuilder<UsuarioRole> builder)
        {
            builder.ToTable("UsuariosRoles");

            // Chave composta
            builder.HasKey(ur => new { ur.UsuarioId, ur.RoleId });

            // Propriedades
            builder.Property(ur => ur.DataAtribuicao)
                .IsRequired();

            builder.Property(ur => ur.DataExpiracao)
                .IsRequired(false);

            builder.Property(ur => ur.Ativo)
                .IsRequired();

            builder.Property(ur => ur.AtribuidorId)
                .IsRequired();

            builder.Property(ur => ur.Justificativa)
                .IsRequired(false)
                .HasMaxLength(500);

            // Relacionamentos
            builder.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Atribuidor)
                .WithMany(u => u.UsuarioRolesAtribuidos)
                .HasForeignKey(ur => ur.AtribuidorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

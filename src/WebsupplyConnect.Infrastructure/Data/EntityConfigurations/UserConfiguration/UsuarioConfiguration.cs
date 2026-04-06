using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.UserConfiguration
{
    public class UsuarioConfiguration : EntidadeBaseConfiguration<Usuario>
    {
        public override void Configure(EntityTypeBuilder<Usuario> builder)
        {
            base.Configure(builder);
            // Configurações específicas do Usuario
            builder.ToTable("Usuarios");

            // Configurações de propriedades específicas da entidade Usuario
            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Cargo)
                .HasMaxLength(100);

            builder.Property(u => u.Departamento)
                .HasMaxLength(100);

            builder.Property(u => u.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.ObjectId)
                .IsRequired()
                .HasMaxLength(36); // Tamanho típico de um GUID

            builder.Property(u => u.Upn)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.DisplayName)
                .HasMaxLength(200);

            builder.Property(u => u.IsExternal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.DataCriacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(u => u.DataModificacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(u => u.Excluido)
                .IsRequired()
                .HasDefaultValue(false);

            // Relacionamentos

            // Relacionamento com UsuarioSuperior (hierarquia)
            builder.HasOne(u => u.UsuarioSuperior)
                .WithMany(u => u.Subordinados)
                .HasForeignKey(u => u.UsuarioSuperiorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Índice para consultas por nome
            builder.HasIndex(u => u.Nome);

            // Índice para consultas por status (ativo/inativo)
            builder.HasIndex(u => u.Ativo);
        }
    }
}

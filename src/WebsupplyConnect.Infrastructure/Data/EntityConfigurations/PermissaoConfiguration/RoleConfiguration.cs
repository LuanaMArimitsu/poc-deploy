using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.PerfilConfiguration
{
    public class RoleConfiguration : EntidadeBaseConfiguration<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            builder.ToTable("Roles");

            // Propriedades
            builder.Property(r => r.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.Ativa)
                .IsRequired();

            builder.Property(r => r.IsSistema)
                .IsRequired();

            builder.Property(r => r.Contexto)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.EmpresaId)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(rp => rp.Empresa)
                .WithMany(r => r.Roles)
                .HasForeignKey(rp => rp.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

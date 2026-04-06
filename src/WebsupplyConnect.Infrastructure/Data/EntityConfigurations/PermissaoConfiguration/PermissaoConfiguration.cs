using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.PerfilConfiguration
{
    public class PermissaoConfiguration : EntidadeBaseConfiguration<Permissao>
    {
        public override void Configure(EntityTypeBuilder<Permissao> builder)
        {
            base.Configure(builder);

            builder.ToTable("Permissoes");

            // Propriedades
            builder.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Descricao)
                .HasMaxLength(200);

            builder.Property(p => p.Modulo)
                .HasMaxLength(200);

            builder.Property(p => p.Categoria)
               .HasMaxLength(200);


            builder.Property(p => p.Recurso)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(p => p.Acao)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.IsCritica)
                .IsRequired();

            builder.Property(p => p.Ativa)
                .IsRequired();

            builder.HasIndex(p => p.Codigo)
                .IsUnique();
        }
    }
}

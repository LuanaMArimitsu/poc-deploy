using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class TemplateConfiguration : EntidadeBaseConfiguration<Template>
    {
        public override void Configure(EntityTypeBuilder<Template> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);

            // Tabela
            builder.ToTable("Templates");

            // Propriedades específicas do Template
            builder.Property(t => t.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Conteudo)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.Descricao)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.CategoriaId)
                .IsRequired();

            builder.Property(t => t.ParametrosContagem)
                .IsRequired();

            builder.Property(t => t.Exemplo)
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            builder.Property(t => t.CanalId)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(t => t.Categoria)
                .WithMany()
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Canal)
                .WithMany(e => e.Templates)
                .HasForeignKey(t => t.CanalId);

            // Índices
            builder.HasIndex(t => t.CategoriaId)
                .HasDatabaseName("IX_Templates_CategoriaId");

            builder.HasIndex(t => t.CanalId)
                .HasDatabaseName("IX_Templates_CanalId");

            builder.HasIndex(t => new { t.CanalId, t.Nome })
                .HasDatabaseName("IX_Templates_CanalId_Nome");
        }
    }
}

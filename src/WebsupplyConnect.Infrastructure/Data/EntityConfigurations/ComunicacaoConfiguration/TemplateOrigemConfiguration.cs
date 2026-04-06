using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class TemplateOrigemConfiguration : EntidadeBaseConfiguration<TemplateOrigem>
    {
        public override void Configure(EntityTypeBuilder<TemplateOrigem> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);
            // Tabela

            builder.ToTable("TemplateOrigens");

            // Propriedades específicas do TemplateOrigem
            builder.Property(to => to.TemplateId)
                .IsRequired();

            builder.Property(to => to.OrigemId)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(to => to.Template)
                .WithMany()
                .HasForeignKey(to => to.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(to => to.Origem)
                .WithMany() 
                .HasForeignKey(to => to.OrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(to => new { to.TemplateId, to.OrigemId })
                .HasDatabaseName("IX_TemplateOrigens_TemplateId_OrigemId")
                .IsUnique();
        }
    }
}

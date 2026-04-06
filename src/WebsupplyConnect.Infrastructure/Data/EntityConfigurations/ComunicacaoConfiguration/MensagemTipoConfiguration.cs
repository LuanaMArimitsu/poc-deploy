using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MensagemTipoConfiguration : EntidadeTipificacaoConfiguration<MensagemTipo>
    {
        public override void Configure(EntityTypeBuilder<MensagemTipo> builder)
        {
            // Chama a configuração base para EntidadeTipificacao (que já inclui EntidadeBase)
            base.Configure(builder);

            builder.Property(t => t.SuportaMidia)
                .IsRequired();

            builder.Property(t => t.RequerMidia)
                .IsRequired();

            builder.Property(d => d.Id)
              .ValueGeneratedNever() // Não usar identity para esse campo
              .IsRequired();

            // Índices específicos
            builder.HasIndex(t => t.SuportaMidia)
                .HasDatabaseName("IX_MensagemTipos_SuportaMidia");

            builder.HasIndex(t => t.RequerMidia)
                .HasDatabaseName("IX_MensagemTipos_RequerMidia");
        }
    }
}

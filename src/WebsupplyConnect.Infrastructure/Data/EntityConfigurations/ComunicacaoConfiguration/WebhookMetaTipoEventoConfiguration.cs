using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class WebhookMetaTipoEventoConfiguration : EntidadeTipificacaoConfiguration<WebhookMetaTipoEvento>
    {
        public override void Configure(EntityTypeBuilder<WebhookMetaTipoEvento> builder)
        {
            // Chama a configuração base para EntidadeTipificacao (que já inclui EntidadeBase)
            base.Configure(builder);

            builder.Property(d => d.Id)
                  .ValueGeneratedNever()
                  .IsRequired();
        }
    }
}

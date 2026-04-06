using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class ConversaStatusConfiguration : EntidadeTipificacaoConfiguration<ConversaStatus>
    {
        public override void Configure(EntityTypeBuilder<ConversaStatus> builder)
        {
            // Chama a configuração base para EntidadeTipificacao
            base.Configure(builder);


            // Índices específicos
            builder.HasIndex(s => s.Ordem)
                .HasDatabaseName("IX_ConversaStatus_Ordem");
        }
    }
}

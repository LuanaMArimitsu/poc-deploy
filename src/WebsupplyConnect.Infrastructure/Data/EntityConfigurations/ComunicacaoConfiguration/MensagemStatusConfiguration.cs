using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MensagemStatusConfiguration : EntidadeTipificacaoConfiguration<MensagemStatus>
    {
        public override void Configure(EntityTypeBuilder<MensagemStatus> builder)
        {
            // Chama a configuração base para EntidadeTipificacao (que já inclui EntidadeBase)
            base.Configure(builder);

            builder.Property(s => s.FinalStatus)
                .IsRequired();

            // Índices específicos
            // Índice para FinalStatus, útil para consultas que buscam status finais
            builder.HasIndex(s => s.FinalStatus)
                .HasDatabaseName("IX_MensagemStatus_FinalStatus");
        }
    }
}

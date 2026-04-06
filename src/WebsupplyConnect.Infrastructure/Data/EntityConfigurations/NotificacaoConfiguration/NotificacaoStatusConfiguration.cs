using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.NotificacaoConfiguration
{
    public class NotificacaoStatusConfiguration : EntidadeTipificacaoConfiguration<NotificacaoStatus>
    {
        public override void Configure(EntityTypeBuilder<NotificacaoStatus> builder)
        {
            base.Configure(builder);

            // Propriedades específicas
            builder.Property(s => s.StatusFinal)
                .IsRequired()
                .HasDefaultValue(false);

            // Índices adicionais
            builder.HasIndex(s => s.StatusFinal)
                .HasDatabaseName("IX_NotificacaoStatus_StatusFinal");
        }
    }
}

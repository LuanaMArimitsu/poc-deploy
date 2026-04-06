using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class WebhookMetaConfiguration : EntidadeBaseConfiguration<WebhookMeta>
    {
        public override void Configure(EntityTypeBuilder<WebhookMeta> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);

            // Tabela
            builder.ToTable("WebhooksMeta");

            // Propriedades específicas do WebhookMeta
            builder.Property(w => w.IdExterno)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.DataRegistro)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(w => w.Payload)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(w => w.AssinaturaHMAC)
                .HasMaxLength(200);

            builder.Property(w => w.WebhookMetaTipoEventoId)
                .IsRequired(false);

            builder.Property(w => w.Processado)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(w => w.ConversaId)
                .IsRequired(false); // Pode ser nulo antes do processamento

            builder.Property(w => w.TempoRespostaMs)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(w => w.Conversa)
                .WithMany(c => c.WebhooksMeta)
                .HasForeignKey(w => w.ConversaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Pode ser nulo antes do processamento

            builder.HasOne(w => w.WebhookMetaTipoEvento)
                .WithMany()
                .HasForeignKey(w => w.WebhookMetaTipoEventoId)
                .OnDelete(DeleteBehavior.Restrict);


            // Índices
            builder.HasIndex(w => w.IdExterno)
                .HasDatabaseName("IX_WebhooksMeta_IdExterno");

            builder.HasIndex(w => w.DataRegistro)
                .HasDatabaseName("IX_WebhooksMeta_DataRegistro");

            builder.HasIndex(w => w.WebhookMetaTipoEventoId)
                .HasDatabaseName("IX_WebhooksMeta_TipoEvento");

            builder.HasIndex(w => w.Processado)
                .HasDatabaseName("IX_WebhooksMeta_Processado");

            builder.HasIndex(w => w.ConversaId)
                .HasDatabaseName("IX_WebhooksMeta_ConversaId");

            // Índices compostos para consultas frequentes
            builder.HasIndex(w => new { w.Processado, w.DataRegistro })
                .HasDatabaseName("IX_WebhooksMeta_Processado_DataRegistro");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.NotificacaoConfiguration
{
    public class NotificacaoTipoConfiguration : EntidadeTipificacaoConfiguration<NotificacaoTipo>
    {
        public override void Configure(EntityTypeBuilder<NotificacaoTipo> builder)
        {
            base.Configure(builder);

            // Propriedades específicas
            builder.Property(nt => nt.Categoria)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("geral");

            builder.Property(nt => nt.OrigemSistema)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Sistema");

            builder.Property(nt => nt.AtivoParaWeb)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(nt => nt.AtivoParaMobile)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices adicionais
            builder.HasIndex(nt => nt.Categoria)
                .HasDatabaseName("IX_NotificacaoTipos_Categoria");

            builder.HasIndex(nt => nt.OrigemSistema)
                .HasDatabaseName("IX_NotificacaoTipos_OrigemSistema");

            builder.HasIndex(nt => new { nt.AtivoParaWeb, nt.AtivoParaMobile })
                .HasDatabaseName("IX_NotificacaoTipos_Plataformas");
        }
    }
}

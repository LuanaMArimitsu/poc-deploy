using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.NotificacaoConfiguration
{
    public class NotificacaoConfiguration : EntidadeBaseConfiguration<Notificacao>
    {
        public override void Configure(EntityTypeBuilder<Notificacao> builder)
        {
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("Notificacoes");

            // Propriedades obrigatórias
            builder.Property(n => n.Titulo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(n => n.Conteudo)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.DataHora)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(n => n.DataEnvio)
                .HasColumnType("datetime2");

            builder.Property(n => n.DataVisualizacao)
                .HasColumnType("datetime2");

            builder.Property(n => n.UsuarioDestinatarioId)
                .IsRequired();

            builder.Property(n => n.UsuarioRemetenteId);

            builder.Property(n => n.NotificacaoTipoId)
                .IsRequired();

            builder.Property(n => n.StatusId)
                .IsRequired();

            builder.Property(n => n.EntidadeAlvoId);

            builder.Property(n => n.TipoEntidadeAlvo)
                .HasMaxLength(50)
                .HasDefaultValue(string.Empty);

            // Propriedades booleanas de controle de envio
            builder.Property(n => n.EnviadoPush)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.EnviadaSignalR)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.EnviadaEmail)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.ExibidaWeb)
                .IsRequired()
                .HasDefaultValue(false);

            // Relacionamentos
            builder.HasOne(n => n.UsuarioDestinatario)
                .WithMany() // Não criamos navegação reversa para evitar circular reference
                .HasForeignKey(n => n.UsuarioDestinatarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Notificacoes_UsuarioDestinatario");

            builder.HasOne(n => n.UsuarioRemetente)
                .WithMany() // Não criamos navegação reversa para evitar circular reference
                .HasForeignKey(n => n.UsuarioRemetenteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Notificacoes_UsuarioRemetente");

            builder.HasOne(n => n.NotificacaoTipo)
                .WithMany(nt => nt.Notificacoes)
                .HasForeignKey(n => n.NotificacaoTipoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Notificacoes_NotificacaoTipo");

            builder.HasOne(n => n.Status)
                .WithMany(s => s.Notificacoes)
                .HasForeignKey(n => n.StatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Notificacoes_Status");

            // Índices para performance
            builder.HasIndex(n => n.UsuarioDestinatarioId)
                .HasDatabaseName("IX_Notificacoes_UsuarioDestinatario");

            builder.HasIndex(n => n.DataHora)
                .HasDatabaseName("IX_Notificacoes_DataHora");

            builder.HasIndex(n => n.StatusId)
                .HasDatabaseName("IX_Notificacoes_Status");

            builder.HasIndex(n => n.NotificacaoTipoId)
                .HasDatabaseName("IX_Notificacoes_Tipo");

            builder.HasIndex(n => new { n.UsuarioDestinatarioId, n.DataVisualizacao })
                .HasDatabaseName("IX_Notificacoes_Usuario_Visualizacao");

            builder.HasIndex(n => new { n.EntidadeAlvoId, n.TipoEntidadeAlvo })
                .HasDatabaseName("IX_Notificacoes_EntidadeAlvo");

            // Índice composto para consultas frequentes
            builder.HasIndex(n => new { n.UsuarioDestinatarioId, n.StatusId, n.DataHora })
                .HasDatabaseName("IX_Notificacoes_Usuario_Status_Data");
        }
    }
}

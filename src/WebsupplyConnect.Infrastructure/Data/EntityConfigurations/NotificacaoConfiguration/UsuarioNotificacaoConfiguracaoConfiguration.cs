using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.NotificacaoConfiguration
{
    public class UsuarioNotificacaoConfiguracaoConfiguration : EntidadeBaseConfiguration<UsuarioNotificacaoConfiguracao>
    {
        public override void Configure(EntityTypeBuilder<UsuarioNotificacaoConfiguracao> builder)
        {
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("UsuarioNotificacaoConfiguracoes");

            // Propriedades obrigatórias
            builder.Property(unc => unc.UsuarioId)
                .IsRequired();

            builder.Property(unc => unc.ReceberPush)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(unc => unc.ReceberSignalR)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(unc => unc.ReceberEmail)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(unc => unc.ExibirNaWeb)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(unc => unc.HorarioInicio)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(unc => unc.HorarioFim)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(unc => unc.ReceberFinalSemana)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(unc => unc.IntervalMinimoMinutos)
                .IsRequired()
                .HasDefaultValue(5);

            // Relacionamento com Usuario
            builder.HasOne(unc => unc.Usuario)
                .WithOne() // Relacionamento 1:1 - um usuário tem uma configuração
                .HasForeignKey<UsuarioNotificacaoConfiguracao>(unc => unc.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict) 
                .HasConstraintName("FK_UsuarioNotificacaoConfiguracoes_Usuario");

            // Índices
            builder.HasIndex(unc => unc.UsuarioId)
                .IsUnique() // Garante que cada usuário tenha apenas uma configuração
                .HasDatabaseName("IX_UsuarioNotificacaoConfiguracoes_UsuarioId");

            // Índices para consultas de filtro de horário
            builder.HasIndex(unc => new { unc.HorarioInicio, unc.HorarioFim })
                .HasDatabaseName("IX_UsuarioNotificacaoConfiguracoes_Horarios");

            builder.HasIndex(unc => unc.ReceberFinalSemana)
                .HasDatabaseName("IX_UsuarioNotificacaoConfiguracoes_FinalSemana");

            // Índice composto para filtros de preferências de canal
            builder.HasIndex(unc => new { unc.ReceberPush, unc.ReceberSignalR, unc.ReceberEmail, unc.ExibirNaWeb })
                .HasDatabaseName("IX_UsuarioNotificacaoConfiguracoes_Canais");
        }
    }
}

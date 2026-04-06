using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.UserConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade Dispositivo
    /// </summary>
    public class DispositivoConfiguration : EntidadeBaseConfiguration<Dispositivo>
    {
        public override void Configure(EntityTypeBuilder<Dispositivo> builder)
        {
            // Chama a configuração da classe base primeiro (Id, DataCriacao, DataModificacao, Excluido, etc.)
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("Dispositivos");

            // Configurações de propriedades específicas
            builder.Property(d => d.UsuarioId)
                .IsRequired();

            builder.Property(d => d.DeviceId)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(d => d.Modelo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(d => d.UltimaSincronizacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(d => d.SignalRConnectionId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(d => d.UltimoHeartbeatSignalR)
                .HasColumnType("datetime2");

            builder.Property(d => d.UltimaReconexao)
                .HasColumnType("datetime2");

            // Relacionamentos

            // Relação com Usuario (muitos para um)
            builder.HasOne(d => d.Usuario)
                .WithMany(u => u.Dispositivos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices

            // Índice único para DeviceId e UsuarioId
            builder.HasIndex(d => new { d.DeviceId, d.UsuarioId })
                .IsUnique()
                .HasFilter("[Excluido] = 0");

            // Índice para pesquisas por UsuarioId
            builder.HasIndex(d => d.UsuarioId);

            // Índice para pesquisas por status (ativo/inativo)
            builder.HasIndex(d => d.Ativo);
        }
    }
}

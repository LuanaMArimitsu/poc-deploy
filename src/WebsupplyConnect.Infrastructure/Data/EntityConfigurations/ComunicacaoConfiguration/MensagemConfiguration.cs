using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MensagemConfiguration : EntidadeBaseConfiguration<Mensagem>
    {
        public override void Configure(EntityTypeBuilder<Mensagem> builder)
        {
            // Chama a configuração base para EntidadeSincronizavel (que já inclui EntidadeBase)
            base.Configure(builder);

            // Tabela
            builder.ToTable("Mensagens");

            // Propriedades específicas da Mensagem
            builder.Property(m => m.ConversaId)
                .IsRequired();

            builder.Property(m => m.Conteudo)
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.Sentido)
                .IsRequired();

            builder.Property(m => m.UsuarioId);

            builder.Property(m => m.DataRecebimento);

            builder.Property(m => m.DataLeitura);

            builder.Property(m => m.IdExternoMeta)
                .HasMaxLength(100);

            builder.Property(m => m.StatusId);

            builder.Property(m => m.TipoId)
                .IsRequired();

            builder.Property(m => m.TemplateId);

            builder.Property(m => m.Destacada)
                .IsRequired();

            builder.Property(m => m.UsouAssistenteIA)
                .IsRequired();


            // Relacionamentos
            builder.HasOne(m => m.Conversa)
                .WithMany(c => c.Mensagens)
                .HasForeignKey(m => m.ConversaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Tipo)
                .WithMany()
                .HasForeignKey(m => m.TipoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Status)
                .WithMany()
                .HasForeignKey(m => m.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Template)
                .WithMany(t => t.Mensagens)
                .HasForeignKey(m => m.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Usuario)
                .WithMany(u => u.Mensagens)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices específicos (DataUltimaModificacao já é configurado na classe base)
            builder.HasIndex(m => m.ConversaId)
                .HasDatabaseName("IX_Mensagens_ConversaId");

            builder.HasIndex(m => m.UsuarioId)
                .HasDatabaseName("IX_Mensagens_UsuarioId");

            builder.HasIndex(m => m.StatusId)
                .HasDatabaseName("IX_Mensagens_StatusId");

            builder.HasIndex(m => m.TipoId)
                .HasDatabaseName("IX_Mensagens_TipoId");

            builder.HasIndex(m => m.DataEnvio)
                .HasDatabaseName("IX_Mensagens_DataEnvio");

            builder.HasIndex(m => m.IdExternoMeta)
                .HasDatabaseName("IX_Mensagens_IdExternoMeta");

            builder.HasIndex(m => m.Destacada)
                .HasDatabaseName("IX_Mensagens_Destacada");

            builder.HasIndex(m => m.Sentido)
                .HasDatabaseName("IX_Mensagens_Sentido");
        }
    }
}

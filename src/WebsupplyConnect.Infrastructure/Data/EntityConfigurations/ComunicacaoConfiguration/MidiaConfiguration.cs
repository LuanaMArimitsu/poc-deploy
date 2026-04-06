using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MidiaConfiguration : EntidadeBaseConfiguration<Midia>
    {
        public override void Configure(EntityTypeBuilder<Midia> builder)
        {
            // Chama a configuração base para EntidadeSincronizavel (que já inclui EntidadeBase)
            base.Configure(builder);

            // Tabela
            builder.ToTable("Midias");

            // Propriedades específicas da Midia
            builder.Property(m => m.MensagemId)
                .IsRequired();

            builder.Property(m => m.Caption);

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(m => m.Formato)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.TamanhoBytes);

            builder.Property(m => m.UrlStorage)
                .HasMaxLength(1000);

            builder.Property(m => m.ThumbnailUrlStorage)
                .HasMaxLength(1000);


            builder.Property(m => m.IdExternoMeta)
                .HasMaxLength(100);

            builder.Property(m => m.BlobId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.ContainerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.MidiaStatusProcessamentoId)
                .IsRequired();

            builder.Property(m => m.Transcricao)
                .HasMaxLength(5000);

            // Relacionamentos
            builder.HasOne(m => m.Mensagem)
                   .WithOne(msg => msg.Midia)
                   .HasForeignKey<Midia>(m => m.MensagemId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(m => m.MidiaStatusProcessamento)
                .WithMany()
                .HasForeignKey(m => m.MidiaStatusProcessamentoId)
                .OnDelete(DeleteBehavior.Restrict);


            // Índices específicos (DataUltimaModificacao já é configurado na classe base)
            builder.HasIndex(m => m.MensagemId)
                .IsUnique()
                .HasFilter("[MensagemId] IS NOT NULL")
                .HasDatabaseName("IX_Midias_MensagemId");

            builder.HasIndex(m => m.MidiaStatusProcessamentoId)
                .HasDatabaseName("IX_Midias_MidiaStatusProcessamentoId");

            builder.HasIndex(m => m.BlobId)
                .HasDatabaseName("IX_Midias_BlobId");

            builder.HasIndex(m => m.IdExternoMeta)
                .HasDatabaseName("IX_Midias_IdExternoMeta");

            builder.HasIndex(m => m.Formato)
                .HasDatabaseName("IX_Midias_Formato");
        }
    }
}

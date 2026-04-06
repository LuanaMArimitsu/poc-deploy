using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MensagemSugestaoConfiguration : IEntityTypeConfiguration<MensagemSugestao>
    {
        public void Configure(EntityTypeBuilder<MensagemSugestao> builder)
        {
            // Tabela e chave primária
            builder.ToTable("MensagensSugestoes");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).UseIdentityColumn();

            // Propriedades específicas da MensagemSugestao
            builder.Property(s => s.MensagemId)
                .IsRequired();

            builder.Property(s => s.TextoOriginal)
                .IsRequired()
                .HasColumnType("nvarchar(700)");

            builder.Property(s => s.TextoSugerido)
                .IsRequired()
                .HasColumnType("nvarchar(700)");

            builder.Property(s => s.Tipo)
                .HasMaxLength(50);

            builder.Property(s => s.Foco)
                .HasMaxLength(50);

            builder.Property(s => s.Selecionada)
                .IsRequired();

            builder.Property(s => s.Pontuacao)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(s => s.Mensagem)
                .WithMany(m => m.Sugestoes)
                .HasForeignKey(s => s.MensagemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(s => s.MensagemId);
            builder.HasIndex(s => s.Selecionada);
            builder.HasIndex(s => s.Pontuacao);
            builder.HasIndex(s => s.Tipo);
            builder.HasIndex(s => new { s.MensagemId, s.Selecionada }); // Composto para consultas comuns
        }
    }
}

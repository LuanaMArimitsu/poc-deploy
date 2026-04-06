using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MensagemSugestaoFeedbackConfiguration : IEntityTypeConfiguration<MensagemSugestaoFeedback>
    {
        public void Configure(EntityTypeBuilder<MensagemSugestaoFeedback> builder)
        {
            // Tabela e chave primária
            builder.ToTable("MensagensSugestoesFeedbacks");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).UseIdentityColumn();

            // Propriedades específicas da MensagemSugestaoFeedback
            builder.Property(f => f.SugestaoId)
                .IsRequired();

            builder.Property(f => f.UsuarioId)
                .IsRequired();

            builder.Property(f => f.Positivo)
                .IsRequired();

            builder.Property(f => f.Comentario)
                .HasMaxLength(500);

            builder.Property(f => f.DataFeedback)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(f => f.Sugestao)
                .WithMany(s => s.Feedbacks)
                .HasForeignKey(f => f.SugestaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(f => f.SugestaoId);
            builder.HasIndex(f => f.UsuarioId);
            builder.HasIndex(f => f.Positivo);
            builder.HasIndex(f => f.DataFeedback);

            // Índice único para garantir que um usuário não forneça múltiplos feedbacks para a mesma sugestão
            builder.HasIndex(f => new { f.SugestaoId, f.UsuarioId }).IsUnique();
        }
    }
}

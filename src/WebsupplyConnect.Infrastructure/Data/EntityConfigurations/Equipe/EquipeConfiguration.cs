using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Infrastructure.Data.Configurations
{
    public class EquipeConfiguration : IEntityTypeConfiguration<Equipe>
    {
        public void Configure(EntityTypeBuilder<Equipe> builder)
        {
            builder.ToTable(nameof(Equipe));
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nome)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Descricao)
                   .HasMaxLength(500);

            builder.Property(e => e.Ativa)
                   .IsRequired();

            builder.Property(e => e.DataCriacao).IsRequired();
            builder.Property(e => e.DataModificacao).IsRequired();

            builder.Property(e => e.NotificarAtribuicaoAoDestinatario)
                    .IsRequired()
                    .HasDefaultValue(false);

            builder.Property(e => e.NotificarAtribuicaoAosLideres)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(e => e.NotificarSemAtendimentoLideres)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(e => e.TempoMaxSemAtendimento)
                   .IsRequired(false)
                   .HasColumnType("time");

            builder.Property(e => e.TempoMaxDuranteAtendimento)
               .IsRequired(false)
               .HasColumnType("time");

            builder.HasIndex(e => e.Nome);
            builder.HasIndex(e => new { e.EmpresaId, e.TipoEquipeId });
            
            builder.HasOne(e => e.TipoEquipe)
                   .WithMany(t => t.Equipes)
                   .HasForeignKey(e => e.TipoEquipeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Empresa)
                   .WithMany(e => e.Equipes) 
                   .HasForeignKey(e => e.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ResponsavelMembro)
                   .WithMany() // não mapeado no outro lado
                   .HasForeignKey(e => e.ResponsavelMembroId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            // NÃO configure .HasMany(e => e.Membros) aqui.
            // O lado dependente (MembroEquipe) vai declarar o relacionamento.
        }
    }
}

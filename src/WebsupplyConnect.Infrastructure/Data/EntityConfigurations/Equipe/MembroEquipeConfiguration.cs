using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Infrastructure.Data.Configurations
{
    public class MembroEquipeConfiguration : IEntityTypeConfiguration<MembroEquipe>
    {
        public void Configure(EntityTypeBuilder<MembroEquipe> builder)
        {
            builder.ToTable(nameof(MembroEquipe));
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Observacoes)
                   .HasMaxLength(1024);

            builder.Property(m => m.IsLider).IsRequired();

            builder.Property(m => m.DataCriacao).IsRequired();
            builder.Property(m => m.DataModificacao).IsRequired();

            builder.HasIndex(m => new { m.EquipeId, m.UsuarioId });
            builder.HasIndex(m => m.StatusMembroEquipeId);

            // RELACIONAMENTOS (somente aqui; não repetir em Equipe/Status/Usuario)
            builder.HasOne(m => m.Equipe)
                   .WithMany(e => e.Membros)
                   .HasForeignKey(m => m.EquipeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Usuario)
                   .WithMany(u => u.MembrosEquipe)
                   .HasForeignKey(m => m.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(m => m.StatusMembroEquipe)
                   .WithMany(s => s.Membros)
                   .HasForeignKey(m => m.StatusMembroEquipeId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}

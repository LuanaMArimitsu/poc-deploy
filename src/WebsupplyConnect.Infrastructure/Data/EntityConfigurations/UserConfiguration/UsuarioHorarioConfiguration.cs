using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.UserConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade UsuarioHorario
    /// </summary>
    public class UsuarioHorarioConfiguration : IEntityTypeConfiguration<UsuarioHorario>
    {
        public void Configure(EntityTypeBuilder<UsuarioHorario> builder)
        {
            // Configuração da tabela
            builder.ToTable("UsuariosHorarios");

            // Chave primária
            builder.HasKey(uh => uh.Id);

            builder.Property(eu => eu.Id)
                .IsRequired()
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // Diz ao EF: "deixa o banco gerar"
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            // Configurações de propriedades
            builder.Property(uh => uh.UsuarioId)
                .IsRequired();

            builder.Property(uh => uh.DiaSemanaId)
                .IsRequired();

            builder.Property(uh => uh.HorarioInicio)
                .IsRequired()
                .HasColumnType("time");

            builder.Property(uh => uh.HorarioFim)
                .IsRequired()
                .HasColumnType("time");

            // Configuração de relacionamentos

            // Relacionamento com Usuario (muitos para um)
            builder.HasOne(uh => uh.Usuario)
                .WithMany(u => u.HorariosUsuario)
                .HasForeignKey(uh => uh.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com DiaSemana (um para um)
            builder.HasOne(uh => uh.DiaSemana)
                .WithMany()
                .HasForeignKey(uh => uh.DiaSemanaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices

            // Índice composto para garantir que um usuário não tenha horários duplicados para o mesmo dia da semana
            builder.HasIndex(uh => new { uh.UsuarioId, uh.DiaSemanaId })
                .IsUnique();

            // Índice para melhorar a performance na busca de horários por usuário
            builder.HasIndex(uh => uh.UsuarioId);

            // Índice para melhorar a performance na busca de todos os horários de um dia específico
            builder.HasIndex(uh => uh.DiaSemanaId);
        
        }
    }
}

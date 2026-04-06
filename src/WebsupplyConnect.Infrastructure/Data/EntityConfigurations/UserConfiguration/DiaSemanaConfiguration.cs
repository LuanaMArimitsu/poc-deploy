using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.UserConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade DiaSemana
    /// </summary>
    public class DiaSemanaConfiguration : IEntityTypeConfiguration<DiaSemana>
    {
        public void Configure(EntityTypeBuilder<DiaSemana> builder)
        {
            // Configuração da tabela
            builder.ToTable("DiasSemana");

            // Chave primária
            builder.HasKey(d => d.Id);

            // Configuração de propriedades

            // ID não é identity column, pois representa um conceito fixo (1=Dom, 2=Seg, etc.)
            builder.Property(d => d.Id)
                .ValueGeneratedNever() // Não usar identity para esse campo
                .IsRequired();

            builder.Property(d => d.Descricao)
                .IsRequired()
                .HasMaxLength(20);

        }
    }
}

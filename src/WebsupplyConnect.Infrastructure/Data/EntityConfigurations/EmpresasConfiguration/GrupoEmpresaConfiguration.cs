using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Empresa;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.EmpresasConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade GrupoEmpresa
    /// </summary>
    public class GrupoEmpresaConfiguration : IEntityTypeConfiguration<GrupoEmpresa>
    {
        public void Configure(EntityTypeBuilder<GrupoEmpresa> builder)
        {
            // Configuração da tabela
            builder.ToTable("GruposEmpresa");

            // Configuração da chave primária
            builder.HasKey(g => g.Id);

            builder.Property(d => d.Id)
                .IsRequired()
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // <- reforça que o banco deve gerar o valor
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);               

            // Configuração das propriedades da entidade
            builder.Property(g => g.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.CnpjHolding)
                .HasMaxLength(14);

            builder.Property(g => g.Ativo)
                .IsRequired();

            builder.Property(g => g.Logo)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(d => d.Id)
                .ValueGeneratedNever() // Não usar identity para esse campo
                .IsRequired();

            builder.HasIndex(g => g.Ativo);
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.EmpresasConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade Empresa
    /// </summary>
    public class EmpresaConfiguration : EntidadeBaseConfiguration<Empresa>
    {
        public override void Configure(EntityTypeBuilder<Empresa> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("Empresas");

            // Configuração das propriedades da entidade
            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.RazaoSocial)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Cnpj)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(e => e.Telefone)
                .HasMaxLength(15);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.Ativo)
                .IsRequired();

            builder.Property(e => e.ConfiguracaoIntegracao)
                .IsRequired(true);  // Pode ser nulo se não houver configuração de integração

            builder.Property(e => e.PossuiIntegracaoNBS)
                .IsRequired();


            // Configuração do relacionamento com GrupoEmpresa (muitos para um)
            builder.HasOne(e => e.GrupoEmpresa)
                .WithMany(ge => ge.Empresas)  
                .HasForeignKey(e => e.GrupoEmpresaId)
                .OnDelete(DeleteBehavior.Restrict)  
                .IsRequired();

            // Configuração do relacionamento com PromptEmpresas (um para muitos)
            builder.HasMany(e => e.Prompts)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

            // Índices para melhorar a performance das consultas

            builder.HasIndex(e => e.GrupoEmpresaId);  // Índice para melhorar consultas por grupo empresarial

            builder.HasIndex(e => e.Ativo);  // Índice para filtrar por status de ativação
        }
    }
}

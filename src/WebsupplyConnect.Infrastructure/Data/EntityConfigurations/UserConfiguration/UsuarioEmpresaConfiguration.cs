using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.UserConfiguration
{

    // <summary>
    /// Configuração do Entity Framework Core para a entidade UsuarioEmpresa
    /// </summary>
    public class UsuarioEmpresaConfiguration : IEntityTypeConfiguration<UsuarioEmpresa>
    {
        public void Configure(EntityTypeBuilder<UsuarioEmpresa> builder)
        {
            // Configuração da tabela
            builder.ToTable("UsuariosEmpresas");

            // Chave primária
            builder.HasKey(ue => ue.Id);

            builder.Property(eu => eu.Id)
                .IsRequired()
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // Diz ao EF: "deixa o banco gerar"
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            // Configurações de propriedades
            builder.Property(ue => ue.UsuarioId)
                .IsRequired();

            builder.Property(ue => ue.EmpresaId)
                .IsRequired();

            builder.Property(ue => ue.IsPrincipal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ue => ue.DataAssociacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(c => c.CodVendedorNBS)
                .HasColumnType("nvarchar(50)");

            builder.Property(ue => ue.EquipePadraoId)
                .IsRequired(false);

            // Configuração de relacionamentos

            // Relacionamento com Usuario (muitos para um)
            builder.HasOne(ue => ue.Usuario)
                .WithMany(u => u.UsuarioEmpresas)
                .HasForeignKey(ue => ue.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com Empresa (muitos para um)
            builder.HasOne(ue => ue.Empresa)
                .WithMany(e => e.UsuarioEmpresas)
                .HasForeignKey(ue => ue.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(ue => ue.CanalPadrao)
                .WithMany(c => c.UsuarioEmpresas)
                .HasForeignKey(ue => ue.CanalPadraoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(ue => ue.EquipePadrao)
                .WithMany(e => e.UsuarioEmpresas)
                .HasForeignKey(ue => ue.EquipePadraoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Índices

            // Índice composto para garantir que um usuário não tenha duplicatas da mesma empresa
            builder.HasIndex(ue => new { ue.UsuarioId, ue.EmpresaId })
                .IsUnique();

            // Índice para melhorar a performance na busca de empresas principais de usuários
            builder.HasIndex(ue => new { ue.UsuarioId, ue.IsPrincipal });

            // Índice para buscar todos os usuários de uma empresa específica
            builder.HasIndex(ue => ue.EmpresaId);

            // Buscar todos os usuários de uma equipe específica
            builder.HasIndex(ue => ue.EquipePadraoId);

            // Buscar usuários por empresa e equipe para distribuição de leads
            builder.HasIndex(ue => new { ue.EmpresaId, ue.EquipePadraoId });
        }
    }
}


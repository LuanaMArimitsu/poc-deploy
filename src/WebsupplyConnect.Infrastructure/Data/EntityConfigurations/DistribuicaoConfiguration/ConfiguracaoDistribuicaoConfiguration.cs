using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    public class ConfiguracaoDistribuicaoConfiguration : EntidadeBaseConfiguration<ConfiguracaoDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<ConfiguracaoDistribuicao> builder)
        {
            base.Configure(builder);
            
            // Configuração da tabela
            builder.ToTable("ConfiguracoesDistribuicao");
            
            // Propriedades
            builder.Property(c => c.EmpresaId).IsRequired();
            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(c => c.Descricao)
                .HasMaxLength(500);
            builder.Property(c => c.Ativo).IsRequired();
            builder.Property(c => c.DataInicioVigencia)
                .HasColumnType("datetime2");
            builder.Property(c => c.DataFimVigencia)
                .HasColumnType("datetime2");
            builder.Property(c => c.PermiteAtribuicaoManual).IsRequired();
            builder.Property(c => c.ConsiderarHorarioTrabalho).IsRequired();
            builder.Property(c => c.ConsiderarFeriados).IsRequired();
            builder.Property(c => c.ParametrosGerais)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            
            // Configuração de navegação para Empresa
            builder.HasOne(c => c.Empresa)
                .WithMany()
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // IMPORTANTE: Configuração explícita das coleções para evitar shadow properties
            builder.HasMany(c => c.Regras)
                .WithOne(r => r.ConfiguracaoDistribuicao)
                .HasForeignKey(r => r.ConfiguracaoDistribuicaoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(c => c.Historicos)
                .WithOne(h => h.ConfiguracaoDistribuicao)
                .HasForeignKey(h => h.ConfiguracaoDistribuicaoId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasMany(c => c.Atribuicoes)
                .WithOne(a => a.ConfiguracaoDistribuicao)
                .HasForeignKey(a => a.ConfiguracaoDistribuicaoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
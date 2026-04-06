using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    public class AtribuicaoLeadConfiguration : EntidadeBaseConfiguration<AtribuicaoLead>
    {
        public override void Configure(EntityTypeBuilder<AtribuicaoLead> builder)
        {
            base.Configure(builder);
            
            // Configuração da tabela
            builder.ToTable("AtribuicoesLead");
            
            // Propriedades obrigatórias
            builder.Property(a => a.LeadId).IsRequired();
            builder.Property(a => a.MembroAtribuidoId).IsRequired();
            builder.Property(a => a.TipoAtribuicaoId).IsRequired();
            builder.Property(a => a.DataAtribuicao)
                .IsRequired()
                .HasColumnType("datetime2");
            builder.Property(a => a.MotivoAtribuicao)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(a => a.AtribuicaoAutomatica).IsRequired();
            builder.Property(a => a.ParametrosAplicados)
                .HasColumnType("nvarchar(max)");
            builder.Property(a => a.VendedoresElegiveis)
                .HasColumnType("nvarchar(max)");
            builder.Property(a => a.ScoresCalculados)
                .HasColumnType("nvarchar(max)");
            builder.Property(a => a.ScoreVendedor)
                .HasPrecision(18, 4);
        
            // Configuração das relações
            builder.HasOne(a => a.ConfiguracaoDistribuicao)
                .WithMany(c => c.Atribuicoes)
                .HasForeignKey(a => a.ConfiguracaoDistribuicaoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Importante: Use a coleção Atribuicoes definida em RegraDistribuicao
            builder.HasOne(a => a.RegraDistribuicao)
                .WithMany(r => r.Atribuicoes)
                .HasForeignKey(a => a.RegraDistribuicaoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(a => a.Lead)
                .WithMany()
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(a => a.MembroAtribuido)
                .WithMany()
                .HasForeignKey(a => a.MembroAtribuidoId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(a => a.MembroAtribuiu)
                .WithMany()
                .HasForeignKey(a => a.MembroAtribuiuId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(a => a.LeadStatusHistorico)
                .WithMany()
                .HasForeignKey(a => a.LeadStatusHistoricoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(a => a.TipoAtribuicao)
                .WithMany(t => t.AtribuicoesLead)
                .HasForeignKey(a => a.TipoAtribuicaoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
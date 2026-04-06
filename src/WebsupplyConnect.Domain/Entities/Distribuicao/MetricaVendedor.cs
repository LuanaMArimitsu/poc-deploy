using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Armazena as métricas de performance de um vendedor para uso no sistema de distribuição.
    /// As métricas são calculadas periodicamente e usadas para determinar o score de mérito.
    /// </summary>
    public class MetricaVendedor : EntidadeBase
    {
        /// <summary>
        /// ID do usuário (vendedor) ao qual as métricas pertencem
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// ID da empresa para a qual as métricas foram calculadas
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Taxa de conversão percentual (leads convertidos / total recebidos)
        /// </summary>
        public decimal TaxaConversao { get; private set; }

        /// <summary>
        /// Velocidade média de primeiro atendimento em minutos
        /// </summary>
        public decimal VelocidadeMediaAtendimento { get; private set; }

        /// <summary>
        /// Taxa percentual de leads perdidos por inatividade
        /// </summary>
        public decimal TaxaPerdaInatividade { get; private set; }

        /// <summary>
        /// Total de leads recebidos no período
        /// </summary>
        public int TotalLeadsRecebidos { get; private set; }

        /// <summary>
        /// Total de leads convertidos em vendas
        /// </summary>
        public int TotalLeadsConvertidos { get; private set; }

        /// <summary>
        /// Total de leads perdidos (não convertidos)
        /// </summary>
        public int TotalLeadsPerdidos { get; private set; }

        /// <summary>
        /// Quantidade de leads ativos atualmente sob responsabilidade do vendedor
        /// </summary>
        public int LeadsAtivosAtual { get; private set; }

        /// <summary>
        /// Data de início do período de medição
        /// </summary>
        public DateTime DataInicioMedicao { get; private set; }

        /// <summary>
        /// Data da última atualização das métricas
        /// </summary>
        public DateTime DataUltimaAtualizacao { get; private set; }

        /// <summary>
        /// Score geral calculado com base nas métricas (0-100)
        /// </summary>
        public decimal ScoreGeral { get; private set; }

        /// <summary>
        /// JSON com métricas detalhadas adicionais
        /// </summary>
        public string MetricasDetalhadas { get; private set; }

        // Propriedades de navegação
        public virtual Usuario.Usuario Usuario { get; private set; }
        public virtual Empresa.Empresa Empresa { get; private set; }

        // Construtor para EF Core
        protected MetricaVendedor() { }

        /// <summary>
        /// Cria uma nova métrica de vendedor
        /// </summary>
        public MetricaVendedor(
            int usuarioId,
            int empresaId,
            DateTime dataInicioMedicao,
            decimal taxaConversao = 0,
            decimal velocidadeMediaAtendimento = 0,
            decimal taxaPerdaInatividade = 0,
            int totalLeadsRecebidos = 0,
            int totalLeadsConvertidos = 0,
            int totalLeadsPerdidos = 0,
            int leadsAtivosAtual = 0,
            decimal scoreGeral = 0,
            string metricasDetalhadas = null)
        {
            ValidarParametros(
                usuarioId, 
                empresaId, 
                taxaConversao, 
                velocidadeMediaAtendimento, 
                taxaPerdaInatividade);

            UsuarioId = usuarioId;
            EmpresaId = empresaId;
            DataInicioMedicao = dataInicioMedicao;
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
            TaxaConversao = taxaConversao;
            VelocidadeMediaAtendimento = velocidadeMediaAtendimento;
            TaxaPerdaInatividade = taxaPerdaInatividade;
            TotalLeadsRecebidos = totalLeadsRecebidos;
            TotalLeadsConvertidos = totalLeadsConvertidos;
            TotalLeadsPerdidos = totalLeadsPerdidos;
            LeadsAtivosAtual = leadsAtivosAtual;
            ScoreGeral = scoreGeral;
            MetricasDetalhadas = metricasDetalhadas ?? "{}";
        }

        /// <summary>
        /// Incrementa o contador de leads recebidos
        /// </summary>
        public void IncrementarLeadsRecebidos()
        {
            TotalLeadsRecebidos++;
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Incrementa o contador de leads ativos
        /// </summary>
        public void IncrementarLeadsAtivos()
        {
            LeadsAtivosAtual++;
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Decrementa o contador de leads ativos
        /// </summary>
        /// <param name="quantidade">Quantidade a decrementar (padrão: 1)</param>
        public void DecrementarLeadsAtivos(int quantidade = 1)
        {
            if (quantidade <= 0)
                throw new DomainException("A quantidade a decrementar deve ser maior que zero", nameof(MetricaVendedor));
            
            LeadsAtivosAtual = Math.Max(0, LeadsAtivosAtual - quantidade);
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Incrementa o contador de leads convertidos e recalcula a taxa de conversão
        /// </summary>
        /// <param name="quantidade">Quantidade a incrementar (padrão: 1)</param>
        public void IncrementarConversoes(int quantidade = 1)
        {
            if (quantidade <= 0)
                throw new DomainException("A quantidade de conversões deve ser maior que zero", nameof(MetricaVendedor));
            
            TotalLeadsConvertidos += quantidade;
            RecalcularTaxaConversao();
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Incrementa o contador de leads perdidos e recalcula a taxa de perdas
        /// </summary>
        /// <param name="quantidade">Quantidade a incrementar (padrão: 1)</param>
        public void IncrementarPerdas(int quantidade = 1)
        {
            if (quantidade <= 0)
                throw new DomainException("A quantidade de perdas deve ser maior que zero", nameof(MetricaVendedor));
            
            TotalLeadsPerdidos += quantidade;
            RecalcularTaxaPerda();
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza a velocidade média de atendimento
        /// </summary>
        /// <param name="tempoAtendimentoMinutos">Tempo de atendimento em minutos</param>
        public void AtualizarVelocidadeMedia(decimal tempoAtendimentoMinutos)
        {
            if (tempoAtendimentoMinutos < 0)
                throw new DomainException("Tempo de atendimento deve ser não-negativo", nameof(MetricaVendedor));

            // Se for o primeiro atendimento, simplesmente define a velocidade
            if (TotalLeadsRecebidos <= 1)
            {
                VelocidadeMediaAtendimento = tempoAtendimentoMinutos;
            }
            else
            {
                // Atualiza a média ponderada
                VelocidadeMediaAtendimento = ((VelocidadeMediaAtendimento * (TotalLeadsRecebidos - 1)) + tempoAtendimentoMinutos) / TotalLeadsRecebidos;
            }

            // Recalcular score geral que depende da velocidade média
            RecalcularScoreGeral();
            
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Recalcula a taxa de conversão com base nos contadores atuais
        /// </summary>
        public void RecalcularTaxaConversao()
        {
            if (TotalLeadsRecebidos > 0)
            {
                TaxaConversao = (decimal)TotalLeadsConvertidos / TotalLeadsRecebidos * 100;
            }
            else
            {
                TaxaConversao = 0;
            }

            RecalcularScoreGeral();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Recalcula a taxa de perdas por inatividade com base nos contadores atuais
        /// </summary>
        public void RecalcularTaxaPerda()
        {
            if (TotalLeadsRecebidos > 0)
            {
                TaxaPerdaInatividade = (decimal)TotalLeadsPerdidos / TotalLeadsRecebidos * 100;
            }
            else
            {
                TaxaPerdaInatividade = 0;
            }

            RecalcularScoreGeral();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Recalcula o score geral com base em todas as métricas atuais
        /// </summary>
        public void RecalcularScoreGeral()
        {
            // Cálculo de score para velocidade (menor é melhor)
            decimal scoreVelocidade = Math.Max(0, 100 - Math.Min(100, VelocidadeMediaAtendimento / 10));
            
            // Cálculo de score geral ponderado pelos três fatores principais
            ScoreGeral = (TaxaConversao * 0.5m) + 
                         (scoreVelocidade * 0.3m) + 
                         ((100 - TaxaPerdaInatividade) * 0.2m);
            
            // Garantir que o score esteja no intervalo [0, 100]
            ScoreGeral = Math.Min(100, Math.Max(0, ScoreGeral));
            
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza a data de última atualização
        /// </summary>
        public void AtualizarDataAtualizacao()
        {
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }
        
        /// <summary>
        /// Atualiza a quantidade de leads ativos
        /// </summary>
        /// <param name="quantidade">Nova quantidade de leads ativos</param>
        public void AtualizarLeadsAtivos(int quantidade)
        {
            if (quantidade < 0)
                throw new DomainException("A quantidade de leads ativos deve ser não-negativa", nameof(MetricaVendedor));
            
            LeadsAtivosAtual = quantidade;
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }
        
        /// <summary>
        /// Restaura a entidade excluída logicamente
        /// </summary>
        public void Restaurar()
        {
            if (!Excluido)
                return;
            
            Excluido = false;
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }
        
        /// <summary>
        /// Atualiza totais de leads
        /// </summary>
        /// <param name="totalRecebidos">Total de leads recebidos</param>
        /// <param name="totalConvertidos">Total de leads convertidos</param>
        /// <param name="totalPerdidos">Total de leads perdidos</param>
        /// <param name="ativos">Leads ativos atualmente</param>
        public void AtualizarTotais(int totalRecebidos, int totalConvertidos, int totalPerdidos, int ativos)
        {
            if (totalRecebidos < 0)
                throw new DomainException("Total de leads recebidos deve ser não-negativo", nameof(MetricaVendedor));
            if (totalConvertidos < 0)
                throw new DomainException("Total de leads convertidos deve ser não-negativo", nameof(MetricaVendedor));
            if (totalPerdidos < 0)
                throw new DomainException("Total de leads perdidos deve ser não-negativo", nameof(MetricaVendedor));
            if (ativos < 0)
                throw new DomainException("Total de leads ativos deve ser não-negativo", nameof(MetricaVendedor));
            
            TotalLeadsRecebidos = totalRecebidos;
            TotalLeadsConvertidos = totalConvertidos;
            TotalLeadsPerdidos = totalPerdidos;
            LeadsAtivosAtual = ativos;
            
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }
        
        /// <summary>
        /// Atualiza os indicadores calculados
        /// </summary>
        /// <param name="taxaConversao">Taxa de conversão em percentual</param>
        /// <param name="velocidadeMedia">Velocidade média de atendimento em minutos</param>
        /// <param name="taxaPerda">Taxa de perda em percentual</param>
        public void AtualizarIndicadores(decimal taxaConversao, decimal velocidadeMedia, decimal taxaPerda)
        {
            if (taxaConversao < 0 || taxaConversao > 100)
                throw new DomainException("Taxa de conversão deve estar entre 0 e 100", nameof(MetricaVendedor));
            if (velocidadeMedia < 0)
                throw new DomainException("Velocidade média deve ser não-negativa", nameof(MetricaVendedor));
            if (taxaPerda < 0 || taxaPerda > 100)
                throw new DomainException("Taxa de perda deve estar entre 0 e 100", nameof(MetricaVendedor));
            
            TaxaConversao = taxaConversao;
            VelocidadeMediaAtendimento = velocidadeMedia;
            TaxaPerdaInatividade = taxaPerda;
            
            // Recalcular score geral
            RecalcularScoreGeral();
            
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }
        
        /// <summary>
        /// Atualiza as métricas detalhadas (JSON)
        /// </summary>
        /// <param name="metricasDetalhadas">JSON com métricas detalhadas</param>
        public void AtualizarMetricasDetalhadas(string metricasDetalhadas)
        {
            if (string.IsNullOrWhiteSpace(metricasDetalhadas))
                metricasDetalhadas = "{}";

            MetricasDetalhadas = metricasDetalhadas;
            AtualizarDataModificacao();
            DataUltimaAtualizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Valida os parâmetros de entrada
        /// </summary>
        private void ValidarParametros(
            int usuarioId, 
            int empresaId, 
            decimal taxaConversao, 
            decimal velocidadeMediaAtendimento, 
            decimal taxaPerdaInatividade)
        {
            if (usuarioId <= 0)
                throw new DomainException("ID do usuário deve ser maior que zero", nameof(MetricaVendedor));

            if (empresaId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(MetricaVendedor));

            if (taxaConversao < 0 || taxaConversao > 100)
                throw new DomainException("Taxa de conversão deve estar entre 0 e 100", nameof(MetricaVendedor));

            if (velocidadeMediaAtendimento < 0)
                throw new DomainException("Velocidade média de atendimento deve ser não-negativa", nameof(MetricaVendedor));

            if (taxaPerdaInatividade < 0 || taxaPerdaInatividade > 100)
                throw new DomainException("Taxa de perda por inatividade deve estar entre 0 e 100", nameof(MetricaVendedor));
        }
    }
}
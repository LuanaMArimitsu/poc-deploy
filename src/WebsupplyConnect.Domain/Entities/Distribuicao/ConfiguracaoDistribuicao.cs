using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Entidade que representa a configuração de distribuição de leads para uma empresa.
    /// Cada empresa pode ter múltiplas configurações, mas apenas uma ativa por vez.
    /// Define os parâmetros gerais de como os leads serão distribuídos entre os vendedores.
    /// </summary>
    public class ConfiguracaoDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID da empresa à qual esta configuração pertence
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Nome identificador da configuração (ex: "Distribuição Padrão - Daitan SP")
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada do objetivo e funcionamento desta configuração
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Indica se esta configuração está ativa. Apenas uma pode estar ativa por empresa
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// Data de início da vigência desta configuração
        /// </summary>
        public DateTime? DataInicioVigencia { get; private set; }

        /// <summary>
        /// Data de fim da vigência. Null indica vigência indefinida
        /// </summary>
        public DateTime? DataFimVigencia { get; private set; }

        /// <summary>
        /// Permite que leads sejam atribuídos manualmente, sobrescrevendo as regras automáticas
        /// </summary>
        public bool PermiteAtribuicaoManual { get; private set; }

        /// <summary>
        /// Número máximo de leads ativos que um vendedor pode ter simultaneamente
        /// </summary>
        public int? MaxLeadsAtivosVendedor { get; private set; }

        /// <summary>
        /// Define se o sistema deve considerar o horário de trabalho dos vendedores
        /// </summary>
        public bool ConsiderarHorarioTrabalho { get; private set; }

        /// <summary>
        /// Define se o sistema deve considerar feriados e fins de semana
        /// </summary>
        public bool ConsiderarFeriados { get; private set; }

        /// <summary>
        /// JSON com parâmetros gerais adicionais (escalação, SLA, etc)
        /// </summary>
        public string ParametrosGerais { get; private set; }

        // Propriedades de navegação
        public virtual Empresa.Empresa Empresa { get; private set; }
        public virtual ICollection<RegraDistribuicao> Regras { get; private set; }
        public virtual ICollection<HistoricoDistribuicao> Historicos { get; private set; }
        public virtual ICollection<AtribuicaoLead> Atribuicoes { get; private set; }

        // Construtor para EF Core
        protected ConfiguracaoDistribuicao()
        {
            Regras = new HashSet<RegraDistribuicao>();
            Historicos = new HashSet<HistoricoDistribuicao>();
            Atribuicoes = new HashSet<AtribuicaoLead>();
        }

        /// <summary>
        /// Cria uma nova configuração de distribuição
        /// </summary>
        public ConfiguracaoDistribuicao(
            int empresaId,
            string nome,
            string? descricao,
            bool considerarHorarioTrabalho = true,
            bool considerarFeriados = true,
            bool permiteAtribuicaoManual = true,
            int? maxLeadsAtivosVendedor = null,
            DateTime? dataInicioVigencia = null,
            DateTime? dataFimVigencia = null,
            bool ativo = false,
            string? parametrosGerais = null)
        {
            if (empresaId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(ConfiguracaoDistribuicao));

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da configuração é obrigatório", nameof(ConfiguracaoDistribuicao));

            EmpresaId = empresaId;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ativo = ativo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            PermiteAtribuicaoManual = permiteAtribuicaoManual;
            MaxLeadsAtivosVendedor = maxLeadsAtivosVendedor;
            ConsiderarHorarioTrabalho = considerarHorarioTrabalho;
            ConsiderarFeriados = considerarFeriados;
            ParametrosGerais = parametrosGerais ?? "{}";

            Regras = new HashSet<RegraDistribuicao>();
            Historicos = new HashSet<HistoricoDistribuicao>();
            Atribuicoes = new HashSet<AtribuicaoLead>();
        }

        /// <summary>
        /// Atualiza os dados básicos da configuração
        /// </summary>
        public void Atualizar(
            string nome, 
            string descricao, 
            bool ativo, 
            DateTime? dataInicioVigencia, 
            DateTime? dataFimVigencia,
            int? maxLeadsAtivosVendedor,
            bool considerarHorarioTrabalho,
            bool considerarFeriados = true,
            bool permiteAtribuicaoManual = true)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da configuração é obrigatório", nameof(ConfiguracaoDistribuicao));

            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ativo = ativo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            MaxLeadsAtivosVendedor = maxLeadsAtivosVendedor;
            ConsiderarHorarioTrabalho = considerarHorarioTrabalho;
            ConsiderarFeriados = considerarFeriados;
            PermiteAtribuicaoManual = permiteAtribuicaoManual;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa a configuração
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa a configuração
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Adiciona uma regra à configuração
        /// </summary>
        public void AdicionarRegra(RegraDistribuicao regra)
        {
            if (regra == null)
                throw new DomainException("Regra não pode ser nula", nameof(ConfiguracaoDistribuicao));
                
            // Verificar se a regra já existe
            if (Regras.Any(r => r.Id == regra.Id))
                return;
                
            // Adicionar regra
            ((HashSet<RegraDistribuicao>)Regras).Add(regra);
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove uma regra da configuração
        /// </summary>
        public void RemoverRegra(int regraId)
        {
            var regra = Regras.FirstOrDefault(r => r.Id == regraId);
            if (regra != null)
            {
                ((HashSet<RegraDistribuicao>)Regras).Remove(regra);
                AtualizarDataModificacao();
            }
        }

        /// <summary>
        /// Limpa todas as regras da configuração
        /// </summary>
        public void LimparRegras()
        {
            if (Regras is HashSet<RegraDistribuicao> hashSet)
            {
                hashSet.Clear();
            }
            else
            {
                Regras = new HashSet<RegraDistribuicao>();
            }
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Marca a configuração como excluída logicamente
        /// </summary>
        public void Excluir()
        {
            if (Ativo)
                throw new DomainException("Não é possível excluir uma configuração ativa. Desative-a primeiro.", nameof(ConfiguracaoDistribuicao));

            ExcluirLogicamente();
        }

        /// <summary>
        /// Método de fábrica para criar uma nova configuração
        /// </summary>
        public static ConfiguracaoDistribuicao Criar(
            int empresaId,
            string nome,
            string descricao,
            bool considerarHorarioTrabalho = true,
            bool considerarFeriados = true,
            bool permiteAtribuicaoManual = true,
            int? maxLeadsAtivosVendedor = null,
            DateTime? dataInicioVigencia = null,
            DateTime? dataFimVigencia = null,
            bool ativo = false,
            string? parametrosGerais = null)
        {
            return new ConfiguracaoDistribuicao(
                empresaId, nome, descricao, considerarHorarioTrabalho, considerarFeriados,
                permiteAtribuicaoManual, maxLeadsAtivosVendedor, dataInicioVigencia,
                dataFimVigencia, ativo, parametrosGerais);
        }
    }
}
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa um canal de comunicação (WhatsApp, Email, etc.)
    /// </summary>
    public class Canal
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Nome do canal de comunicação
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada do canal
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Tipo do canal (WhatsApp, Email, SMS, etc.)
        /// </summary>
        public int CanalTipoId { get; private set; }

        /// <summary>
        /// Indica se o canal está ativo
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// ID da empresa associada ao canal
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Limite diário de mensagens
        /// </summary>
        public int? LimiteDiario { get; private set; }

        /// <summary>
        /// Número de WhatsApp associado ao canal
        /// </summary>
        public string? WhatsAppNumero { get; private set; }

        /// <summary>
        /// Configuração de integração com serviços externos (JSON)
        /// </summary>
        public string? ConfiguracaoIntegracao { get; private set; }

        /// <summary>
        /// Origem Id padrão para criação do canal e lead
        /// </summary>
        public int OrigemPadraoId { get; private set; }

        public virtual Empresa.Empresa Empresa { get; private set; }
        /// <summary>
        /// Coleção de templates de mensagem desta empresa
        /// </summary>
        public virtual ICollection<Template> Templates { get; private set; }

        public virtual CanalTipo CanalTipo { get; private set; }

        /// <summary>
        /// Conversas associadas a este lead
        /// </summary>
        // Propriedades de navegação
        public virtual ICollection<Conversa> Conversas { get; private set; }
        public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; private set; }
        public virtual ICollection<LeadEvento> LeadEventos { get; private set; }

        // Construtor protegido para EF
        protected Canal() : base()
        {
            Conversas = new HashSet<Conversa>();
            Templates = new HashSet<Template>();
        }

        /// <summary>
        /// Cria um novo canal de comunicação
        /// </summary>
        public Canal(
            string nome,
            string descricao,
            int canalTipoId,
            int empresaId,
            int origemPadraoId,
            int? limiteDiario,
            string? whatsAppNumero = null,
            string? configuracaoIntegracao = null) : this()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome do canal não pode ser vazio", nameof(nome));

            if (canalTipoId <= 0)
                throw new DomainException("ID do canal tipo não pode ser vazio", nameof(canalTipoId));

            if (empresaId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(empresaId));

            if (origemPadraoId <= 0)
                throw new DomainException("ID da Origem Padrão deve ser maior que zero", nameof(origemPadraoId));

            Nome = nome;
            Descricao = descricao ?? string.Empty;
            CanalTipoId = canalTipoId;
            EmpresaId = empresaId;
            LimiteDiario = limiteDiario;
            WhatsAppNumero = whatsAppNumero;
            ConfiguracaoIntegracao = configuracaoIntegracao;
            OrigemPadraoId = origemPadraoId;
            Ativo = true;
        }

        /// <summary>
        /// Atualiza as informações do canal
        /// </summary>
        public void Atualizar(
            string nome,
            string descricao,
            int limiteDiario,
            string whatsAppNumero,
            string? configuracaoIntegracao = null)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome do canal não pode ser vazio", nameof(nome));

            Nome = nome;
            Descricao = descricao ?? string.Empty;
            LimiteDiario = limiteDiario;
            WhatsAppNumero = whatsAppNumero;
            ConfiguracaoIntegracao = configuracaoIntegracao;
        }

        /// <summary>
        /// Ativa o canal
        /// </summary>
        public void Ativar() 
        {
            if (!Ativo)
            {
                Ativo = true;
            }
        }

        /// <summary>
        /// Desativa o canal
        /// </summary>
        public void Desativar()
        {
            if (Ativo)
            {
                Ativo = false;
            }
        }
    }
}
using System.Text.RegularExpressions;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Empresa
{
    /// <summary>
    /// Representa uma empresa dentro do sistema de CRM.
    /// Herda de EntidadeBase conforme o diagrama de classes e possui relacionamento
    /// muitos-para-um com GrupoEmpresa.
    /// </summary>
    public class Empresa : EntidadeBase
    {
        /// <summary>
        /// Nome fantasia da empresa
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Razão social da empresa
        /// </summary>
        public string RazaoSocial { get; private set; }

        /// <summary>
        /// CNPJ da empresa (apenas números)
        /// </summary>
        public string Cnpj { get; private set; }

        /// <summary>
        /// Telefone de contato principal da empresa
        /// </summary>
        public string Telefone { get; private set; }

        /// <summary>
        /// Email de contato principal da empresa
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Indica se a empresa está ativa no sistema
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// ID do grupo empresarial ao qual a empresa pertence
        /// </summary>
        public int GrupoEmpresaId { get; private set; }

        /// <summary>
        /// Configuração de integração com sistemas externos
        ///     
        public string ConfiguracaoIntegracao { get; set; }

        /// <summary>
        /// Indica se a empresa possui integração com o sistema NBS
        /// </summary>
        public bool PossuiIntegracaoNBS { get; private set; }

        /// <summary>
        /// Relacionamento com o grupo empresarial
        /// </summary>
        public virtual GrupoEmpresa GrupoEmpresa { get; private set; }

        /// <summary>
        /// Coleção de associações entre usuários e esta empresa
        /// </summary>
        public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; private set; }

        public virtual ICollection<Role> Roles { get; private set; }

        /// <summary>
        /// Coleção de canais de comunicação desta empresa
        /// </summary>
        public virtual ICollection<Canal> Canais { get; private set; }

        /// <summary>
        /// Coleção de Leads  desta empresa
        /// </summary>
        public virtual ICollection<Lead.Lead> Leads { get; private set; }

        /// <summary>
        /// Coleção de Leads  desta empresa
        /// </summary>
        public virtual ICollection<Campanha> Campanhas { get; private set; }

        /// <summary>
        /// Coleção de prompts associados a esta empresa
        /// </summary>
        public virtual ICollection<PromptEmpresas> Prompts { get; private set; }

        public virtual ICollection<Domain.Entities.Equipe.Equipe> Equipes{ get; private set; }


        /// <summary>
        /// Construtor protegido para uso do EF Core, evitando erros de null
        /// </summary>
        protected Empresa()
        {
            UsuarioEmpresas = new HashSet<UsuarioEmpresa>();
            Equipes = new HashSet<Domain.Entities.Equipe.Equipe>();
            Canais = new HashSet<Canal>();
            Leads = new HashSet<Lead.Lead>();
            Prompts = new HashSet<PromptEmpresas>();
        }

        /// <summary>
        /// Construtor para criar uma nova empresa
        /// </summary>
        /// <param name="nome">Nome fantasia da empresa</param>
        /// <param name="razaoSocial">Razão social da empresa</param>
        /// <param name="cnpj">CNPJ da empresa</param>
        /// <param name="telefone">Telefone de contato</param>
        /// <param name="email">Email de contato</param>
        /// <param name="grupoEmpresaId">ID do grupo empresarial</param>
        /// <param name="configuracaoIntegracao">Configuração de integração com sistemas externos</param>
        /// <param name="possuiIntegracaoNBS">Indica se a empresa possui integração com o sistema NBS</param>
        public Empresa(string nome, string razaoSocial, string cnpj, string telefone, string email, int grupoEmpresaId, string configuracaoIntegracao, bool possuiIntegracaoNBS)
        {
            ValidarDominio(nome, razaoSocial, cnpj, telefone, email, grupoEmpresaId);
            Nome = nome;
            RazaoSocial = razaoSocial;
            Cnpj = LimparCnpj(cnpj);
            Telefone = LimparTelefone(telefone);
            Email = email;
            Ativo = true;
            GrupoEmpresaId = grupoEmpresaId;
            PossuiIntegracaoNBS = possuiIntegracaoNBS;

            ConfiguracaoIntegracao = configuracaoIntegracao;
            UsuarioEmpresas = new HashSet<UsuarioEmpresa>();
            Canais = new HashSet<Canal>();
        }


        /// <summary>
        /// Construtor para criar uma nova empresa
        /// </summary>
        /// <param name="nome">Nome fantasia da empresa</param>
        /// <param name="razaoSocial">Razão social da empresa</param>
        /// <param name="cnpj">CNPJ da empresa</param>
        /// <param name="telefone">Telefone de contato</param>
        /// <param name="email">Email de contato</param>
        /// <param name="grupoEmpresaId">ID do grupo empresarial</param>
        /// <param name="configuracaoIntegracao">Configuração de integração com sistemas externos</param>
        /// <param name="possuiIntegracaoNBS">Indica se a empresa possui integração com o sistema NBS</param>
        public Empresa(int id, string nome, string razaoSocial, string cnpj, string telefone, string email, int grupoEmpresaId, string configuracaoIntegracao, bool possuiIntegracaoNBS, DateTime dataCriacao, DateTime dataModificacao)
        {
            Id = id;
            Nome = nome;
            RazaoSocial = razaoSocial;
            Cnpj = cnpj;
            Telefone = telefone;
            Email = email;
            Ativo = true;
            GrupoEmpresaId = grupoEmpresaId;

            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            ConfiguracaoIntegracao = configuracaoIntegracao;
            PossuiIntegracaoNBS = possuiIntegracaoNBS;
            UsuarioEmpresas = new HashSet<UsuarioEmpresa>();
            Canais = new HashSet<Canal>();
        }

        /// <summary>
        /// Atualiza as informações da empresa
        /// </summary>
        /// <param name="nome">Nome fantasia da empresa</param>
        /// <param name="razaoSocial">Razão social da empresa</param>
        /// <param name="cnpj">CNPJ da empresa</param>
        /// <param name="telefone">Telefone de contato</param>
        /// <param name="email">Email de contato</param>
        public void Atualizar(string nome, string razaoSocial, string cnpj, string telefone, string email, string configuracaoIntegracao)
        {
            ValidarDominio(nome, razaoSocial, cnpj, telefone, email, GrupoEmpresaId);

            Nome = nome;
            RazaoSocial = razaoSocial;
            Cnpj = LimparCnpj(cnpj);
            Telefone = LimparTelefone(telefone);
            ConfiguracaoIntegracao = configuracaoIntegracao;
            Email = email;
        }

        /// <summary>
        /// Altera o grupo empresarial da empresa
        /// </summary>
        /// <param name="grupoEmpresaId">ID do novo grupo empresarial</param>
        public void AlterarGrupoEmpresarial(int grupoEmpresaId)
        {
            if (grupoEmpresaId <= 0)
                throw new DomainException("O ID do grupo empresarial deve ser maior que zero.", nameof(Empresa));

            GrupoEmpresaId = grupoEmpresaId;
        }

        /// <summary>
        /// Ativa a empresa
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
        }

        /// <summary>
        /// Desativa a empresa
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
        }

        /// <summary>
        /// Adiciona um canal de comunicação à empresa
        /// </summary>
        /// <param name="canal">Canal de comunicação</param>
        public void AdicionarCanal(Canal canal)
        {
            if (canal == null)
                throw new DomainException("O canal não pode ser nulo.", nameof(Empresa));

            if (canal.EmpresaId != Id)
                throw new DomainException("O canal não pertence a esta empresa.", nameof(Empresa));

            Canais.Add(canal);
        }


        /// <summary>
        /// Adiciona uma associação de usuário à empresa
        /// </summary>
        /// <param name="usuarioEmpresa">Associação entre usuário e empresa</param>
        public void AdicionarUsuarioEmpresa(UsuarioEmpresa usuarioEmpresa)
        {
            if (usuarioEmpresa == null)
                throw new DomainException("A associação usuário-empresa não pode ser nula.", nameof(Empresa));

            if (usuarioEmpresa.EmpresaId != Id)
                throw new DomainException("A associação não pertence a esta empresa.", nameof(Empresa));

            UsuarioEmpresas.Add(usuarioEmpresa);
        }

        /// <summary>
        /// Valida as regras de domínio para a empresa
        /// </summary>
        private void ValidarDominio(string nome, string razaoSocial, string cnpj, string telefone, string email, int grupoEmpresaId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da empresa é obrigatório.", nameof(Empresa));

            if (nome.Length > 100)
                throw new DomainException("O nome da empresa não pode ter mais que 100 caracteres.", nameof(Empresa));

            if (string.IsNullOrWhiteSpace(razaoSocial))
                throw new DomainException("A razão social da empresa é obrigatória.", nameof(Empresa));

            if (razaoSocial.Length > 200)
                throw new DomainException("A razão social não pode ter mais que 200 caracteres.", nameof(Empresa));

            if (string.IsNullOrWhiteSpace(cnpj))
                throw new DomainException("O CNPJ da empresa é obrigatório.", nameof(Empresa));

            if (!ValidarCNPJ(cnpj))
                throw new DomainException("O CNPJ informado é inválido.", nameof(Empresa));

            if (!string.IsNullOrWhiteSpace(email) && !ValidarEmail(email))
                throw new DomainException("O email informado é inválido.", nameof(Empresa));

            if (!string.IsNullOrWhiteSpace(telefone) && !ValidarTelefone(telefone))
                throw new DomainException("O telefone informado é inválido.", nameof(Empresa));

            if (grupoEmpresaId <= 0)
                throw new DomainException("O ID do grupo empresarial deve ser maior que zero.", nameof(Empresa));
        }

        /// <summary>
        /// Remove caracteres não numéricos do CNPJ
        /// </summary>
        private string LimparCnpj(string cnpj)
        {
            return string.IsNullOrWhiteSpace(cnpj)
                ? string.Empty
                : new string(cnpj.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Remove caracteres não numéricos do telefone
        /// </summary>
        private string LimparTelefone(string telefone)
        {
            return string.IsNullOrWhiteSpace(telefone)
                ? string.Empty
                : new string(telefone.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Valida o formato e dígitos verificadores do CNPJ
        /// </summary>
        private bool ValidarCNPJ(string cnpj)
        {
            // Remove caracteres não numéricos
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            // Verifica se tem 14 dígitos
            if (cnpj.Length != 14)
                return false;

            // Verifica se todos os dígitos são iguais
            if (cnpj.Distinct().Count() == 1)
                return false;

            // Calcula o primeiro dígito verificador
            int[] multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += (cnpj[i] - '0') * multiplicadores1[i];

            int resto = soma % 11;
            int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

            // Verifica o primeiro dígito verificador
            if ((cnpj[12] - '0') != digitoVerificador1)
                return false;

            // Calcula o segundo dígito verificador
            int[] multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += (cnpj[i] - '0') * multiplicadores2[i];

            resto = soma % 11;
            int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

            // Verifica o segundo dígito verificador
            return (cnpj[13] - '0') == digitoVerificador2;
        }

        /// <summary>
        /// Valida o formato do email
        /// </summary>
        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Verifica o formato básico do email usando regex
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida o formato do telefone
        /// </summary>
        private bool ValidarTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return false;

            // Remove caracteres não numéricos
            var apenasNumeros = new string(telefone.Where(char.IsDigit).ToArray());

            // Verifica se o telefone tem um tamanho válido (entre 10 e 11 dígitos para Brasil)
            return apenasNumeros.Length >= 10 && apenasNumeros.Length <= 11;
        }
    }
}
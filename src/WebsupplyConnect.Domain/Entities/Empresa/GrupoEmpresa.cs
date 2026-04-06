using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Empresa
{
    /// <summary>
    /// Representa um grupo empresarial que contém uma ou mais empresas.
    /// Conforme o diagrama de entidades, esta é uma entidade base com relacionamento
    /// de um-para-muitos com a entidade Empresa.
    /// </summary>
    public class GrupoEmpresa
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Nome do grupo empresarial
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// CNPJ da holding do grupo
        /// </summary>
        public string CnpjHolding { get; private set; }

        public string? Logo { get; set; }

        /// <summary>
        /// Indica se o grupo está ativo no sistema
        /// </summary>
        public bool Ativo { get; private set; }


        /// <summary>
        /// Coleção das empresas associadas a este grupo
        /// </summary>
        public virtual ICollection<Empresa> Empresas { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected GrupoEmpresa()
        {
            Empresas = new HashSet<Empresa>();
        }

        /// <summary>
        /// Construtor para criar um novo grupo empresarial
        /// </summary>
        /// <param name="nome">Nome do grupo empresarial</param>
        /// <param name="cnpjHolding">CNPJ da holding do grupo</param>
        public GrupoEmpresa(int id, string nome, string cnpjHolding)
        {
            ValidarDominio(nome, cnpjHolding);

            Id = id;
            Nome = nome;
            CnpjHolding = cnpjHolding;
            Ativo = true;
            Empresas = new HashSet<Empresa>();
        }

        /// <summary>
        /// Atualiza as informações do grupo empresarial
        /// </summary>
        /// <param name="nome">Nome do grupo empresarial</param>
        /// <param name="cnpjHolding">CNPJ da holding do grupo</param>
        public void Atualizar(string nome, string cnpjHolding)
        {
            ValidarDominio(nome, cnpjHolding);

            Nome = nome;
            CnpjHolding = cnpjHolding;
        }

        /// <summary>
        /// Ativa o grupo empresarial
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
        }

        /// <summary>
        /// Desativa o grupo empresarial
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
        }

        /// <summary>
        /// Adiciona uma empresa ao grupo
        /// </summary>
        /// <param name="empresa">Empresa a ser adicionada</param>
        public void AdicionarEmpresa(Empresa empresa)
        {
            if (empresa == null)
                throw new DomainException("A empresa não pode ser nula.", nameof(GrupoEmpresa));

            if (empresa.GrupoEmpresaId != Id)
                throw new DomainException("A empresa não pertence a este grupo.", nameof(GrupoEmpresa));

            Empresas.Add(empresa);
        }

        /// <summary>
        /// Valida as regras de domínio para o grupo empresarial
        /// </summary>
        /// <param name="nome">Nome do grupo empresarial</param>
        /// <param name="cnpjHolding">CNPJ da holding do grupo</param>
        private void ValidarDominio(string nome, string cnpjHolding)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do grupo empresarial é obrigatório.", nameof(GrupoEmpresa));

            if (nome.Length > 100)
                throw new DomainException("O nome do grupo empresarial não pode ter mais que 100 caracteres.", nameof(GrupoEmpresa));

            if (!string.IsNullOrWhiteSpace(cnpjHolding))
            {
                if (!ValidarCNPJ(cnpjHolding))
                    throw new DomainException("O CNPJ da holding é inválido.", nameof(GrupoEmpresa));
            }
        }

        /// <summary>
        /// Valida o formato e dígitos verificadores do CNPJ
        /// </summary>
        /// <param name="cnpj">CNPJ a ser validado</param>
        /// <returns>True se o CNPJ for válido, False caso contrário</returns>
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
    }
}
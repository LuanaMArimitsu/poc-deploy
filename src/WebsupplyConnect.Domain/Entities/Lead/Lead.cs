using System.Text.RegularExpressions;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa um lead/contato no sistema CRM.
    /// Implementa o padrão TPT (Table Per Type) através da herança de EntidadeAlvo.
    /// </summary>
    public class Lead : EntidadeBase
    {
        /// <summary>
        /// Nome do lead
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        ///  Apelido do lead
        /// </summary>
        public string? Apelido { get; private set; }

        /// <summary>
        /// Email do lead
        /// </summary>
        public string? Email { get; private set; }

        /// <summary>
        /// Telefone do lead
        /// </summary>
        public string? Telefone { get; private set; }

        /// <summary>
        /// Cargo do lead na empresa
        /// </summary>
        public string? Cargo { get; private set; }

        /// <summary>
        /// Número de WhatsApp do lead
        /// </summary>
        public string? WhatsappNumero { get; private set; }

        /// <summary>
        /// CPF do lead
        /// </summary>
        public string? CPF { get; private set; }

        /// <summary>
        /// Data de nascimento do lead
        /// </summary>
        public DateTime? DataNascimento { get; private set; }

        /// <summary>
        /// Gênero do lead
        /// </summary>
        public string? Genero { get; private set; }

        /// <summary>
        /// Nome da empresa onde o lead trabalha
        /// </summary>
        public string? NomeEmpresa { get; private set; }

        /// <summary>
        /// CNPJ da empresa onde o lead trabalha
        /// </summary>
        public string? CNPJEmpresa { get; private set; }

        /// <summary>
        /// ID do status atual do lead
        /// </summary>
        public int LeadStatusId { get; private set; }

        /// <summary>
        /// Data em que o lead foi convertido para cliente
        /// </summary>
        public DateTime? DataConversaoCliente { get; private set; }

        /// <summary>
        /// Nível de interesse do lead (baixo, médio, alto)
        /// </summary>
        public string? NivelInteresse { get; private set; }

        /// <summary>
        /// Observações cadastrais sobre o lead
        /// </summary>
        public string? ObservacoesCadastrais { get; private set; }

        /// <summary>
        /// ID do usuário responsável pelo lead
        /// </summary>
        public int? ResponsavelId { get; private set; }

        /// <summary>
        /// ID da origem do lead (como o lead chegou ao sistema)
        /// </summary>
        public int OrigemId { get; private set; }

        /// <summary>
        /// ID do endereço residencial do lead (opcional)
        /// </summary>
        public int? EnderecoResidencialId { get; private set; }

        /// <summary>
        /// ID do endereço comercial do lead (opcional)
        /// </summary>
        public int? EnderecoComercialId { get; private set; }

        /// <summary>
        /// ID da empresa que o Lead pertence (opcional)
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// ID da Equipe que o Lead pertence (opcional no primeiro momento)
        /// </summary>
        public int? EquipeId { get; private set; }

        /// <summary>
        /// Data e hora do primeiro contato realizado com o lead
        /// </summary>
        public DateTime? DataPrimeiroContato { get; private set; }

        /// <summary>
        /// Navegação para a equipe que o Lead pertence
        /// </summary>
        public virtual Equipe.Equipe Equipe { get; private set; }

        /// <summary>
        /// Navegação para empresa que o Lead pertence
        /// </summary>
        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Navegação para o status do lead
        /// </summary>
        public virtual LeadStatus LeadStatus { get; private set; }

        /// <summary>
        /// Navegação para o usuário responsável
        /// </summary>
        public virtual Equipe.MembroEquipe Responsavel { get; private set; }

        /// <summary>
        /// Navegação para a origem do lead
        /// </summary>
        public virtual Origem Origem { get; private set; }

        /// <summary>
        /// Navegação para o endereço residencial
        /// </summary>
        public virtual Endereco EnderecoResidencial { get; private set; }

        /// <summary>
        /// Navegação para o endereço comercial
        /// </summary>
        public virtual Endereco EnderecoComercial { get; private set; }

        /// <summary>
        /// Histórico de mudanças de status do lead
        /// </summary>
        public virtual ICollection<LeadStatusHistorico> StatusHistorico { get; private set; }

        /// <summary>
        /// Conversas associadas a este lead
        /// </summary>
        public virtual ICollection<Conversa> Conversas { get; private set; }

        public virtual ICollection<LeadEvento> LeadEventos { get; private set; }

        public virtual ICollection<Oportunidade.Oportunidade> Oportunidades { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Lead()
        {
            StatusHistorico = new HashSet<LeadStatusHistorico>();
            Conversas = new HashSet<Conversa>();
        }

        /// <summary>
        /// Construtor para criar um novo lead
        /// </summary>
        /// <param name="whatsappNumero">Número de WhatsApp</param>
        /// <param name="leadStatusId">ID do status inicial</param>
        /// <param name="responsavelId">ID do usuário responsável</param>
        /// <param name="origemId">ID da origem do lead</param>
        public Lead(
            string whatsappNumero,
            int leadStatusId,
            int? responsavelId,
            int origemId,
            int empresaId,
            string? apelido)
        {
            Nome = "Novo Lead";
            WhatsappNumero = LimparNumeroTelefone(whatsappNumero);
            LeadStatusId = leadStatusId;
            ResponsavelId = responsavelId;
            OrigemId = origemId;
            EmpresaId = empresaId;
            NivelInteresse = "baixo";
            StatusHistorico = new HashSet<LeadStatusHistorico>();
            //Oportunidades = new HashSet<Oportunidade.Oportunidade>();
            Conversas = new HashSet<Conversa>();
            Apelido = apelido;
        }

        public Lead(
            string nome,
            int leadStatusId,
            int? responsavelId,
            int equipeId,
            int origemId,
            int empresaId,
            string? whatsappNumero,
            string? email,
            string? telefone,
            string? cargo,
            string? cpf,
            string? genero,
            string? cnpjEmpresa,
            string? nomeEmpresa,
            string? nivelInteresse,
            string? observacoes,
            DateTime? dataNascimento,
            int? enderecoResidencialID = null,
            int? enderecoComercialID = null,
            string? apelido = null)
        {
            Nome = nome;
            Email = email;
            Telefone = telefone;
            Cargo = cargo;
            CPF = cpf;
            Genero = genero;
            CNPJEmpresa = cnpjEmpresa;
            NomeEmpresa = nomeEmpresa;
            NivelInteresse = nivelInteresse ?? "baixo";
            ObservacoesCadastrais = observacoes;
            DataNascimento = dataNascimento;
            EnderecoResidencialId = enderecoResidencialID;
            EnderecoComercialId = enderecoComercialID;
            WhatsappNumero = whatsappNumero != null ? LimparNumeroTelefone(whatsappNumero) : null;
            LeadStatusId = leadStatusId;
            EquipeId = equipeId;
            ResponsavelId = responsavelId;
            OrigemId = origemId;
            EmpresaId = empresaId;
            StatusHistorico = new HashSet<LeadStatusHistorico>();
            Conversas = new HashSet<Conversa>();
            Apelido = apelido;
        }

        public void EditarLead(
            string? nome,
            int? origemId,
            string nivelInteresse,
            string? email = null,
            string? telefone = null,
            string? cargo = null,
            string? whatsappNumero = null,
            string? cpf = null,
            string? genero = null,
            string? cnpjEmpresa = null,
            string? nomeEmpresa = null,
            string? observacoes = null,
            DateTime? dataNascimento = null,
            int? enderecoResidencialId = null,
            int? enderecoComercialId = null)
        {
            // Email
            if (!string.IsNullOrWhiteSpace(email) && !ValidarEmail(email))
                throw new DomainException("Formato de email inválido.", nameof(Lead));

            // CPF
            if (!string.IsNullOrWhiteSpace(cpf))
            {
                var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());
                if (!ValidarCPF(cpfLimpo))
                    throw new DomainException("CPF inválido.", nameof(Lead));
                CPF = cpfLimpo;
            }
            else
            {
                CPF = null;
            }

            // CNPJ
            if (!string.IsNullOrWhiteSpace(cnpjEmpresa))
            {
                var cnpjLimpo = new string(cnpjEmpresa.Where(char.IsDigit).ToArray());
                if (!ValidarCNPJ(cnpjLimpo))
                    throw new DomainException("CNPJ inválido.", nameof(Lead));
                CNPJEmpresa = cnpjLimpo;
            }
            else
            {
                CNPJEmpresa = null;
            }

            Nome = !string.IsNullOrWhiteSpace(nome) && nome != Nome ? nome : Nome;

            OrigemId = origemId.HasValue && origemId > 0 && origemId != OrigemId
                ? origemId.Value
                : OrigemId;

            Email = email;
            Telefone = telefone;
            Cargo = cargo;
            Genero = genero;
            NomeEmpresa = nomeEmpresa;
            NivelInteresse = nivelInteresse != NivelInteresse ? nivelInteresse : NivelInteresse;
            ObservacoesCadastrais = observacoes;
            DataNascimento = dataNascimento;
            EnderecoResidencialId = enderecoResidencialId;
            EnderecoComercialId = enderecoComercialId;

            WhatsappNumero = string.IsNullOrWhiteSpace(whatsappNumero) ? null : LimparNumeroTelefone(whatsappNumero);

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera o nível de interesse do lead
        /// </summary>
        /// <param name="nivelInteresse">Novo nível de interesse (Baixo, Médio, Alto)</param>
        public void AlterarNivelInteresse(string nivelInteresse)
        {
            var niveisValidos = new[] { "Baixo", "Médio", "Alto" };

            if (!niveisValidos.Contains(nivelInteresse))
                throw new DomainException("Nível de interesse inválido. Use: Baixo, Médio ou Alto.", nameof(Lead));

            NivelInteresse = nivelInteresse;

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera o status do lead
        /// </summary>
        /// <param name="novoStatusId">ID do novo status</param>
        /// <param name="observacao">Observação sobre a mudança</param>
        /// <param name="usuarioId">ID do usuário que realizou a alteração</param>
        /// <returns>Objeto que representa o histórico da alteração</returns>
        public LeadStatusHistorico AlterarStatus(int novoStatusId, int usuarioId,string? observacao = "STATUS ALTERADO")
        {
            if (novoStatusId <= 0)
                throw new DomainException("O ID do status deve ser maior que zero.", nameof(Lead));

            var statusAnteriorId = LeadStatusId;
            LeadStatusId = novoStatusId;

            var historico = new LeadStatusHistorico(
                Id,
                statusAnteriorId,
                novoStatusId,
                TimeHelper.GetBrasiliaTime(),
                usuarioId,
                observacao);

            StatusHistorico.Add(historico);

            AtualizarDataModificacao();

            return historico;
        }


        /// <summary>
        /// Adiciona a associação com o endereço residencial
        /// </summary>
        public void AdicionarEnderecoResidencial(int enderecoId)
        {
            EnderecoResidencialId = enderecoId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Adiciona a associação com o endereço comercial
        /// </summary>
        public void AdicionarEnderecoComercial(int enderecoId)
        {
            EnderecoComercialId = enderecoId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove a associação com o endereço residencial
        /// </summary>
        public void RemoverEnderecoResidencial()
        {
            EnderecoResidencialId = null;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove a associação com o endereço comercial
        /// </summary>
        public void RemoverEnderecoComercial()
        {
            EnderecoComercialId = null;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove caracteres não numéricos de um número de telefone
        /// </summary>
        private string LimparNumeroTelefone(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return string.Empty;

            return new string(numero.Where(char.IsDigit).ToArray());
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
        /// Valida o CPF
        /// </summary>
        private bool ValidarCPF(string cpf)
        {
            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            if (cpf.Distinct().Count() == 1)
                return false;

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * (10 - i);

            int resto = soma % 11;
            int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

            // Verifica o primeiro dígito verificador
            if ((cpf[9] - '0') != digitoVerificador1)
                return false;

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (cpf[i] - '0') * (11 - i);

            resto = soma % 11;
            int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

            // Verifica o segundo dígito verificador
            return (cpf[10] - '0') == digitoVerificador2;
        }

        /// <summary>
        /// Valida o CNPJ
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
        /// Atribui um novo responsável, equipe e empresa ao lead
        /// </summary>
        /// <param name="membroId">ID do membro responsável</param>
        /// <param name="equipeId">ID da equipe do membro</param>
        /// <param name="empresaId">ID da empresa</param>
        public void AtribuirResponsavel(int membroId, int equipeId, int empresaId)
        {
            ResponsavelId = membroId;
            EquipeId = equipeId;
            EmpresaId = empresaId;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        public void AtribuirSomenteEquipe(int? equipeId)
        {
            EquipeId = equipeId;
            AtualizarDataModificacao();
        }

        public void DefinirDataConversaoCliente()
        {
            if (DataConversaoCliente == null)
                DataConversaoCliente = TimeHelper.GetBrasiliaTime();

            AtualizarDataModificacao();
        }

        public void AlterarNomeLead(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new DomainException("O nome do lead não pode ser vazio.", nameof(Lead));

            var nomeNormalizado = novoNome.Trim();

            if (nomeNormalizado == Nome)
                return;

            Nome = nomeNormalizado;
            AtualizarDataModificacao();
        }
    }
}

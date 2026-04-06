using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa um endereço no sistema.
    /// </summary>
    public class Endereco : EntidadeBase
    {
        /// <summary>
        /// Logradouro (rua, avenida, etc.)
        /// </summary>
        public string Logradouro { get; private set; }

        /// <summary>
        /// Número do endereço
        /// </summary>
        public string Numero { get; private set; }

        /// <summary>
        /// Complemento do endereço
        /// </summary>
        public string? Complemento { get; private set; }

        /// <summary>
        /// Bairro
        /// </summary>
        public string Bairro { get; private set; }

        /// <summary>
        /// Cidade
        /// </summary>
        public string Cidade { get; private set; }

        /// <summary>
        /// Estado (UF)
        /// </summary>
        public string Estado { get; private set; }

        /// <summary>
        /// País
        /// </summary>
        public string Pais { get; private set; }

        /// <summary>
        /// CEP
        /// </summary>
        public string CEP { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Endereco() : base()
        {
        }

        /// <summary>
        /// Construtor para criar um novo endereço
        /// </summary>
        /// <param name="logradouro">Logradouro (rua, avenida, etc.)</param>
        /// <param name="numero">Número do endereço</param>
        /// <param name="bairro">Bairro</param>
        /// <param name="cidade">Cidade</param>
        /// <param name="estado">Estado (UF)</param>
        /// <param name="cep">CEP</param>
        /// <param name="complemento">Complemento do endereço</param>
        /// <param name="pais">País</param>
        public Endereco(
            string logradouro,
            string numero,
            string bairro,
            string cidade,
            string estado,
            string cep,
            string? complemento = null,
            string pais = "Brasil") : base()
        {
            ValidarDominio(logradouro, numero, bairro, cidade, estado, cep);

            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            Pais = pais;
            CEP = LimparCep(cep);
        }

        /// <summary>
        /// Atualiza as informações do endereço
        /// </summary>
        /// <param name="logradouro">Logradouro (rua, avenida, etc.)</param>
        /// <param name="numero">Número do endereço</param>
        /// <param name="bairro">Bairro</param>
        /// <param name="cidade">Cidade</param>
        /// <param name="estado">Estado (UF)</param>
        /// <param name="cep">CEP</param>
        /// <param name="complemento">Complemento do endereço</param>
        /// <param name="pais">País</param>
        public void Atualizar(
            string logradouro,
            string numero,
            string bairro,
            string cidade,
            string estado,
            string cep,
            string complemento = null,
            string pais = "Brasil")
        {

            Logradouro = string.IsNullOrWhiteSpace(logradouro) ? Logradouro : logradouro;
            Numero = string.IsNullOrWhiteSpace(numero) ? Numero : numero;
            Complemento = string.IsNullOrWhiteSpace(complemento) ? Complemento : complemento;
            Bairro = string.IsNullOrWhiteSpace(bairro) ? Bairro : bairro;
            Cidade = string.IsNullOrWhiteSpace(cidade) ? Cidade : cidade;
            Estado = string.IsNullOrWhiteSpace(estado) ? Estado : estado;
            Pais = string.IsNullOrWhiteSpace(pais) ? Pais : pais;
            CEP = string.IsNullOrWhiteSpace(cep) ? CEP : LimparCep(cep);


            AtualizarDataModificacao();
        }

        /// <summary>
        /// Obtém o endereço completo formatado
        /// </summary>
        /// <returns>Endereço formatado</returns>
        public string ObterEnderecoCompleto()
        {
            var endereco = $"{Logradouro}, {Numero}";

            if (!string.IsNullOrWhiteSpace(Complemento))
                endereco += $" - {Complemento}";

            endereco += $" - {Bairro}, {Cidade}/{Estado}";

            if (!string.IsNullOrWhiteSpace(CEP))
                endereco += $" - CEP: {FormatarCep(CEP)}";

            if (!string.IsNullOrWhiteSpace(Pais) && Pais != "Brasil")
                endereco += $" - {Pais}";

            return endereco;
        }

        /// <summary>
        /// Valida as regras de domínio para o endereço
        /// </summary>
        private void ValidarDominio(string logradouro, string numero, string bairro, string cidade, string estado, string cep)
        {
            if (string.IsNullOrWhiteSpace(logradouro))
                throw new DomainException("O logradouro é obrigatório.", nameof(Endereco));

            if (logradouro.Length > 200)
                throw new DomainException("O logradouro não pode ter mais que 200 caracteres.", nameof(Endereco));

            if (string.IsNullOrWhiteSpace(numero))
                throw new DomainException("O número é obrigatório.", nameof(Endereco));

            if (numero.Length > 20)
                throw new DomainException("O número não pode ter mais que 20 caracteres.", nameof(Endereco));

            if (!string.IsNullOrWhiteSpace(Complemento) && Complemento.Length > 100)
                throw new DomainException("O complemento não pode ter mais que 100 caracteres.", nameof(Endereco));

            if (string.IsNullOrWhiteSpace(bairro))
                throw new DomainException("O bairro é obrigatório.", nameof(Endereco));

            if (bairro.Length > 100)
                throw new DomainException("O bairro não pode ter mais que 100 caracteres.", nameof(Endereco));

            if (string.IsNullOrWhiteSpace(cidade))
                throw new DomainException("A cidade é obrigatória.", nameof(Endereco));

            if (cidade.Length > 100)
                throw new DomainException("A cidade não pode ter mais que 100 caracteres.", nameof(Endereco));

            if (string.IsNullOrWhiteSpace(estado))
                throw new DomainException("O estado é obrigatório.", nameof(Endereco));

            if (estado.Length > 50)
                throw new DomainException("O estado não pode ter mais que 50 caracteres.", nameof(Endereco));

            if (string.IsNullOrWhiteSpace(cep))
                throw new DomainException("O CEP é obrigatório.", nameof(Endereco));

            if (!ValidarCep(cep))
                throw new DomainException("O formato do CEP é inválido.", nameof(Endereco));
        }

        /// <summary>
        /// Remove caracteres não numéricos do CEP
        /// </summary>
        private string LimparCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return string.Empty;

            return new string(cep.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Formata o CEP no padrão 00000-000
        /// </summary>
        private string FormatarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep) || cep.Length != 8)
                return cep;

            return $"{cep.Substring(0, 5)}-{cep.Substring(5, 3)}";
        }

        /// <summary>
        /// Valida o formato do CEP
        /// </summary>
        private bool ValidarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return false;

            // Remove caracteres não numéricos
            cep = new string(cep.Where(char.IsDigit).ToArray());

            // CEP brasileiro deve ter 8 dígitos
            return cep.Length == 8;
        }
    }
}
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Base
{
    /// <summary>
    /// Classe base abstrata para entidades de tipificação (status, tipos, etc)
    /// </summary>
    public abstract class EntidadeTipificacao : EntidadeBase
    {
        /// <summary>
        /// Código único da tipificação, usado como identificador chave em integrações
        /// </summary>
        public string Codigo { get; protected set; }

        /// <summary>
        /// Nome da tipificação
        /// </summary>
        public string Nome { get; protected set; }

        /// <summary>
        /// Descrição detalhada da tipificação
        /// </summary>
        public string Descricao { get; protected set; }

        /// <summary>
        /// Ordem de exibição/prioridade da tipificação
        /// </summary>
        public int Ordem { get; protected set; }

        /// <summary>
        /// Ícone associado 
        /// </summary>
        public string? Icone { get; private set; }

        /// <summary>
        /// Cor associada (formato hexadecimal #RRGGBB)
        /// </summary>
        public string? Cor { get; private set; }

        protected EntidadeTipificacao()
        {
        }

        protected EntidadeTipificacao(string codigo, string nome, string descricao, int ordem, string? icone, string? cor)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new DomainException("Código da tipificação não pode ser vazio", nameof(codigo));

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da tipificação não pode ser vazio", nameof(nome));

            if (!string.IsNullOrWhiteSpace(cor))
                ValidarCor(cor ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(icone))
                ValidarIcone(icone ?? string.Empty);

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            Icone = icone ?? string.Empty;
            Cor = cor ?? string.Empty;
        }

        /// <summary>
        /// Atualiza as informações da tipificação
        /// </summary>
        public virtual void Atualizar(string nome, string descricao, int ordem)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da tipificação não pode ser vazio", nameof(nome));

            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            AtualizarDataModificacao();
        }


        /// <summary>
        /// Valida as regras de domínio específicas para o tipo de origem
        /// </summary>
        /// <param name="icone">Nome do ícone a ser validado</param>
        private void ValidarIcone(string icone)
        {
            if (string.IsNullOrWhiteSpace(icone))
                throw new DomainException("O ícone do tipo de origem é obrigatório.", nameof(OrigemTipo));

            if (icone.Length > 50)
                throw new DomainException("O nome do ícone não pode ter mais que 50 caracteres.", nameof(OrigemTipo));
        }


        private void ValidarCor(string cor)
        {
            if (string.IsNullOrWhiteSpace(cor))
                throw new DomainException("A cor do status é obrigatória.", nameof(LeadStatus));

            if (!ValidarCorHexadecimal(cor))
                throw new DomainException("O formato da cor deve ser hexadecimal (#RRGGBB).", nameof(LeadStatus));
        }

        /// <summary>
        /// Valida se a cor está no formato hexadecimal correto (#RRGGBB)
        /// </summary>
        /// <param name="cor">Cor hexadecimal a ser validada</param>
        /// <returns>True se a cor for válida, False caso contrário</returns>
        private bool ValidarCorHexadecimal(string cor)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(cor, @"^#[0-9A-Fa-f]{6}$");
        }
        
         /// <summary>
        /// Atualiza o ícone associado ao tipo
        /// </summary>
        public void AtualizarIcone(string icone)
        {
            Icone = icone ?? "fa-circle";
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza a cor associada ao tipo
        /// </summary>
        public void AtualizarCor(string cor)
        {
            Cor = cor ?? "#808080";
            AtualizarDataModificacao();
        }
    }
}
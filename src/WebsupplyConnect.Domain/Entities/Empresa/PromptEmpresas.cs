using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Empresa
{
    public class PromptEmpresas
    {
        public int Id { get; set; }
        public required string Prompt { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataUltimaAtualizacao { get; set; }
        public bool Excluido { get; set; } = false;
        public bool Sistema { get; set; } = false;
        public int? TipoPromptId { get; set; }
        public TipoPromptEmpresas? TipoPrompt { get; set; }

        // Construtor para criação de novo prompt
        public PromptEmpresas(string prompt, int empresaId, int? tipoPromptId, bool excluido = false, bool sistema = false)
        {
            Prompt = prompt;
            EmpresaId = empresaId;
            TipoPromptId = tipoPromptId;
            Excluido = excluido;
            DataCriacao = TimeHelper.GetBrasiliaTime();
            Sistema = sistema;
            AtualizarDataUltimaAlteracao();
        }

        // Atualiza o texto do prompt
        public void AtualizarPrompt(string novoPrompt)
        {
            if (string.IsNullOrWhiteSpace(novoPrompt))
                throw new ArgumentException("O prompt não pode ser vazio.", nameof(novoPrompt));

            Prompt = novoPrompt;
            AtualizarDataUltimaAlteracao();
        }

        // Validação de domínio
        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Prompt))
                throw new InvalidOperationException("O prompt é obrigatório.");

            if (EmpresaId <= 0)
                throw new InvalidOperationException("EmpresaId inválido.");
        }

        // Atualiza a data da última alteração
        public void AtualizarDataUltimaAlteracao()
        {
            DataUltimaAtualizacao = DateTime.UtcNow;
        }
    }
}

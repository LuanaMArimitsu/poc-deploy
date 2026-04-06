namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// Resultado de validação com erros e warnings
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();
        
        /// <summary>
        /// Lista de erros encontrados
        /// </summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        
        /// <summary>
        /// Lista de warnings encontrados
        /// </summary>
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
        
        /// <summary>
        /// Indica se a validação passou (sem erros)
        /// </summary>
        public bool IsValid => !_errors.Any();
        
        /// <summary>
        /// Indica se há warnings
        /// </summary>
        public bool HasWarnings => _warnings.Any();
        
        /// <summary>
        /// Adiciona um erro à validação
        /// </summary>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                _errors.Add(error);
            }
        }
        
        /// <summary>
        /// Adiciona um warning à validação
        /// </summary>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                _warnings.Add(warning);
            }
        }
        
        /// <summary>
        /// Adiciona múltiplos erros
        /// </summary>
        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                AddError(error);
            }
        }
        
        /// <summary>
        /// Adiciona múltiplos warnings
        /// </summary>
        public void AddWarnings(IEnumerable<string> warnings)
        {
            foreach (var warning in warnings)
            {
                AddWarning(warning);
            }
        }
    }
}

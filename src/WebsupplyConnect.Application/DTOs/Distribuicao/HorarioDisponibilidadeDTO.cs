namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para horários de disponibilidade de distribuição
    /// </summary>
    public class HorarioDisponibilidadeDTO
    {
        /// <summary>
        /// Data do horário
        /// </summary>
        public DateTime Data { get; set; }
        
        /// <summary>
        /// Dia da semana (1=Segunda, 2=Terça, etc.)
        /// </summary>
        public int DiaSemanaId { get; set; }
        
        /// <summary>
        /// Nome do dia da semana
        /// </summary>
        public string NomeDia { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se há distribuição neste dia
        /// </summary>
        public bool DistribuicaoAtiva { get; set; }
        
        /// <summary>
        /// Horário de início da distribuição
        /// </summary>
        public string? HorarioInicio { get; set; }
        
        /// <summary>
        /// Horário de fim da distribuição
        /// </summary>
        public string? HorarioFim { get; set; }
        
        /// <summary>
        /// Indica se é feriado
        /// </summary>
        public bool EhFeriado { get; set; }
        
        /// <summary>
        /// Nome do feriado (se aplicável)
        /// </summary>
        public string? NomeFeriado { get; set; }
        
        /// <summary>
        /// Motivo da indisponibilidade (se houver)
        /// </summary>
        public string? MotivoIndisponibilidade { get; set; }
    }
}

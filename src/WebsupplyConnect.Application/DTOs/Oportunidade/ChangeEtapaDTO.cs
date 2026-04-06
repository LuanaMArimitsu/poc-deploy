namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class ChangeEtapaDTO
    {
        public int EtapaDestinoId { get; set; }
        /// <summary>Obrigatório quando destino é vitória; opcional nos demais.</summary>
        public decimal? ValorFinalVenda { get; set; }
        /// <summary>Obrigatório em: perda, reabertura (sair de etapa final), regressão.</summary>
        public string? Observacao { get; set; }
        public int EmpresaId { get; set; }
    }

}

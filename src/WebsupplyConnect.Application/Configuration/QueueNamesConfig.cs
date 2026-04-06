namespace WebsupplyConnect.Application.Configuration
{
    /// <summary>
    /// Conjunto de constantes globais utilizadas para definir o nome de cada fila
    /// </summary>
    public static class QueueNamesConfig
    {
        public const string WebhookInboundMeta = "WebhookInboundMeta";
        public const string MidiasInbound = "MidiasInbound";
        public const string MensagensOutbound = "MensagensOutbound";
        public const string MidiasOutbound = "MidiasOutbound";
        public const string Notificacoes = "Notificacoes";
        public const string Default = "Default";
    }
}

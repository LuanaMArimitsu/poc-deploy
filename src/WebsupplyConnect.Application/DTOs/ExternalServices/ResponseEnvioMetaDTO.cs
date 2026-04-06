namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class ResponseEnvioMetaDTO
    {
        public string messaging_product { get; set; }
        public Contact[] contacts { get; set; }
        public Message[] messages { get; set; }
    }

    public class Contact
    {
        public string input { get; set; }
        public string wa_id { get; set; }
    }

    public class Message
    {
        public string id { get; set; }
        public string message_status { get; set; }
    }

}

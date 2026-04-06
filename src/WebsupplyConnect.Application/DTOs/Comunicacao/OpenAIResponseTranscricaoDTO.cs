using Newtonsoft.Json;

namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class OpenAIResponseTranscricaoDTO
    {
        public class Rootobject
        {
            public string text { get; set; }
            public Usage usage { get; set; }
        }

        public class Usage
        {
            public string type { get; set; }
            public int input_tokens { get; set; }
            public Input_Token_Details input_token_details { get; set; }
            public int output_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public class Input_Token_Details
        {
            public int text_tokens { get; set; }
            public int audio_tokens { get; set; }
        }
    }
}

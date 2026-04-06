using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.Firebase
{
    public class PushNotificationService(ILogger<PushNotificationService> logger) : IPushNotificationService
    {
        private readonly HttpClient _httpClient = new();

        private async Task<string> GerarAccessToken()
        {
            var credenciais = @"{
                ""type"": ""service_account"",
                ""project_id"": ""loginmicrosft-94c88"",
                ""private_key_id"": ""daa1f4456f8fd7ce9b0197508975b0e0697b4051"",
                ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDoCjAHT5LuywN7\nq0ua+XJVwYmSmpwvtjIsfoohXKGbYLtrKu9T4XLg2dg7u3Z9+AihGtPa0pYWYFpL\nlFOOhYRcPYJRyXDN5p79xGGnWMYzf9R98S/MV3qjADAor/NEEcZOb/1nW8MOKEv3\nLHIf+z7v+9ZZibEkCCcP3mzX/ISaY7L8DkNDSmWDlMe4StVxzTDDFrZNzQRukB8D\ndLuqHQd7EUdCJ7fmNrO+xroGo+kKo9Rksklq0GrGFKpTxcMi8t/RGLEkp1cRNmRy\nrOWjs5C/Mn326S/U+kvghJCr4FIEUOF0hq5z2l5qYpduEbUxI0J9xXHCrZiWWwan\n45aWPsLRAgMBAAECggEAGwvs8z00PD9PpZ+ezW6cBCDt/zekUu9iw8rwINliQPEy\nh1hW0ykpMcpSqQu90QsTPmwZG52GPw92Fu8wGiG3/uRwh0X6rxVdnOjCFTaEy8Xv\np8pwLtpXgh5ofqWbrmh/++6T0/NfNgw+Zo44sz+e42wwXPluu2tz2iar+zUE49XY\nWkSzpawhm/X58q4asB6igb0dIFMaKbQH1O8fFRdS9lRuGwe8o/fzRUMD2fp2uMl7\nWB2ve8E8sAaiT445aF3YsI33wVoGD3+0WxXP66d1l/mV6PMk8gqwEzGiJcEDIxf7\nygpIyHlUWt5lfcgNLyEvaP2+2qiH9lNMao6QCZS1rQKBgQD1RCaPtF4gesDAMTFM\ni7u5+8JMWDUAZXbDQ15iubIzXWWYo9HPhOeIPIxRia8GkDRUeTGletKfeUQx3twW\nBn1T+wVjjpL6ZUfYlmZFaOow5ibxAOMDvpGuG4vDgmG9eouscPfIdvaFAe0VWLIZ\nwpsWecfwuwF1MYrVWVLoQYrZ4wKBgQDyMdqzlErU7tYtX+OALZKQzhyncIrlaXC0\n9gsnXg5EF4ZVuoZPeUAulbBvOWs5+Ga3D6CxBqevte5z5IYt+rzCAxgElfyFXxib\n63wcejZqONnBWzIcOuHcEpiTiqQeGjQ8r/norUtw+i22fZyiVEeK0NhpQZonp5gr\nNmdOlOMeuwKBgQCi5F5P9tTE8YHuoz1Av1Uwklpa5gJdfwW/bZDUNMx7fL4rADIq\nhvRW8Q+oX68UxtVafRtR8h7Mt3dpP8AgCLNYAVF0644GKxnqaQkHdESFsXWPfq1H\nIVwTrEvIz2EmvKrjHiwSwZ+8eqkBEmVG4o6qALuf0DOJqBuy0p5TjqQvTQKBgHLr\nlwOo9M0Ouw/ytOdoOGh/dHc63p02p+Ul7mrypUBIDVT2Wa6yMPMp0fskuq1aIZrx\nTmVRbBXi9M+G+ugsVo6Umzvp01WRpwKs/Uoh71n9uc2WsTNV+T/MjxtLKM6jzm+R\nbLqsJ+TmwPQbrEwWQ5ApwtZG65evXXP7r49I9G/rAoGBAIIyXFuqIVEGE8/6sgGB\nIrwHNHnDxLBcbLjoK1819LE3bq7E8pAKBuV3UawAQvG4U9hhHTw5jotdZ3fsm/JR\nEedKxu4kyFxEPft5/xhLnqfUzBd3SNbMCmR3hIN1EIB6aFo6/9yROjA3kWSusE+x\nww/5LtcTK19xM52NFaIU+dxE\n-----END PRIVATE KEY-----\n"",
                ""client_email"": ""firebase-adminsdk-fbsvc@loginmicrosft-94c88.iam.gserviceaccount.com"",
                ""client_id"": ""101852820975098010715"",
                ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                ""token_uri"": ""https://oauth2.googleapis.com/token"",
                ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40loginmicrosft-94c88.iam.gserviceaccount.com"",
                ""universe_domain"": ""googleapis.com""
            }";

            var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(credenciais));
            var credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }

        public async Task SendToDeviceAsync(string deviceToken, NotificacaoDTO notificacao)
        {
            try
            {
                var accessToken = await GerarAccessToken();
                var url = $"https://fcm.googleapis.com/v1/projects/loginmicrosft-94c88/messages:send";

                var mensagemJson = new
                {
                    message = new
                    {
                        token = deviceToken,
                        notification = new
                        {
                            title = notificacao.Title,
                            body = notificacao.Content
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(mensagemJson), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                logger.LogWarning(ex, "Falha ao enviar via Push");
            }
        }
    }
}

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GymApp.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public WebhookService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _webhookUrl = configuration["n8n:WebhookUrl"] ?? string.Empty;
        }

        public async Task EnviarAlertaInstantaneaAsync(string tipo, object datos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return;

            var payload = new
            {
                Type = tipo,
                Data = datos,
                ChatId = chatId
            };

            await SendWebhookAsync(payload);
        }

        public async Task EnviarReporteProgramadoAsync(object resumenDatos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return;

            var payload = new
            {
                Type = "ReporteProgramado",
                Data = resumenDatos,
                ChatId = chatId
            };

            await SendWebhookAsync(payload);
        }

        private async Task SendWebhookAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await _httpClient.PostAsync(_webhookUrl, content);
            }
            catch
            {
                // Manejar error o registrar log según sea necesario
            }
        }
    }
}

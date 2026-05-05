using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using GymApp.Configuration;

namespace GymApp.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public WebhookService(IHttpClientFactory httpClientFactory, IOptions<N8nSettings> n8nSettings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _webhookUrl = n8nSettings.Value.WebhookUrl;
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

        public async Task<bool> EnviarMensajePruebaAsync(string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return false;

            var payload = new
            {
                chatId = chatId,
                mensaje = "🟢 *Prueba Exitosa:* GymApp está conectado correctamente a este chat vía n8n."
            };

            return await SendWebhookAsync(payload);
        }

        private async Task<bool> SendWebhookAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
                // Manejar error o registrar log según sea necesario
            }
        }
    }
}

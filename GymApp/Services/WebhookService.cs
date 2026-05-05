using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GymApp.Configuration;

namespace GymApp.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(IHttpClientFactory httpClientFactory, IOptions<N8nSettings> n8nSettings, ILogger<WebhookService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _webhookUrl = n8nSettings.Value.WebhookUrl;
            _logger = logger;
        }

        public async Task EnviarAlertaInstantaneaAsync(string tipo, object datos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return;

            var payload = new
            {
                Evento = tipo,
                ChatId = chatId,
                Datos = datos
            };

            await SendWebhookAsync(payload);
        }

        public async Task EnviarReporteProgramadoAsync(object resumenDatos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return;

            var payload = new
            {
                Evento = "RESUMEN_PROGRAMADO",
                ChatId = chatId,
                Datos = resumenDatos
            };

            await SendWebhookAsync(payload);
        }

        public async Task<bool> EnviarMensajePruebaAsync(string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                _logger.LogWarning("Webhook URL no está configurada.");
                return false;
            }

            var payload = new
            {
                Evento = "PING",
                ChatId = chatId,
                Datos = new { mensaje = "Prueba de conexión" }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Enviando mensaje de prueba a: {Url}", _webhookUrl);
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Éxito al enviar mensaje de prueba. Respuesta: {ResponseBody}", responseBody);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al enviar el webhook. StatusCode: {StatusCode}, Respuesta: {ResponseBody}", response.StatusCode, responseBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al enviar webhook de prueba a n8n.");
                return false;
            }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SendWebhookAsync.");
                return false;
            }
        }
    }
}

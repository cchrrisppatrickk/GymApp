using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GymApp.Configuration;
using GymApp.Models;
using GymApp.Repositories;
using System.Linq;

namespace GymApp.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly ILogger<WebhookService> _logger;
        private readonly IConfiguracionAlertaRepository _configRepo;

        public WebhookService(
            IHttpClientFactory httpClientFactory, 
            IOptions<N8nSettings> n8nSettings, 
            ILogger<WebhookService> logger,
            IConfiguracionAlertaRepository configRepo)
        {
            _httpClient = httpClientFactory.CreateClient();
            _webhookUrl = n8nSettings.Value.WebhookUrl;
            _logger = logger;
            _configRepo = configRepo;
        }

        public async Task NotificarNuevoUsuarioAsync(Usuario usuario)
        {
            var configs = (await _configRepo.GetAllAsync())
                .Where(c => c.Activo && c.AvisarNuevoUsuario);

            foreach (var config in configs)
            {
                await EnviarAlertaInstantaneaAsync("NUEVO_USUARIO", new
                {
                    NombreCompleto = usuario.NombreCompleto,
                    FechaRegistro = usuario.FechaRegistro
                }, config.ChatIdDestino);
            }
        }

        public async Task NotificarNuevoPagoAsync(decimal monto, string cliente, string metodoPago)
        {
            var configs = (await _configRepo.GetAllAsync())
                .Where(c => c.Activo && c.AvisarNuevoPago);

            foreach (var config in configs)
            {
                await EnviarAlertaInstantaneaAsync("NUEVO_PAGO", new
                {
                    Monto = monto,
                    Cliente = cliente,
                    MetodoPago = metodoPago
                }, config.ChatIdDestino);
            }
        }

        public async Task NotificarNuevaMembresiaAsync(string cliente, string plan, decimal precio)
        {
            var configs = (await _configRepo.GetAllAsync())
                .Where(c => c.Activo && c.AvisarNuevaMembresia);

            foreach (var config in configs)
            {
                await EnviarAlertaInstantaneaAsync("NUEVA_MEMBRESIA", new
                {
                    Cliente = cliente,
                    Plan = plan,
                    Precio = precio
                }, config.ChatIdDestino);
            }
        }

        public async Task<bool> EnviarAlertaInstantaneaAsync(string tipo, object datos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return false;

            var payload = new
            {
                Evento = tipo,
                ChatId = chatId,
                Datos = datos
            };

            return await SendWebhookAsync(payload);
        }

        public async Task<bool> EnviarReporteProgramadoAsync(object resumenDatos, string chatId)
        {
            if (string.IsNullOrEmpty(_webhookUrl)) return false;

            var payload = new
            {
                Evento = "RESUMEN_PROGRAMADO",
                ChatId = chatId,
                Datos = resumenDatos
            };

            return await SendWebhookAsync(payload);
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

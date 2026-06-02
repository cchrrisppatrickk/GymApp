using GymApp.ViewModels;
using System.Net.Http.Json;

namespace GymApp.Services
{
    public class PeruApiService : IPeruApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PeruApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PeruApiDniResponse?> ConsultarDniAsync(string dni)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("PERU_API_KEY") ?? _configuration["PeruApi:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("API Key de PeruAPI no configurada.");
                }

                // Usamos la variante por query string para mayor simplicidad en el HttpClient si se prefiere,
                // pero la documentación recomienda cabecera.
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://peruapi.com/api/dni/{dni}");
                request.Headers.Add("X-API-KEY", apiKey);

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PeruApiDniResponse>();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
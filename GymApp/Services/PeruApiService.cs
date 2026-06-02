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
                dni = dni.Trim();
                var apiKey = Environment.GetEnvironmentVariable("PERU_API_KEY") ?? _configuration["PeruApi:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return new PeruApiDniResponse { Mensaje = "API Key no configurada", Code = "401" };
                }

                // Usamos la variante por query string (?api_token=) que es más robusta en algunos entornos
                var url = $"https://peruapi.com/api/dni/{dni}?api_token={apiKey}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PeruApiDniResponse>();
                }
                
                // Intentar leer el error si existe
                var errorContent = await response.Content.ReadAsStringAsync();
                return new PeruApiDniResponse 
                { 
                    Mensaje = $"Error API: {response.StatusCode} - {errorContent}", 
                    Code = ((int)response.StatusCode).ToString() 
                };
            }
            catch (Exception ex)
            {
                return new PeruApiDniResponse { Mensaje = $"Excepción: {ex.Message}", Code = "500" };
            }
        }
    }
}
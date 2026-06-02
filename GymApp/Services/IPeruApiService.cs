using GymApp.ViewModels;

namespace GymApp.Services
{
    public interface IPeruApiService
    {
        Task<PeruApiDniResponse?> ConsultarDniAsync(string dni);
    }
}
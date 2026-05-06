using System.Threading.Tasks;
using GymApp.Models;

namespace GymApp.Services
{
    public interface IWebhookService
    {
        Task<bool> EnviarAlertaInstantaneaAsync(string tipo, object datos, string chatId);
        Task<bool> EnviarReporteProgramadoAsync(object resumenDatos, string chatId);
        Task<bool> EnviarMensajePruebaAsync(string chatId);
        
        // Métodos de dominio que consultan configuraciones
        Task NotificarNuevoUsuarioAsync(Usuario usuario);
        Task NotificarNuevoPagoAsync(decimal monto, string cliente, string metodoPago);
        Task NotificarNuevaMembresiaAsync(string cliente, string plan, decimal precio);
    }
}

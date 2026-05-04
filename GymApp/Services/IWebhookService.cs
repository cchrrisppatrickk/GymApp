using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IWebhookService
    {
        Task EnviarAlertaInstantaneaAsync(string tipo, object datos, string chatId);
        Task EnviarReporteProgramadoAsync(object resumenDatos, string chatId);
    }
}

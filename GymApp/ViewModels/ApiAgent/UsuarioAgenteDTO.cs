namespace GymApp.ViewModels.ApiAgent;

/// <summary>
/// DTO ultra-ligero para consultas de identidad de usuarios desde el agente IA (n8n).
/// Contiene únicamente los campos de identificación necesarios para no saturar
/// la ventana de contexto del LLM. Excluye intencionalmente: PasswordHash, FotoUrl,
/// CodigoQr, Email, Role y colecciones de navegación.
/// </summary>
public class UsuarioAgenteDTO
{
    public int      Id             { get; set; }
    public string   NombreCompleto { get; set; }
    public string   DNI            { get; set; }
    public string   Telefono       { get; set; }
    public DateTime FechaRegistro  { get; set; }
}

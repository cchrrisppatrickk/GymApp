namespace GymApp.ViewModels.ApiAgent;

public class DeudaUsuarioAgenteDTO
{
    public decimal DeudaTotal { get; set; }
    public int MembresiasConDeuda { get; set; } // Cantidad de membresías que aún no se pagan por completo
}

using GymApp.Models;
using GymApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IMembresiaService
    {
        Task<IEnumerable<MembresiaListDTO>> ListarMembresiasAsync(string filtro);
        Task CrearMembresiaAsync(MembresiaCreateDTO dto);
        Task<IEnumerable<object>> BuscarClientesAsync(string termino); // Para el autocomplete

        // Nuevo: Verifica si el Turno seleccionado ya está asignado a otra membresía activa
        Task VerificarTurnoExistente(int userId, int turnoId);
    }

}
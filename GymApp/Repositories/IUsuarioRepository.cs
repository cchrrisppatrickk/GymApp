using GymApp.Models; // Asegúrate de que apunte a donde están tus clases generadas
using System;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    // Heredamos de IGenericRepository<Usuario> para tener ya el CRUD básico
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        // Método 1: Búsqueda por DNI (Vital para el registro y login)
        Task<Usuario> ObtenerPorDNIAsync(string dni);

        // Método 2: Validar si existe (Más ligero que traer todo el objeto)
        Task<bool> ExisteDniAsync(string dni);

        // Método 3: EL CORAZÓN DEL SISTEMA - Búsqueda por QR
        // Recibe el Guid y busca al usuario activo
        Task<Usuario> ObtenerPorQRAsync(Guid codigoQR);

        // Método 4: Obtener usuario con su Rol cargado (Eager Loading)
        // El GetById genérico no trae el nombre del rol ("Admin"), este sí.
        Task<Usuario> ObtenerConDetallesAsync(int id);

        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    }
}
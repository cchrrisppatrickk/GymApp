using GymApp.Repositories;
using GymApp.Models;

namespace GymApp.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<Role>> ObtenerTodosAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<Role> ObtenerPorIdAsync(int id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task CrearRolAsync(Role role)
        {
            // Regla de negocio: Validar nombres
            role.Nombre = role.Nombre.Trim();
            await _roleRepository.InsertAsync(role);
            await _roleRepository.SaveAsync();
        }

        public async Task ActualizarRolAsync(Role role)
        {
            // Validaciones adicionales si fueran necesarias
            await _roleRepository.UpdateAsync(role);
            await _roleRepository.SaveAsync();
        }

        public async Task<bool> EliminarRolAsync(int id)
        {
            var rol = await _roleRepository.GetByIdAsync(id);
            if (rol == null) return false;

            // --- REGLA DE NEGOCIO CRÍTICA ---
            // Evitar borrar roles base del sistema
            if (rol.Nombre == "Admin" || rol.Nombre == "Portero" || rol.Nombre == "Cliente")
            {
                // No permitimos borrar estos roles porque romperían la lógica del Login
                throw new InvalidOperationException("No se pueden eliminar los roles del sistema.");
            }

            await _roleRepository.DeleteAsync(id);
            await _roleRepository.SaveAsync();
            return true;
        }
    }
}
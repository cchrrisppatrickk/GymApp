using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    // T debe ser una clase (una entidad de nuestra BD)
    public interface IGenericRepository<T> where T : class
    {
        // Obtener todos los registros
        Task<IEnumerable<T>> GetAllAsync();

        // Obtener uno por su ID
        Task<T> GetByIdAsync(int id);

        // Insertar nuevo registro
        Task InsertAsync(T entity);

        // Actualizar registro existente
        Task UpdateAsync(T entity);

        // Eliminar por ID
        Task DeleteAsync(int id);

        // Guardar cambios (Commit)
        Task SaveAsync();

        System.Linq.IQueryable<T> GetQueryable();
    }
}
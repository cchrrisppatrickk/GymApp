using GymApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // Inyección de dependencias del Contexto de BD
        private readonly GymDbContext _context;
        private readonly DbSet<T> _table;

        public GenericRepository(GymDbContext context)
        {
            _context = context;
            // Vinculamos la tabla correspondiente a la entidad T
            _table = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Retorna la lista completa de la tabla
            return await _table.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            // Busca por Primary Key
            return await _table.FindAsync(id);
        }

        public async Task InsertAsync(T entity)
        {
            // Agrega a la memoria del contexto
            await _table.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            // Marca la entidad como modificada para el siguiente Save
            _table.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            // Nota: Update en EF Core no es async por naturaleza, 
            // pero retornamos Task para mantener consistencia en la interfaz.
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            // Primero buscamos la entidad para asegurarnos que existe
            T existing = await _table.FindAsync(id);
            if (existing != null)
            {
                _table.Remove(existing);
            }
        }

        public async Task SaveAsync()
        {
            // Impacta los cambios (Insert/Update/Delete) en la BD SQL Server
            await _context.SaveChangesAsync();
        }
    }
}
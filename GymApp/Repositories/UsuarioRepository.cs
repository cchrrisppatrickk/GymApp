using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    // Heredamos de GenericRepository para reutilizar código
    // E implementamos IUsuarioRepository para las funciones nuevas
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        private readonly GymDbContext _context;

        public UsuarioRepository(GymDbContext context) : base(context)
        {
            // Guardamos el contexto localmente por si necesitamos acceso directo
            // más allá de lo que ofrece el genérico
            _context = context;
        }

        public async Task<Usuario> ObtenerPorDNIAsync(string dni)
        {
            // Busca el primer usuario que coincida con el DNI
            return await _context.Usuarios
                                 .Include(u => u.Role) // Traemos el nombre del Rol
                                 .FirstOrDefaultAsync(u => u.Dni == dni);
        }

        public async Task<bool> ExisteDniAsync(string dni)
        {
            // Consulta optimizada: solo verifica existencia, no descarga datos
            return await _context.Usuarios.AnyAsync(u => u.Dni == dni);
        }

        public async Task<Usuario> ObtenerPorQRAsync(Guid codigoQR)
        {
            // Esta función la usará el escáner de la tablet
            return await _context.Usuarios
                                 .Include(u => u.Role)
                                 // Filtramos por QR y aseguramos que el usuario no esté borrado lógicamente (Estado)
                                 .FirstOrDefaultAsync(u => u.CodigoQr == codigoQR && u.Estado == true);
        }

        public async Task<Usuario> ObtenerConDetallesAsync(int id)
        {
            // Sobrescribe la lógica de GetById para incluir relaciones
            return await _context.Usuarios
                                 .Include(u => u.Role)
                                 // Aquí podrías incluir también .Include(u => u.Membresias) si quisieras
                                 .FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
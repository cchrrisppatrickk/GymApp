using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class RestriccionService : IRestriccionService
    {
        private readonly GymDbContext _context;

        public RestriccionService(GymDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RestriccionUsuario>> ObtenerPorUsuarioAsync(int userId)
        {
            return await _context.RestriccionesUsuarios
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.FechaAplicacion)
                .ToListAsync();
        }

        public async Task<RestriccionUsuario> AplicarRestriccionAsync(int userId, string tipo, string descripcion, int usuarioAplicadorId)
        {
            var restriccion = new RestriccionUsuario
            {
                UserId = userId,
                TipoRestriccion = tipo,
                Descripcion = descripcion,
                FechaAplicacion = DateTime.Now,
                UsuarioAplicadorId = usuarioAplicadorId,
                EstadoActiva = true
            };

            await _context.RestriccionesUsuarios.AddAsync(restriccion);
            await _context.SaveChangesAsync();
            return restriccion;
        }

        public async Task<bool> LevantarRestriccionAsync(int restriccionId)
        {
            var restriccion = await _context.RestriccionesUsuarios.FindAsync(restriccionId);
            if (restriccion == null) return false;

            restriccion.EstadoActiva = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsuarioTieneRestriccionesActivasAsync(int userId)
        {
            return await _context.RestriccionesUsuarios
                .AnyAsync(r => r.UserId == userId && r.EstadoActiva);
        }
    }
}

using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Repositories
{
    public class ConfiguracionAlertaRepository : IConfiguracionAlertaRepository
    {
        private readonly GymDbContext _context;

        public ConfiguracionAlertaRepository(GymDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la configuración global (siempre el primer registro).
        /// Si no existe ninguno, retorna un objeto en memoria con valores por defecto (Id=0).
        /// </summary>
        public async Task<ConfiguracionAlerta> ObtenerConfiguracionGlobalAsync()
        {
            var config = await _context.ConfiguracionAlertas.FirstOrDefaultAsync();

            // Si no existe registro en BD, retornar objeto vacío (Id=0, nunca se guarda aquí)
            return config ?? new ConfiguracionAlerta { Id = 0, Activo = false };
        }

        /// <summary>
        /// Guarda la configuración global.
        /// - Si ya existe un registro: actualiza el objeto trackeado por EF (sin conflicto de IDENTITY).
        /// - Si no existe: inserta sin forzar el Id (SQL Server lo asigna automáticamente).
        /// </summary>
        public async Task<bool> GuardarConfiguracionGlobalAsync(ConfiguracionAlerta configuracion)
        {
            var existente = await _context.ConfiguracionAlertas.FirstOrDefaultAsync();

            if (existente != null)
            {
                // UPDATE: copiar valores al objeto ya trackeado por EF Context
                existente.HoraEnvio               = configuracion.HoraEnvio;
                existente.DiasSemana              = configuracion.DiasSemana;
                existente.ChatIdDestino           = configuracion.ChatIdDestino;
                existente.Activo                  = configuracion.Activo;

                // Reportes programados
                existente.EnviarNuevosMiembros        = configuracion.EnviarNuevosMiembros;
                existente.EnviarProximosVencimientos  = configuracion.EnviarProximosVencimientos;
                existente.EnviarDeudasPendientes      = configuracion.EnviarDeudasPendientes;
                existente.EnviarPagosHoy              = configuracion.EnviarPagosHoy;

                // Tiempo real
                existente.AvisarNuevoUsuario   = configuracion.AvisarNuevoUsuario;
                existente.AvisarNuevoPago      = configuracion.AvisarNuevoPago;
                existente.AvisarNuevaMembresia = configuracion.AvisarNuevaMembresia;
            }
            else
            {
                // INSERT: NO asignar Id explícito — SQL Server genera el valor IDENTITY
                configuracion.Id = 0; // Forzar default para que EF no envíe el valor
                await _context.ConfiguracionAlertas.AddAsync(configuracion);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Devuelve la configuración global si está activa y coincide con la hora/día actual.
        /// Usado por el job de notificaciones programadas.
        /// </summary>
        public async Task<IEnumerable<ConfiguracionAlerta>> ObtenerAlertasParaEjecutarAsync(TimeSpan horaActual, string diaSemana)
        {
            var horaComparar = new TimeSpan(horaActual.Hours, horaActual.Minutes, 0);

            return await _context.ConfiguracionAlertas
                .Where(c => c.Activo &&
                            c.HoraEnvio.Hours   == horaComparar.Hours &&
                            c.HoraEnvio.Minutes == horaComparar.Minutes &&
                            c.DiasSemana.Contains(diaSemana))
                .ToListAsync();
        }
    }
}

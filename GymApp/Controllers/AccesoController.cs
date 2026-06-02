// Controllers/AccesoController.cs
using Microsoft.AspNetCore.Mvc;
using GymApp.Constants;
using Microsoft.EntityFrameworkCore;
using GymApp.Data; // Tu namespace de datos
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace GymApp.Controllers
{
    [Authorize(Policy = AppPoliticas.RequiereVerAcceso)]
    public class AccesoController : BaseController
    {
        private readonly GymDbContext _context;

        public AccesoController(GymDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Retorna la vista con la cámara
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] string qrToken)
        {
            if (string.IsNullOrEmpty(qrToken))
                return Json(new { success = false, message = "QR inválido" });

            // 1. Buscar Usuario por QR
            // Convertimos el string del QR a Guid
            if (!Guid.TryParse(qrToken, out Guid qrGuid))
                return Json(new { success = false, message = "Formato de QR incorrecto" });

            var usuario = await _context.Usuarios
                .Include(u => u.Membresia)
                .ThenInclude(m => m.Plan)
                .Include(u => u.Restricciones)
                .FirstOrDefaultAsync(u => u.CodigoQr == qrGuid);

            if (usuario == null)
                return Json(new { success = false, message = "Usuario no encontrado" });

            // 1.5 Verificar Restricciones de Seguridad
            var restriccionActiva = usuario.Restricciones.FirstOrDefault(r => r.EstadoActiva);
            if (restriccionActiva != null)
            {
                // Registrar intento fallido por restricción
                var asistenciaFallo = new Asistencia
                {
                    UserId = usuario.UserId,
                    FechaHora = DateTime.Now,
                    AccesoPermitido = false,
                    MotivoDenegacion = $"BLOQUEADO: {restriccionActiva.TipoRestriccion}"
                };
                _context.Asistencias.Add(asistenciaFallo);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = false,
                    nombre = usuario.NombreCompleto,
                    mensaje = $"ACCESO DENEGADO: {restriccionActiva.TipoRestriccion}",
                    motivo = restriccionActiva.Descripcion
                });
            }

            // 2. Buscar Membresía Activa (La que vence en el futuro)
            var membresia = usuario.Membresia
                .Where(m => m.Estado == "Activa" && m.FechaVencimiento >= DateOnly.FromDateTime(DateTime.Today))
                .OrderByDescending(m => m.FechaVencimiento)
                .FirstOrDefault();

            bool accesoPermitido = false;
            string mensaje = "";
            int diasRestantes = 0;
            string nombrePlan = "Sin Plan";

            if (membresia != null)
            {
                // Calcular días restantes (No restamos uso, es calendario)
                // Usamos ToDateTime para operar fechas
                diasRestantes = (membresia.FechaVencimiento.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
                nombrePlan = membresia.Plan.Nombre;
                accesoPermitido = true;
                mensaje = "Bienvenido";
            }
            else
            {
                accesoPermitido = false;
                mensaje = "Membresía Vencida o Inexistente";
            }

            // 3. Registrar en Historial (Tabla Asistencias)
            var asistencia = new Asistencia
            {
                UserId = usuario.UserId,
                FechaHora = DateTime.Now,
                AccesoPermitido = accesoPermitido,
                MotivoDenegacion = accesoPermitido ? "OK" : mensaje
            };

            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();

            // 4. Retornar DTO al Frontend
            return Json(new
            {
                success = accesoPermitido,
                nombre = usuario.NombreCompleto,
                plan = nombrePlan,
                diasRestantes = diasRestantes,
                mensaje = mensaje,
                fotoUrl = "/img/default-avatar.png" // Opcional si tienes fotos
            });
        }
    }
}
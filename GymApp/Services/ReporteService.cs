using GymApp.Data;
using GymApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class ReporteService : IReporteService
    {
        private readonly GymDbContext _context;

        public ReporteService(GymDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReporteIngresosDTO>> ObtenerReporteMensualAsync(int mes, int anio)
        {
            // 1. Traer datos crudos (Raw Data)
            // Se mantiene tu lógica de validación de nulos (HasValue)
            var ventasDelMes = await _context.VentasDetalles
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                .Where(d => d.Venta.FechaVenta.HasValue &&
                            d.Venta.FechaVenta.Value.Month == mes &&
                            d.Venta.FechaVenta.Value.Year == anio)
                .Select(d => new
                {
                    Dia = d.Venta.FechaVenta.Value.Day,
                    Hora = d.Venta.FechaVenta.Value.Hour,
                    Categoria = d.Producto.Categoria,
                    Metodo = d.Venta.MetodoPago,
                    Total = d.Subtotal
                })
                .ToListAsync();

            // 2. Agrupación y Cálculo en Memoria
            // CAMBIO CLAVE: Usamos .Contains("Yape") en lugar de == "Yape"
            var reporte = ventasDelMes
                .GroupBy(v => v.Dia)
                .Select(g => new ReporteIngresosDTO
                {
                    Dia = g.Key,

                    // --- BEBIDAS ---
                    BebidasMananaEfectivo = g.Where(x => (x.Categoria == "General" || x.Categoria == "Bebida") && x.Hora < 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    BebidasMananaYape = g.Where(x => (x.Categoria == "General" || x.Categoria == "Bebida") && x.Hora < 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),

                    BebidasTardeEfectivo = g.Where(x => (x.Categoria == "General" || x.Categoria == "Bebida") && x.Hora >= 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    BebidasTardeYape = g.Where(x => (x.Categoria == "General" || x.Categoria == "Bebida") && x.Hora >= 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),

                    // --- LIBRES ---
                    LibresMananaEfectivo = g.Where(x => x.Categoria == "Servicio" && x.Hora < 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    LibresMananaYape = g.Where(x => x.Categoria == "Servicio" && x.Hora < 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),

                    LibresTardeEfectivo = g.Where(x => x.Categoria == "Servicio" && x.Hora >= 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    LibresTardeYape = g.Where(x => x.Categoria == "Servicio" && x.Hora >= 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),

                    // --- XB ---
                    XBMananaEfectivo = g.Where(x => x.Categoria == "XB" && x.Hora < 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    XBMananaYape = g.Where(x => x.Categoria == "XB" && x.Hora < 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),

                    XBTardeEfectivo = g.Where(x => x.Categoria == "XB" && x.Hora >= 14 && x.Metodo == "Efectivo").Sum(x => x.Total),
                    // CORREGIDO:
                    XBTardeYape = g.Where(x => x.Categoria == "XB" && x.Hora >= 14 && x.Metodo.Contains("Yape")).Sum(x => x.Total),
                })
                .OrderBy(r => r.Dia)
                .ToList();

            return reporte;
        }

        public async Task<List<ReporteMembresiaDTO>> ObtenerReporteMembresiasAsync(int mes, int anio)
        {
            var data = await _context.Membresias
                .Include(m => m.User)          
                .Include(m => m.Plan)
                .Include(m => m.Turno)
                .Include(m => m.PagosMembresia)
                .Where(m => m.FechaInicio.Month == mes && m.FechaInicio.Year == anio)
                .OrderByDescending(m => m.FechaInicio)
                .Select(m => new ReporteMembresiaDTO
                {
                    MembresiaId = m.MembresiaId,
                    NombreCliente = m.User.NombreCompleto,
                    Telefono = m.User.Telefono ?? "-",
                    FechaInicio = m.FechaInicio.ToString("dd/MM/yyyy"),
                    FechaFin = m.FechaVencimiento.ToString("dd/MM/yyyy"),
                    NombrePlan = m.Plan.Nombre,
                    NombreTurno = m.Turno.Nombre,
                    PagadoEfectivo = m.PagosMembresia
                        .Where(p => p.MetodoPago == "Efectivo")
                        .Sum(p => p.Monto),
                    PagadoYape = m.PagosMembresia
                        .Where(p => p.MetodoPago.Contains("Yape"))
                        .Sum(p => p.Monto),
                    Observaciones = m.Observaciones ?? "",
                    Estado = m.Estado
                })
                .ToListAsync();

            return data;
        }

        public async Task<DashboardUserStatsDTO> ObtenerEstadisticasUsuariosAsync()
        {
            var hoyDt = DateTime.Now;
            var hoy = DateOnly.FromDateTime(hoyDt);
            var finSemana = hoy.AddDays(7);
            var mesActual = hoyDt.Month;
            var anioActual = hoyDt.Year;

            // 1. Nuevos Miembros del Mes
            int nuevos = await _context.Usuarios
                .AsNoTracking()
                .CountAsync(u => u.FechaRegistro.HasValue && 
                                 u.FechaRegistro.Value.Month == mesActual && 
                                 u.FechaRegistro.Value.Year == anioActual);

            // 2. Vencidos Sin Renovar
            // Buscamos usuarios que tienen alguna membresía vencida pero NINGUNA activa o futura
            var usuariosConMembresia = await _context.Membresias
                .AsNoTracking()
                .GroupBy(m => m.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TieneActivaOFutura = g.Any(m => m.Estado == "Activa" || m.FechaInicio > hoy),
                    TieneVencida = g.Any(m => m.Estado == "Vencida")
                })
                .ToListAsync();

            int sinRenovar = usuariosConMembresia.Count(u => u.TieneVencida && !u.TieneActivaOFutura);

            // 3. Por Vencer en 7 Días
            int porVencer = await _context.Membresias
                .AsNoTracking()
                .CountAsync(m => m.Estado == "Activa" && 
                                 m.FechaVencimiento >= hoy && 
                                 m.FechaVencimiento <= finSemana);

            // 4. Deudas
            // Traemos membresías con su precio de plan y total pagado
            var deudasData = await _context.Membresias
                .AsNoTracking()
                .Select(m => new
                {
                    m.UserId,
                    PrecioBase = m.Plan.PrecioBase,
                    TotalPagado = m.PagosMembresia.Sum(p => p.Monto)
                })
                .ToListAsync();

            var listaDeudores = deudasData
                .Select(d => new { d.UserId, Deuda = d.PrecioBase - d.TotalPagado })
                .Where(x => x.Deuda > 0)
                .ToList();

            // 5. Membresías Congeladas
            int congeladas = await _context.Membresias
                .AsNoTracking()
                .CountAsync(m => m.Estado == "Congelada");

            return new DashboardUserStatsDTO
            {
                NuevosMiembrosMes = nuevos,
                VencidosSinRenovar = sinRenovar,
                PorVencer7Dias = porVencer,
                UsuariosConDeuda = listaDeudores.Select(x => x.UserId).Distinct().Count(),
                MontoTotalDeuda = listaDeudores.Sum(x => x.Deuda),
                MembresiasCongeladas = congeladas
            };
        }
    }
}
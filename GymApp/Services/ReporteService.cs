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
    }
}
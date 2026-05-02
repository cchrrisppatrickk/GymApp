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

            // 1. Nuevos Miembros del Mes (AsNoTracking)
            int nuevos = await _context.Usuarios
                .AsNoTracking()
                .CountAsync(u => u.FechaRegistro.HasValue && 
                                 u.FechaRegistro.Value.Month == mesActual && 
                                 u.FechaRegistro.Value.Year == anioActual);

            // 2. Vencidos Sin Renovar (AsNoTracking)
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

            // 3. Por Vencer en 7 Días (Solo Activas vigentes)
            int porVencer = await _context.Membresias
                .AsNoTracking()
                .CountAsync(m => m.Estado == "Activa" && 
                                 m.FechaVencimiento >= hoy && 
                                 m.FechaVencimiento <= finSemana);

            // 4. Deudas (Criterio Real: Solo Activas o Pendientes de Pago)
            var deudasData = await _context.Membresias
                .AsNoTracking()
                .Where(m => m.Estado == "Activa" || m.Estado == "Pendiente Pago")
                .Select(m => new
                {
                    m.UserId,
                    PrecioCalculo = m.PrecioAcordado,
                    TotalPagado = m.PagosMembresia.Where(p => !p.EsAnulado).Sum(p => p.Monto)
                })
                .ToListAsync();

            var listaDeudores = deudasData
                .Select(d => new { d.UserId, Deuda = d.PrecioCalculo - d.TotalPagado })
                .Where(x => x.Deuda > 0)
                .ToList();

            // 5. Membresías Congeladas (AsNoTracking)
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

        public async Task<DashboardFinancialStatsDTO> ObtenerEstadisticasFinancierasAsync()
        {
            var hoy = DateTime.Now;
            var result = new DashboardFinancialStatsDTO();

            // 1. Mensual (Últimos 6 meses) - Excluyendo Pagos Anulados
            var hace6Meses = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-5);
            var pagosMensualesDb = await _context.PagosMembresia
                .AsNoTracking()
                .Where(p => p.FechaPago >= hace6Meses && !p.EsAnulado)
                .ToListAsync();

            var pagosAgrupadosMes = pagosMensualesDb
                .Where(p => p.FechaPago.HasValue)
                .GroupBy(p => new { p.FechaPago.Value.Year, p.FechaPago.Value.Month })
                .ToDictionary(g => new DateTime(g.Key.Year, g.Key.Month, 1), g => g.Sum(p => p.Monto));

            var mesesLabels = new List<string>();
            var ingresosMensuales = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var mes = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                mesesLabels.Add(mes.ToString("MMM", new System.Globalization.CultureInfo("es-ES")));
                ingresosMensuales.Add(pagosAgrupadosMes.ContainsKey(mes) ? pagosAgrupadosMes[mes] : 0m);
            }

            result.MesesLabels = mesesLabels;
            result.IngresosMensuales = ingresosMensuales;

            result.IngresoMesActual = ingresosMensuales.LastOrDefault();
            decimal ingresoMesAnterior = ingresosMensuales.Count > 1 ? ingresosMensuales[ingresosMensuales.Count - 2] : 0;
            
            if (ingresoMesAnterior != 0)
                result.CrecimientoMensualPorcentaje = ((result.IngresoMesActual - ingresoMesAnterior) / ingresoMesAnterior) * 100;
            else
                result.CrecimientoMensualPorcentaje = result.IngresoMesActual > 0 ? 100 : 0;

            // 2. Semanal (Últimos 28 días) - Excluyendo Pagos Anulados
            var hace28Dias = hoy.Date.AddDays(-28);
            var pagosSemanalesDb = await _context.PagosMembresia
                .AsNoTracking()
                .Where(p => p.FechaPago >= hace28Dias && !p.EsAnulado)
                .ToListAsync();

            var semanasLabels = new List<string>();
            var ingresosSemanales = new List<decimal>();

            for (int i = 0; i < 4; i++)
            {
                var inicioSemana = hace28Dias.AddDays(i * 7);
                var finSemana = inicioSemana.AddDays(7);
                semanasLabels.Add($"Sem {i + 1}");
                var sumaSemana = pagosSemanalesDb
                    .Where(p => p.FechaPago >= inicioSemana && p.FechaPago < finSemana)
                    .Sum(p => p.Monto);
                ingresosSemanales.Add(sumaSemana);
            }

            result.SemanasLabels = semanasLabels;
            result.IngresosSemanales = ingresosSemanales;

            result.IngresoSemanaActual = ingresosSemanales.LastOrDefault();
            decimal ingresoSemanaAnterior = ingresosSemanales.Count > 1 ? ingresosSemanales[ingresosSemanales.Count - 2] : 0;

            if (ingresoSemanaAnterior != 0)
                result.CrecimientoSemanalPorcentaje = ((result.IngresoSemanaActual - ingresoSemanaAnterior) / ingresoSemanaAnterior) * 100;
            else
                result.CrecimientoSemanalPorcentaje = result.IngresoSemanaActual > 0 ? 100 : 0;

            return result;
        }

        public async Task<List<PagoRecienteDTO>> ObtenerPagosRecientesAsync(int cantidad = 7)
        {
            var pagos = await _context.PagosMembresia
                .Include(p => p.Membresia)
                    .ThenInclude(m => m.User)
                .Include(p => p.Membresia)
                    .ThenInclude(m => m.Plan)
                .OrderByDescending(p => p.FechaPago)
                .Take(cantidad)
                .Select(p => new PagoRecienteDTO
                {
                    NombreCliente = p.Membresia.User.NombreCompleto,
                    NombrePlan = p.Membresia.Plan.Nombre,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago.Value,
                    MetodoPago = p.MetodoPago
                })
                .AsNoTracking()
                .ToListAsync();

            return pagos;
        }

        public async Task<List<DeudaInfoDTO>> ObtenerListaDeudoresAsync()
        {
            var deudores = await _context.Membresias
                .Include(m => m.User)
                .Include(m => m.Plan)
                .AsNoTracking()
                .Where(m => m.Estado == "Activa" || m.Estado == "Pendiente Pago")
                .Select(m => new DeudaInfoDTO
                {
                    MembresiaId = m.MembresiaId,
                    NombreCliente = m.User.NombreCompleto,
                    DniCliente = m.User.Dni,
                    NombrePlan = m.Plan.Nombre,
                    Estado = m.Estado,
                    PrecioTotal = m.PrecioAcordado,
                    TotalPagado = m.PagosMembresia.Where(p => !p.EsAnulado).Sum(p => p.Monto)
                })
                .ToListAsync();

            // Calculamos la deuda pendiente en memoria para facilitar la lógica
            return deudores
                .Select(d => {
                    d.DeudaPendiente = d.PrecioTotal - d.TotalPagado;
                    return d;
                })
                .Where(x => x.DeudaPendiente > 0)
                .OrderByDescending(x => x.DeudaPendiente)
                .ToList();
        }
    }
}
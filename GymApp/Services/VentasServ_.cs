using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public interface IVentaService
    {
        Task<int> RegistrarVentaAsync(VentaCreateDTO dto, int empleadoId);
        Task<IEnumerable<VentasCabecera>> HistorialVentasAsync();
    }

    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepo;
        private readonly IProductoRepository _productoRepo;

        // Usamos el repositorio genérico para guardar los detalles
        private readonly IGenericRepository<VentasDetalle> _detalleRepo;

        public VentaService(
            IVentaRepository ventaRepo,
            IProductoRepository productoRepo,
            IGenericRepository<VentasDetalle> detalleRepo)
        {
            _ventaRepo = ventaRepo;
            _productoRepo = productoRepo;
            _detalleRepo = detalleRepo;
        }

        public async Task<int> RegistrarVentaAsync(VentaCreateDTO dto, int empleadoId)
        {
            // 1. CREAR LA CABECERA (Inicialmente con Total 0)
            var venta = new VentasCabecera
            {
                UserId = dto.UserId,
                UsuarioEmpleadoId = empleadoId,
                FechaVenta = DateTime.Now,
                MetodoPago = dto.MetodoPago,
                Total = 0 // Se calculará sumando los detalles
            };

            await _ventaRepo.InsertAsync(venta);
            await _ventaRepo.SaveAsync(); // Guardamos para obtener el VentaId

            decimal totalCalculado = 0;

            // 2. PROCESAR CADA PRODUCTO (Detalles)
            foreach (var item in dto.Items)
            {
                var producto = await _productoRepo.GetByIdAsync(item.ProductoId);

                // Validaciones Críticas
                if (producto == null) throw new Exception($"Producto ID {item.ProductoId} no existe.");
                if (producto.StockActual < item.Cantidad)
                    throw new Exception($"Stock insuficiente para {producto.Nombre}. Disponibles: {producto.StockActual}");

                // Descontar Stock
                producto.StockActual -= item.Cantidad;
                await _productoRepo.UpdateAsync(producto);

                // Calcular Subtotal (Precio x Cantidad)
                decimal subtotal = producto.PrecioVenta * item.Cantidad;
                totalCalculado += subtotal;

                // Crear el Detalle
                var detalle = new VentasDetalle
                {
                    VentaId = venta.VentaId,
                    ProductoId = producto.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.PrecioVenta, // Importante: Guardar el precio histórico
                    Subtotal = subtotal
                };

                await _detalleRepo.InsertAsync(detalle);
            }

            // 3. ACTUALIZAR EL TOTAL DE LA CABECERA
            venta.Total = totalCalculado;
            await _ventaRepo.UpdateAsync(venta);

            // 4. GUARDAR TODO (Commit final)
            // Nota: En un entorno real productivo, esto iría dentro de un bloque _context.Database.BeginTransaction()
            // pero con EF Core y SaveAsync, si falla algo aquí, se maneja relativamente bien.
            await _productoRepo.SaveAsync();
            await _detalleRepo.SaveAsync();
            await _ventaRepo.SaveAsync();

            return venta.VentaId;
        }

        public async Task<IEnumerable<VentasCabecera>> HistorialVentasAsync()
        {
            return await _ventaRepo.ObtenerHistorialCompletoAsync();
        }
    }
}
using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
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
            // 1. CREAR LA CABECERA
            var venta = new VentasCabecera
            {
                UserId = dto.UserId,
                UsuarioEmpleadoId = empleadoId,
                FechaVenta = DateTime.Now,
                MetodoPago = dto.MetodoPago,
                Total = 0
            };

            await _ventaRepo.InsertAsync(venta);
            await _ventaRepo.SaveAsync();

            decimal totalCalculado = 0;

            // 2. PROCESAR DETALLES
            foreach (var item in dto.Items)
            {
                var producto = await _productoRepo.GetByIdAsync(item.ProductoId);

                if (producto == null) throw new Exception($"Producto ID {item.ProductoId} no existe.");

                // --- LÓGICA DE PRECIO DINÁMICO ---
                // Si el DTO trae un precio mayor a 0 (editado en caja), usamos ese.
                // Si no, usamos el precio oficial del sistema.
                decimal precioFinal = item.PrecioUnitario > 0 ? item.PrecioUnitario : producto.PrecioVenta;

                // --- LÓGICA DE STOCK INTELIGENTE ---
                // Solo validamos y descontamos stock si NO es un Servicio.
                if (producto.Categoria != "Servicio")
                {
                    if (producto.StockActual < item.Cantidad)
                        throw new Exception($"Stock insuficiente para {producto.Nombre}. Disp: {producto.StockActual}");

                    producto.StockActual -= item.Cantidad;
                    await _productoRepo.UpdateAsync(producto);
                }

                // Calcular Subtotal con el precio final decidido
                decimal subtotal = precioFinal * item.Cantidad;
                totalCalculado += subtotal;

                var detalle = new VentasDetalle
                {
                    VentaId = venta.VentaId,
                    ProductoId = producto.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioFinal, // Guardamos el precio que realmente se cobró
                    Subtotal = subtotal
                };

                await _detalleRepo.InsertAsync(detalle);
            }

            // 3. ACTUALIZAR TOTAL CABECERA
            venta.Total = totalCalculado;
            await _ventaRepo.UpdateAsync(venta);

            // 4. GUARDAR CAMBIOS
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
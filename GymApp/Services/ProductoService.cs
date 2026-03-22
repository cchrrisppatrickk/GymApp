using GymApp.Models;
using GymApp.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepo;

        public ProductoService(IProductoRepository productoRepo)
        {
            _productoRepo = productoRepo;
        }

        public async Task<IEnumerable<Producto>> ListarProductosAsync()
        {
            return await _productoRepo.GetAllAsync();
        }

        public async Task<Producto> ObtenerPorIdAsync(int id)
        {
            return await _productoRepo.GetByIdAsync(id);
        }

        public async Task CrearProductoAsync(Producto producto)
        {
            ValidarProducto(producto);
            // Si es nuevo, aseguramos que stock no sea null, mínimo 0
            if (producto.StockActual == null) producto.StockActual = 0;

            await _productoRepo.InsertAsync(producto);
            await _productoRepo.SaveAsync();
        }

        public async Task ActualizarProductoAsync(Producto producto)
        {
            ValidarProducto(producto);
            await _productoRepo.UpdateAsync(producto);
            await _productoRepo.SaveAsync();
        }

        public async Task EliminarProductoAsync(int id)
        {
            // OJO: En el futuro, aquí validaremos que el producto no tenga ventas asociadas
            // antes de borrarlo para no romper la integridad referencial.
            await _productoRepo.DeleteAsync(id);
            await _productoRepo.SaveAsync();
        }

        private void ValidarProducto(Producto p)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.");

            if (p.PrecioVenta < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            if (p.StockActual < 0)
                throw new ArgumentException("El stock no puede ser negativo.");
        }
    }
}
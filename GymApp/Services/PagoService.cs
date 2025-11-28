using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepo;
        private readonly IMembresiaRepository _membresiaRepo;
        private readonly IUsuarioRepository _usuarioRepo; // Necesitamos buscar user por DNI

        public PagoService(IPagoRepository pagoRepo, IMembresiaRepository membresiaRepo, IUsuarioRepository usuarioRepo)
        {
            _pagoRepo = pagoRepo;
            _membresiaRepo = membresiaRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<DeudaInfoDTO> BuscarMembresiaPorDniAsync(string dni)
        {
            var usuario = await _usuarioRepo.ObtenerPorDNIAsync(dni);
            if (usuario == null) throw new Exception("Usuario no encontrado");

            // 1. Buscamos la última membresía (puede ser la que acaban de crear "Pendiente" o una "Vencida" con deuda)
            var membresia = await _membresiaRepo.GetLastActiveMembresiaByUserIdAsync(usuario.UserId);

            // Si no tiene activa, buscamos la última creada en general (para pagar una recién creada)
            if (membresia == null)
            {
                var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
                membresia = todas.Where(m => m.UserId == usuario.UserId)
                                 .OrderByDescending(m => m.MembresiaId)
                                 .FirstOrDefault();
            }

            if (membresia == null) throw new Exception("El usuario no tiene ninguna membresía generada para cobrar.");

            // 2. CALCULAR MATEMÁTICAS FINANCIERAS
            decimal precioPlan = membresia.Plan.PrecioBase; // Asumiendo que Plan tiene PrecioBase
            decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(membresia.MembresiaId);
            decimal deuda = precioPlan - yaPagado;

            // Si la deuda es 0 o menor, significa que está al día. 
            // Podrías lanzar error o mostrar que "No debe nada".

            return new DeudaInfoDTO
            {
                MembresiaId = membresia.MembresiaId,
                Cliente = usuario.NombreCompleto,
                Plan = membresia.Plan.Nombre,
                Estado = membresia.Estado,
                // Info Financiera
                PrecioTotal = precioPlan,
                TotalPagado = yaPagado,
                DeudaPendiente = deuda > 0 ? deuda : 0
            };
        }

        public async Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId)
        {
            // 1. Validaciones
            if (dto.Monto <= 0) throw new Exception("El monto debe ser mayor a 0");

            var infoDeuda = await BuscarMembresiaPorDniAsync(dto.DniCliente);

            if (dto.Monto > infoDeuda.DeudaPendiente)
                throw new Exception($"El monto excede la deuda. Solo debe: {infoDeuda.DeudaPendiente:C}");

            // 2. Registrar el Pago (Inmutable)
            var nuevoPago = new PagosMembresium
            {
                MembresiaId = infoDeuda.MembresiaId,
                UsuarioEmpleadoId = empleadoId,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                FechaPago = DateTime.Now,
                Comprobante = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            await _pagoRepo.InsertAsync(nuevoPago);

            // 3. ACTUALIZACIÓN DE ESTADO (El paso crucial que faltaba)
            // Verificamos si con este pago se salda la deuda
            decimal nuevoTotalPagado = infoDeuda.TotalPagado + dto.Monto;

            if (nuevoTotalPagado >= infoDeuda.PrecioTotal)
            {
                // Recuperamos la entidad Membresía para editarla
                var membresiaEntity = await _membresiaRepo.GetByIdAsync(infoDeuda.MembresiaId);

                if (membresiaEntity.Estado == "Pendiente Pago") // Solo si estaba pendiente
                {
                    membresiaEntity.Estado = "Activa";
                    await _membresiaRepo.UpdateAsync(membresiaEntity);
                }
            }

            // 4. Guardar todo (Pago + Actualización de Membresía si hubo) en una transacción
            await _pagoRepo.SaveAsync();

            return nuevoPago.PagoId;
        }

        public async Task<IEnumerable<PagoListDTO>> ListarPagosAsync()
        {
            var pagos = await _pagoRepo.ObtenerHistorialCompletoAsync();
            return pagos.Select(p => new PagoListDTO
            {
                PagoId = p.PagoId,
                NombreCliente = p.Membresia.User.NombreCompleto,
                NombrePlan = p.Membresia.Plan.Nombre,
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                FechaPago = p.FechaPago?.ToString("g"), // Formato general fecha+hora
                NombreEmpleado = p.UsuarioEmpleado.NombreCompleto
            });
        }
    }
}
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

            // 1. Intentamos buscar la última activa/pendiente con datos cargados
            var membresia = await _membresiaRepo.GetLastActiveMembresiaByUserIdAsync(usuario.UserId);

            // 2. Fallback: Si no tiene activa (ej. está vencida y viene a pagar deuda vieja), buscamos la última histórica
            if (membresia == null)
            {
                var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
                membresia = todas.Where(m => m.UserId == usuario.UserId)
                                    .OrderByDescending(m => m.MembresiaId)
                                    .FirstOrDefault();
            }

            if (membresia == null) throw new Exception("El usuario no tiene ninguna membresía generada para cobrar.");

            // VALIDACIÓN DE SEGURIDAD (Evita el crash si el Plan sigue siendo nulo por alguna razón rara)
            if (membresia.Plan == null)
            {
                // Esto fuerza la carga si falló el Include (parche de emergencia)
                // Pero con el PASO A esto no debería ocurrir.
                throw new Exception("Error de datos: La membresía existe pero no tiene un Plan asignado.");
            }

            // 3. CALCULAR MATEMÁTICAS FINANCIERAS
            decimal precioPlan = membresia.Plan.PrecioBase;
            decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(membresia.MembresiaId);
            decimal deuda = precioPlan - yaPagado;

            return new DeudaInfoDTO
            {
                MembresiaId = membresia.MembresiaId,
                Cliente = usuario.NombreCompleto,
                Plan = membresia.Plan.Nombre,
                // Aquí mostramos el estado real de la BD
                Estado = membresia.FechaVencimiento < DateOnly.FromDateTime(DateTime.Now) ? "Vencida" : membresia.Estado,
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
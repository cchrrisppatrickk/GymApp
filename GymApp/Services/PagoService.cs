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

        public async Task<List<DeudaInfoDTO>> BuscarDeudaClienteAsync(string termino)
        {
            var todas = await _membresiaRepo.ObtenerTodasConDetallesAsync();
            var membresias = todas.Where(m => (m.Estado == "Activa" || m.Estado == "Pendiente Pago" || m.Estado == "Vencida") && 
                (string.IsNullOrEmpty(termino) || 
                 m.User.NombreCompleto.Contains(termino, StringComparison.OrdinalIgnoreCase) || 
                 (m.User.Dni != null && m.User.Dni.Contains(termino)))).ToList();

            var resultados = new List<DeudaInfoDTO>();

            foreach(var membresia in membresias)
            {
                if (membresia.Plan == null) continue;

                decimal precioPlan = membresia.Plan.PrecioBase;
                decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(membresia.MembresiaId);
                decimal deuda = precioPlan - yaPagado;

                resultados.Add(new DeudaInfoDTO
                {
                    MembresiaId = membresia.MembresiaId,
                    NombreCliente = membresia.User.NombreCompleto,
                    DniCliente = membresia.User.Dni,
                    NombrePlan = membresia.Plan.Nombre,
                    Estado = membresia.FechaVencimiento < DateOnly.FromDateTime(DateTime.Now) && membresia.Estado != "Pendiente Pago" ? "Vencida" : membresia.Estado,
                    PrecioTotal = precioPlan,
                    TotalPagado = yaPagado,
                    DeudaPendiente = deuda > 0 ? deuda : 0
                });
            }

            return resultados;
        }

        public async Task<int> RegistrarPagoAsync(PagoCreateDTO dto, int empleadoId)
        {
            // 1. Validaciones
            if (dto.Monto <= 0) throw new Exception("El monto debe ser mayor a 0");

            var membresia = await _membresiaRepo.ObtenerPorIdConDetallesAsync(dto.MembresiaId);
            if (membresia == null || membresia.Plan == null) throw new Exception("Membresía no encontrada o no tiene un plan válido asociado.");

            decimal precioPlan = membresia.Plan.PrecioBase;
            decimal yaPagado = await _pagoRepo.GetTotalPagadoAsync(dto.MembresiaId);
            decimal deudaPendiente = precioPlan - yaPagado;

            if (dto.Monto > deudaPendiente)
                throw new Exception($"El monto excede la deuda. Solo debe: {deudaPendiente:C}");

            // 2. Registrar el Pago (Inmutable)
            var nuevoPago = new PagosMembresium
            {
                MembresiaId = dto.MembresiaId,
                UsuarioEmpleadoId = empleadoId,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                FechaPago = DateTime.Now,
                Comprobante = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            await _pagoRepo.InsertAsync(nuevoPago);

            // 3. ACTUALIZACIÓN DE ESTADO (El paso crucial que faltaba)
            // Verificamos si con este pago se salda la deuda
            decimal nuevoTotalPagado = yaPagado + dto.Monto;

            if (nuevoTotalPagado >= precioPlan)
            {
                if (membresia.Estado == "Pendiente Pago") // Solo si estaba pendiente
                {
                    membresia.Estado = "Activa";
                    await _membresiaRepo.UpdateAsync(membresia);
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